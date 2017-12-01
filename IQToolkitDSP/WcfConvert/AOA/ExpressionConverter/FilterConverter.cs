
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace IQToolkitDSP
{

    internal class FilterConverter : ExpressionConverter
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterConverter"/> class.
        /// </summary>
        /// <param name="annotations">The annotations.</param>
        /// <param name="contextMapping">The context mapping.</param>
        public FilterConverter(ExpressionResourceAnnotations annotations, DSPMetadata contextMapping)
            : base(annotations, contextMapping)
        {
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            var whereMatch = ExpressionUtil.MatchWhereCall(m);
            if (whereMatch != null)
            {
                Expression source = this.Visit(whereMatch.Source);
                LambdaExpression lambda = Expression.Lambda(
                    OptimizeFilterNullableBooleanToCondition(this.Visit(whereMatch.LambdaBody)),
                    whereMatch.Lambda.Parameters.ToArray());

                return Expression.Call(
                    whereMatch.MethodCall.Method,
                    source,
                    lambda);
            }

            return VisitMethodCallSkipProjections(m);
        }

        /// <summary>
        /// ConditionalExpression visit method
        /// </summary>
        /// <param name="c">The ConditionalExpression expression to visit</param>
        /// <returns>
        /// The visited ConditionalExpression expression
        /// </returns>
        internal override Expression VisitConditional(ConditionalExpression c)
        {
            var nullEqualConditionalMatch = ExpressionUtil.MatchNullEqualConditional(c);
            if (nullEqualConditionalMatch != null &&
                nullEqualConditionalMatch.ComparisonOperand.Type == typeof(String))
            {
                if (((MemberExpression)nullEqualConditionalMatch.ComparisonOperand).Member.Name == ((MemberExpression)((MethodCallExpression)((UnaryExpression)nullEqualConditionalMatch.IfNotNull).Operand).Object).Member.Name)
                {
                    return ExpressionUtil.RemoveConversionsAndTypeAs(nullEqualConditionalMatch.IfNotNull);
                }
            }

            return base.VisitConditional(c);
        }

        /// <summary>
        /// BinaryExpression visit method
        /// </summary>
        /// <param name="b">The BinaryExpression expression to visit</param>
        /// <returns>
        /// The visited BinaryExpression expression
        /// </returns>
        internal override Expression VisitBinary(BinaryExpression b)
        {

            if (b.Left.NodeType == ExpressionType.Conditional && ((UnaryExpression)b.Right).Operand.Type == typeof(bool))
            {
                return OptimizeFilterConditionalContains(b);
            }
            else {return base.VisitBinary(b);}
            
        }


        /// <summary>
        /// Visits the unary.
        /// </summary>
        /// <param name="u">The u.</param>
        /// <returns></returns>
        internal override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {        
                case ExpressionType.MemberAccess:
                   
                default:
                    return base.VisitUnary(u);
            }
        }

        /// <summary>
        /// MemberExpression visit method
        /// </summary>
        /// <param name="m">The MemberExpression expression to visit</param>
        /// <returns>
        /// The visited MemberExpression expression
        /// </returns>
        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = this.Visit(m.Expression);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }

            return m;
        }

        /// <summary>
        /// Optimizes the filter nullable boolean to condition.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        private static Expression OptimizeFilterNullableBooleanToCondition(Expression expr)
        {
            if (expr.Type == typeof(bool))
            {
                var nullEqualConditionalMatch = ExpressionUtil.MatchNullEqualConditional(expr);
                if (nullEqualConditionalMatch != null &&
                    nullEqualConditionalMatch.ComparisonOperand.Type == typeof(bool?) &&
                    ExpressionUtil.IsFalseConstant(nullEqualConditionalMatch.IfNull) &&
                    ExpressionUtil.IsMemberAccess(nullEqualConditionalMatch.IfNotNull, "Value"))
                {
                    // (c == null) ? false : c.Value    (bool?)c
                    // =>
                    // c == (bool?)true
                    return Expression.Equal(
                        nullEqualConditionalMatch.ComparisonOperand,
                        Expression.Constant(true, typeof(bool?)));
                }
            }

            return expr;
        }

        /// <summary>
        /// Optimizes the filter conditional contains.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        private static Expression OptimizeFilterConditionalContains(Expression expr)
        {
            if (expr.Type == typeof(bool))
            {
                BinaryExpression comparison = expr as BinaryExpression;
                if (comparison.Left.NodeType == ExpressionType.Conditional)
                {
                    return OptimizeFilterContainsExpressions(comparison.Left);
                }
                
            }

            return expr;
        }

        /// <summary>
        /// Optimizes the filter contains expressions.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        private static Expression OptimizeFilterContainsExpressions(Expression expr)
        {
            var nullEqualConditionalMatch = ExpressionUtil.MatchNullEqualConditional(expr);
            if (nullEqualConditionalMatch != null &&
                nullEqualConditionalMatch.ComparisonOperand.Type == typeof(String))
            {
                if (((MemberExpression)nullEqualConditionalMatch.ComparisonOperand).Member.Name == ((MemberExpression)((MethodCallExpression)((UnaryExpression)nullEqualConditionalMatch.IfNotNull).Operand).Object).Member.Name)
                {
                    return ExpressionUtil.RemoveConversionsAndTypeAs(nullEqualConditionalMatch.IfNotNull);
                }
            }
              
            return expr;
        }

    }
}
