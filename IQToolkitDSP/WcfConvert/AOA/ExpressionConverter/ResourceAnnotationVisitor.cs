// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQToolkitDSP
{

    /// <summary>Visitor which annotates expressions with resource metadata. It also changes the expressions from using the DataServiceProviderMethods
    /// way to access properties and such to the direct CLR property access.</summary>
    internal class ResourceAnnotationVisitor : ResourceAnnotationPreservingExpressionVisitor
    {
        /// <summary>MethodInfo for object DataServiceProviderMethods.GetValue(this object value, string propertyName).</summary>
        internal static readonly MethodInfo GetValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "GetValue",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(object), typeof(ResourceProperty) },
            null);

        /// <summary>MethodInfo for IEnumerable&lt;T&gt; DataServiceProviderMethods.GetSequenceValue(this object value, string propertyName).</summary>
        internal static readonly MethodInfo GetSequenceValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "GetSequenceValue",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(object), typeof(ResourceProperty) },
            null);

        /// <summary>MethodInfo for Convert.</summary>
        internal static readonly MethodInfo ConvertMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "Convert",
            BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for TypeIs.</summary>
        internal static readonly MethodInfo TypeIsMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "TypeIs",
            BindingFlags.Static | BindingFlags.Public);

        /// <summary>Constructor.</summary>
        /// <param name="annotations">Annotations to use and modify.</param>
        private ResourceAnnotationVisitor(ExpressionResourceAnnotations annotations)
            : base(annotations)
        {
        }

        /// <summary>Converts the specified expression and annotates it.</summary>
        /// <param name="expr">The expression to process.</param>
        /// <param name="annotations">Annotations object to store the annotations in.</param>
        /// <returns>The converted expression which will contain only CLR way of accessing properties.</returns>
        internal static Expression Convert(Expression expr, ExpressionResourceAnnotations annotations)
        {
            ResourceAnnotationVisitor visitor = new ResourceAnnotationVisitor(annotations);
            return visitor.Visit(expr);
        }

        /// <summary>Visits a method call.</summary>
        /// <param name="m">The method call to visit.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method == GetValueMethodInfo)
            {
                ResourceProperty property = (ResourceProperty)((ConstantExpression)m.Arguments[1]).Value;
                return this.Annotations.AnnotateResourceProperty(
                    Expression.Property(this.Visit(m.Arguments[0]), property.Name),
                    property);
            }

            if (m.Method.IsGenericMethod && m.Method.GetGenericMethodDefinition() == GetSequenceValueMethodInfo)
            {
                ResourceProperty property = (ResourceProperty)((ConstantExpression)m.Arguments[1]).Value;
                return this.Annotations.AnnotateResourceProperty(
                    Expression.Property(this.Visit(m.Arguments[0]), property.Name),
                    property);
            }

            if (m.Method == ConvertMethodInfo)
            {
                ResourceType type = (ResourceType)((ConstantExpression)m.Arguments[1]).Value;
                return this.Annotations.AnnotateResourceType(
                    Expression.Convert(this.Visit(m.Arguments[0]), type.InstanceType),
                    type);
            }

            if (m.Method == TypeIsMethodInfo)
            {
                ResourceType type = (ResourceType)((ConstantExpression)m.Arguments[1]).Value;
                return Expression.TypeIs(
                    this.Visit(m.Arguments[0]),
                    type.InstanceType);
            }

            var selectMatch = ExpressionUtil.MatchSelectCall(m);
            if (selectMatch != null)
            {
                // Annotate the source first
                Expression source = this.Visit(selectMatch.Source);

                // Annotate the lambda parameter with the source type
                this.Annotations.PropagateResourceType(
                    source,
                    selectMatch.Lambda.Parameters[0]);

                // Now annotate the lambda
                LambdaExpression lambda = (LambdaExpression)this.Visit(selectMatch.Lambda);
                Expression body = lambda.Body;
                return this.Annotations.PropagateResourceType(
                    body,
                    Expression.Call(selectMatch.MethodCall.Method, source, lambda));
            }

            var selectManyMatch = ExpressionUtil.MatchSelectManyCall(m);
            if (selectManyMatch != null)
            {
                // Annotate the lambda parameter with the source type
                this.Annotations.PropagateResourceType(
                    selectManyMatch.Source,
                    selectManyMatch.Lambda.Parameters[0]);
                return base.VisitMethodCall(m);
            }

            var whereMatch = ExpressionUtil.MatchWhereCall(m);
            if (whereMatch != null)
            {
                // Annotate the lambda parameter with the source type
                this.Annotations.PropagateResourceType(
                    whereMatch.Source,
                    whereMatch.Lambda.Parameters[0]);
                return base.VisitMethodCall(m);
            }

            var orderByMatch = ExpressionUtil.MatchOrderByCall(m) ?? ExpressionUtil.MatchThenByCall(m);
            if (orderByMatch != null)
            {
                // Annotate the lambda parameter with the source type
                this.Annotations.PropagateResourceType(
                    orderByMatch.Source,
                    orderByMatch.Lambda.Parameters[0]);
                return base.VisitMethodCall(m);
            }

            return base.VisitMethodCall(m);
        }

        /// <summary>Visits a unary expression.</summary>
        /// <param name="u">The unary expression to visit.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                    {
                        // The input expression contains Convert operation over almost every call to the GetValue or similar methods.
                        // The reason is that these methods return System.Object but the rest of the expression needs the actual type.
                        // So in here we're going to remove the Convert calls after we removed the GetValue calls to make the expression simpler
                        //   and more easy to analyze by the rest of the system.
                        Expression operand = this.Visit(u.Operand);
                        if (operand.Type == u.Type)
                        {
                            // In this case the ResourceType annotation is preserved as the operand should already be annotated as appropriate
                            return operand;
                        }
                        else
                        {
                            return Expression.Convert(operand, u.Type);
                        }
                    }

                default:
                    return base.VisitUnary(u);
            }
        }
    }
}
