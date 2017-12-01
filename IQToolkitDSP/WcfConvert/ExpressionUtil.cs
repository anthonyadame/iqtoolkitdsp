// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQToolkitDSP
{
    /// <summary>Helper methods for handling expressions.</summary>
    internal static class ExpressionUtil
    {
        /// <summary>Returns expression stripped of any Quote expression.</summary>
        /// <param name="expr">The expression to process.</param>
        /// <returns>Expression which is guaranteed not to be a quote expression.</returns>
        internal static Expression RemoveQuotes(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Quote)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            return expr;
        }

        /// <summary>Returns true if the specified <paramref name="expr"/> is a null constant (of any type).</summary>
        /// <param name="expr">The expression to inspect.</param>
        /// <returns>true if the <paramref name="expr"/> is a null constant, or false otherwise.</returns>
        internal static bool IsNullConstant(Expression expr)
        {
            expr = RemoveConversions(expr);
            ConstantExpression constant = expr as ConstantExpression;
            if (constant == null)
            {
                return false;
            }

            if (constant.Value != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>Returns true if the specified <paramref name="expr"/> is a false constant (of type bool).</summary>
        /// <param name="expr">The expression to inspect.</param>
        /// <returns>true if the <paramref name="expr"/> is a false constant, or false otherwise.</returns>
        internal static bool IsFalseConstant(Expression expr)
        {
            expr = RemoveConversions(expr);
            ConstantExpression constant = expr as ConstantExpression;
            if (constant == null || constant.Type != typeof(bool))
            {
                return false;
            }

            if ((bool)constant.Value != false)
            {
                return false;
            }

            return true;
        }

        /// <summary>Removes any Convert expressions.</summary>
        /// <param name="expr">The expression to process.</param>
        /// <returns>Expression which is guaranteed not to be a conversion expression.</returns>
        /// <remarks>Note that the returned expression can be of a different type and thus should not be used
        /// as a replacement for the input.</remarks>
        internal static Expression RemoveConversions(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            return expr;
        }

        /// <summary>Removes any Convert and TypeAs expression.</summary>
        /// <param name="expr">The expression to process.</param>
        /// <returns>Expression which is guaranteed not to be a conversion or type as expression.</returns>
        /// <remarks>Note that the returned expression can be of a different type and thus should not be used
        /// as a replacement for the input.</remarks>
        internal static Expression RemoveConversionsAndTypeAs(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert || expr.NodeType == ExpressionType.TypeAs)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            return expr;
        }

        /// <summary>Determines if the <paramref name="expr"/> is a conditional comparing to null.</summary>
        /// <param name="expr">The expression to inspect.</param>
        /// <returns>Instance of <see cref="NullEqualConditionalMatch"/> if <paramref name="expr"/> is a conditional comparing to null,
        /// or null otherwise.</returns>
        internal static NullEqualConditionalMatch MatchNullEqualConditional(Expression expr)
        {
            ConditionalExpression conditional = expr as ConditionalExpression;
            if (conditional == null)
            {
                return null;
            }

            Expression test = conditional.Test;
            BinaryExpression comparison = test as BinaryExpression;
            if (comparison == null || comparison.NodeType != ExpressionType.Equal)
            {
                return null;
            }

            var result = new NullEqualConditionalMatch();

            if (IsNullConstant(comparison.Left))
            {
                result.ComparisonOperand = comparison.Right;
            }
            else if (IsNullConstant(comparison.Right))
            {
                result.ComparisonOperand = comparison.Left;
            }
            else
            {
                return null;
            }

            result.IfNull = conditional.IfTrue;
            result.IfNotNull = conditional.IfFalse;
            return result;
        }

        /// <summary>
        /// Gets the member access.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        /// <summary>Determines if the specified <paramref name="expr"/> is a member access expression and returns the member being accessed.</summary>
        /// <param name="expr">The expression to inspect.</param>
        /// <returns>The member being accessed by the <paramref name="expr"/> or null otherwise.</returns>
        internal static MemberInfo GetMemberAccess(Expression expr)
        {
            MemberExpression member = expr as MemberExpression;
            return member == null ? null : member.Member;
        }

        /// <summary>Determines if the specified <paramref name="expr"/> is expression accessing member with name <paramref name="memberName"/>.</summary>
        /// <param name="expr">The expression to inspect.</param>
        /// <param name="memberName">Name of the member being accessed.</param>
        /// <returns>True if the <paramref name="expr"/> is a member access to a member with <paramref name="memberName"/> name.</returns>
        internal static bool IsMemberAccess(Expression expr, string memberName)
        {
            MemberInfo member = GetMemberAccess(expr);
            return member != null && member.Name == memberName;
        }

        /// <summary>Determines if the <paramref name="expr"/> is a call to Select.</summary>
        /// <param name="expr">Expression to inspect.</param>
        /// <returns>Instance of the <see cref="SelectCallMatch"/> class if the <paramref name="expr"/> is a Select call,
        /// or null otherwise.</returns>
        internal static SelectCallMatch MatchSelectCall(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Call && TypeSystem.IsMethodLinqSelect(((MethodCallExpression)expr).Method))
            {
                MethodCallExpression call = (MethodCallExpression)expr;
                LambdaExpression lambda = (LambdaExpression)ExpressionUtil.RemoveQuotes(call.Arguments[1]);
                Expression body = ExpressionUtil.RemoveQuotes(lambda.Body);
                return new SelectCallMatch
                {
                    MethodCall = call,
                    Source = call.Arguments[0],
                    Lambda = lambda,
                    LambdaBody = body
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>Determines if the <paramref name="expr"/> is a call to SelectMany.</summary>
        /// <param name="expr">Expression to inspect.</param>
        /// <returns>Instance of the <see cref="SelectManyCallMatch"/> class if the <paramref name="expr"/> is a SelectMany call,
        /// or null otherwise.</returns>
        internal static SelectManyCallMatch MatchSelectManyCall(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Call && TypeSystem.IsMethodLinqSelectMany(((MethodCallExpression)expr).Method))
            {
                MethodCallExpression call = (MethodCallExpression)expr;
                LambdaExpression lambda = (LambdaExpression)ExpressionUtil.RemoveQuotes(call.Arguments[1]);
                Expression body = ExpressionUtil.RemoveQuotes(lambda.Body);
                return new SelectManyCallMatch
                {
                    MethodCall = call,
                    Source = call.Arguments[0],
                    Lambda = lambda,
                    LambdaBody = body
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>Determines if the <paramref name="expr"/> is a call to Where.</summary>
        /// <param name="expr">Expression to inspect.</param>
        /// <returns>Instance of the <see cref="WhereCallMatch"/> class if the <paramref name="expr"/> is a Where call,
        /// or null otherwise.</returns>
        internal static WhereCallMatch MatchWhereCall(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Call && TypeSystem.IsMethodLinqWhere(((MethodCallExpression)expr).Method))
            {
                MethodCallExpression call = (MethodCallExpression)expr;
                LambdaExpression lambda = (LambdaExpression)ExpressionUtil.RemoveQuotes(call.Arguments[1]);
                Expression body = ExpressionUtil.RemoveQuotes(lambda.Body);
                return new WhereCallMatch
                {
                    MethodCall = call,
                    Source = call.Arguments[0],
                    Lambda = lambda,
                    LambdaBody = body
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>Determines if the <paramref name="expr"/> is a call to OrderBy.</summary>
        /// <param name="expr">Expression to inspect.</param>
        /// <returns>Instance of the <see cref="OrderByCallMatch"/> class if the <paramref name="expr"/> is a OrderBy call,
        /// or null otherwise.</returns>
        internal static OrderByCallMatch MatchOrderByCall(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Call && TypeSystem.IsMethodLinqOrderBy(((MethodCallExpression)expr).Method))
            {
                MethodCallExpression call = (MethodCallExpression)expr;
                LambdaExpression lambda = (LambdaExpression)ExpressionUtil.RemoveQuotes(call.Arguments[1]);
                Expression body = ExpressionUtil.RemoveQuotes(lambda.Body);
                return new OrderByCallMatch
                {
                    MethodCall = call,
                    Source = call.Arguments[0],
                    Lambda = lambda,
                    LambdaBody = body
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>Determines if the <paramref name="expr"/> is a call to ThenBy.</summary>
        /// <param name="expr">Expression to inspect.</param>
        /// <returns>Instance of the <see cref="OrderByCallMatch"/> class if the <paramref name="expr"/> is a ThenBy call,
        /// or null otherwise.</returns>
        internal static OrderByCallMatch MatchThenByCall(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Call && TypeSystem.IsMethodLinqThenBy(((MethodCallExpression)expr).Method))
            {
                MethodCallExpression call = (MethodCallExpression)expr;
                LambdaExpression lambda = (LambdaExpression)ExpressionUtil.RemoveQuotes(call.Arguments[1]);
                Expression body = ExpressionUtil.RemoveQuotes(lambda.Body);
                return new OrderByCallMatch
                {
                    MethodCall = call,
                    Source = call.Arguments[0],
                    Lambda = lambda,
                    LambdaBody = body
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>Match result for null equal conditional.</summary>
        public class NullEqualConditionalMatch
        {
            /// <summary>The operand being compared to null.</summary>
            public Expression ComparisonOperand { get; set; }

            /// <summary>Expression evaluated if <see cref="ComparisonOperand"/> is null.</summary>
            public Expression IfNull { get; set; }

            /// <summary>Expression evaluated if <see cref="ComparisonOperand"/> is not null.</summary>
            public Expression IfNotNull { get; set; }
        }

        /// <summary>Match result for a SelectCall</summary>
        public class SelectCallMatch
        {
            /// <summary>The method call expression represented by this match.</summary>
            public MethodCallExpression MethodCall { get; set; }

            /// <summary>The expression on which the Select is being called.</summary>
            public Expression Source { get; set; }

            /// <summary>The lambda expression being executed by the Select.</summary>
            public LambdaExpression Lambda { get; set; }

            /// <summary>The body of the lambda expression.</summary>
            public Expression LambdaBody { get; set; }
        }

        /// <summary>Match result for a SelectCall</summary>
        public class SelectManyCallMatch
        {
            /// <summary>The method call expression represented by this match.</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Not yet used.")]
            public MethodCallExpression MethodCall { get; set; }

            /// <summary>The expression on which the Select is being called.</summary>
            public Expression Source { get; set; }

            /// <summary>The lambda expression being executed by the Select.</summary>
            public LambdaExpression Lambda { get; set; }

            /// <summary>The body of the lambda expression.</summary>
            public Expression LambdaBody { get; set; }
        }

        /// <summary>Match result for a SelectCall</summary>
        public class WhereCallMatch
        {
            /// <summary>The method call expression represented by this match.</summary>
            public MethodCallExpression MethodCall { get; set; }

            /// <summary>The expression on which the Select is being called.</summary>
            public Expression Source { get; set; }

            /// <summary>The lambda expression being executed by the Select.</summary>
            public LambdaExpression Lambda { get; set; }

            /// <summary>The body of the lambda expression.</summary>
            public Expression LambdaBody { get; set; }
        }

        /// <summary>Match result for a OrderByCall and ThenByCall</summary>
        public class OrderByCallMatch
        {
            /// <summary>The method call expression represented by this match.</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Not yet used.")]
            public MethodCallExpression MethodCall { get; set; }

            /// <summary>The expression on which the Select is being called.</summary>
            public Expression Source { get; set; }

            /// <summary>The lambda expression being executed by the Select.</summary>
            public LambdaExpression Lambda { get; set; }

            /// <summary>The body of the lambda expression.</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Not yet used.")]
            public Expression LambdaBody { get; set; }
        }
    }
}