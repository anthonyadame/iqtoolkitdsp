// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace IQToolkitDSP
{

    /// <summary>Visitor which preserves resource annotations. It doesn't modify the expression at all.</summary>
    /// <remarks>Usefull as a base class for other visitors to maintain annotations on the tree.</remarks>
    internal class ResourceAnnotationPreservingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>The expression annotations.</summary>
        private ExpressionResourceAnnotations annotations;

        /// <summary>Constructor.</summary>
        /// <param name="annotations">Annotations to use and modify.</param>
        public ResourceAnnotationPreservingExpressionVisitor(ExpressionResourceAnnotations annotations)
        {
            Debug.Assert(annotations != null, "annotations != null");
            this.annotations = annotations;
        }

        /// <summary>The expression annotations.</summary>
        protected ExpressionResourceAnnotations Annotations
        {
            get { return this.annotations; }
        }

        /// <summary>Visits a method call.</summary>
        /// <param name="m">The method call to visit.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            // Select - we should mark it with ResourceType of the return type of the Func
            var selectMatch = ExpressionUtil.MatchSelectCall(m);
            if (selectMatch != null)
            {
                Expression result = base.VisitMethodCall(m);
                selectMatch = ExpressionUtil.MatchSelectCall(result);
                if (selectMatch != null)
                {
                    return this.annotations.PropagateResourceType(selectMatch.LambdaBody, result);
                }
            }

            // SelectMany - we should mark it with ResourceType of the return type of the Func
            var selectManyMatch = ExpressionUtil.MatchSelectManyCall(m);
            if (selectManyMatch != null)
            {
                Expression result = base.VisitMethodCall(m);
                selectManyMatch = ExpressionUtil.MatchSelectManyCall(result);
                if (selectManyMatch != null)
                {
                    return this.annotations.PropagateResourceType(selectManyMatch.LambdaBody, result);
                }
            }

            // Where - simply propagate the resource type of the source as it doesn't change the results (just filters them)
            var whereMatch = ExpressionUtil.MatchWhereCall(m);
            if (whereMatch != null)
            {
                Expression result = base.VisitMethodCall(m);
                whereMatch = ExpressionUtil.MatchWhereCall(result);
                if (whereMatch != null)
                {
                    return this.annotations.PropagateResourceType(whereMatch.Source, result);
                }
            }

            // OrderBy/ThenBy - simply propagate the resource type of the source as it doesn't change the results (just sorts them)
            var orderByMatch = ExpressionUtil.MatchOrderByCall(m) ?? ExpressionUtil.MatchThenByCall(m);
            if (orderByMatch != null)
            {
                Expression result = base.VisitMethodCall(m);
                orderByMatch = ExpressionUtil.MatchOrderByCall(result) ?? ExpressionUtil.MatchThenByCall(result);
                if (orderByMatch != null)
                {
                    return this.annotations.PropagateResourceType(orderByMatch.Source, result);
                }
            }

            return base.VisitMethodCall(m);
        }

        /// <summary>Visits a lambda expression.</summary>
        /// <param name="lambda">The lambda expression to visit.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            Expression result = lambda;
            if (body != lambda.Body)
            {
                result = Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }

            // The resource type of the whole lambda is the resource type of its body
            return this.annotations.PropagateResourceType(body, result);
        }

        /// <summary>Visits a unary expression.</summary>
        /// <param name="u">The unary expression to visit.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Quote:
                    {
                        // Quote should preserve the resource type annotation
                        Expression operand = this.Visit(u.Operand);
                        return this.annotations.PropagateResourceType(operand, Expression.Quote(operand));
                    }

                default:
                    return base.VisitUnary(u);
            }
        }
    }
}
