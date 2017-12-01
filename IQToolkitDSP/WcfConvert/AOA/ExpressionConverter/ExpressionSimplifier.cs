// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQToolkitDSP
{
	/// <summary>Expression visitor to simlify the expression after some conversions.</summary>
    internal class ExpressionSimplifier : ExpressionVisitor
    {
        /// <summary>Simplifies the specified expression.</summary>
        /// <param name="expr">The expression to simplify.</param>
        /// <returns>The simplified expression.</returns>
        internal static Expression Simplify(Expression expr)
        {
            ExpressionSimplifier visitor = new ExpressionSimplifier();
            return visitor.Visit(expr);
        }

        /// <summary>Visits binary expression.</summary>
        /// <param name="b">The binary expression to visit.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitBinary(BinaryExpression b)
        {
            Expression result;
            result = this.SimplifyNullableToConstantComparison(b);
            if (result != null)
            {
                return result;
            }

            return base.VisitBinary(b);
        }

        /// <summary>Visits method call expression.</summary>
        /// <param name="m">The method call expression.</param>
        /// <returns>The visited expression.</returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Where")
            {
                Expression source = this.Visit(m.Arguments[0]);
                Expression lambdaArg = this.Visit(m.Arguments[1]);
                LambdaExpression lambda = (LambdaExpression)ExpressionUtil.RemoveQuotes(lambdaArg);
                Expression body = ExpressionUtil.RemoveQuotes(lambda.Body);
                Expression newBody;
                newBody = this.SimplifyBoolComparison(body);
                if (newBody != null)
                {
                    return Expression.Call(
                        m.Method,
                        source,
                        Expression.Lambda(newBody, lambda.Parameters.ToArray()));
                }
            }

            return base.VisitMethodCall(m);
        }

        /// <summary>Simplifies expression which comares a nullable value to a non-null constant (can get rid of the nullable conversion).</summary>
        /// <param name="b">The binary expression to simplify.</param>
        /// <returns>The simplified expression.</returns>
        private Expression SimplifyNullableToConstantComparison(BinaryExpression b)
        {
            // Nullable<T>(T)a == Nullable<T>constant => a == constant     iff constant != null
            if (b.NodeType == ExpressionType.Equal && b.Left.Type == b.Right.Type &&
                TypeSystem.GetNullableUnderlyingType(b.Left.Type) != null)
            {
                Expression operand = null;
                ConstantExpression constant = b.Left as ConstantExpression;
                if (constant != null && b.Right.NodeType == ExpressionType.Convert)
                {
                    operand = ((UnaryExpression)b.Right).Operand;
                }
                else
                {
                    constant = b.Right as ConstantExpression;
                    operand = b.Left.NodeType == ExpressionType.Convert ? ((UnaryExpression)b.Left).Operand : null;
                }

                if (constant != null && constant.Value != null &&
                    operand != null && operand.Type == TypeSystem.GetNullableUnderlyingType(constant.Type))
                {
                    return Expression.Equal(
                        this.Visit(operand),
                        Expression.Constant(constant.Value, operand.Type));
                }
            }

            return null;
        }

        /// <summary>Simplifies bool comparison to true. Comparing a boolean value to true is the same as the boolean value itself.</summary>
        /// <param name="expr">The expression to simplify</param>
        /// <returns>The simplified expression.</returns>
        private Expression SimplifyBoolComparison(Expression expr)
        {
            // (bool)a == (bool)true => a
            BinaryExpression b = expr as BinaryExpression;
            if (b != null && b.NodeType == ExpressionType.Equal && b.Left.Type == typeof(bool) && b.Right.Type == typeof(bool))
            {
                Expression operand = null;
                ConstantExpression constant = b.Left as ConstantExpression;
                if (constant != null)
                {
                    operand = b.Right;
                }
                else
                {
                    constant = b.Right as ConstantExpression;
                    operand = b.Left;
                }

                if (constant != null && (bool)constant.Value == true && operand != null)
                {
                    return this.Visit(operand);
                }
            }

            return null;
        }
    }
}