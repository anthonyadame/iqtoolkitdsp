
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace IQToolkitDSP
{
    internal class ExpressionConversionResult
    {
        private Expression expression;
        private IQueryResultsProcessor queryResultsProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionConversionResult"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public ExpressionConversionResult(Expression expression)
        {
            this.SetResultExpression(expression);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionConversionResult"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ExpressionConversionResult(ExpressionConversionResult source)
        {
            this.expression = source.expression;
            this.queryResultsProcessor = source.queryResultsProcessor;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Expression Expression
        {
            get { return this.expression; }
        }

        /// <summary>
        /// Sets the result expression.
        /// </summary>
        /// <param name="expr">The expr.</param>
        public void SetResultExpression(Expression expr)
        {
            Debug.Assert(expr != null, "expression != null");
            this.expression = expr;
        }
    }
}
