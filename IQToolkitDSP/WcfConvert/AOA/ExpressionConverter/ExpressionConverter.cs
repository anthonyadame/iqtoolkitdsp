
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace IQToolkitDSP
{

    internal abstract class ExpressionConverter : ResourceAnnotationPreservingExpressionVisitor
    {
        private ExpressionConversionResult result;
        private DSPMetadata metadataMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionConverter"/> class.
        /// </summary>
        /// <param name="annotations">The annotations.</param>
        /// <param name="metadataMapping">The metadata mapping.</param>
        public ExpressionConverter(ExpressionResourceAnnotations annotations, DSPMetadata metadataMapping)
            : base(annotations)
        {
            Debug.Assert(metadataMapping != null, "metadataMapping != null");
            this.metadataMapping = metadataMapping;
        }

        /// <summary>
        /// Converts the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="metadataMapping">The metadata mapping.</param>
        /// <returns></returns>
        public static ExpressionConversionResult Convert(Expression expression, ExpressionResourceAnnotations annotations, DSPMetadata metadataMapping)
        {
            Debug.Assert(annotations != null, "annotations != null");
            Debug.Assert(metadataMapping != null, "metadataMapping != null");

            expression = ResourceAnnotationVisitor.Convert(expression, annotations);

            ExpressionConversionResult result = ConvertInnerExpression(expression, annotations, metadataMapping);

            expression = ExpressionSimplifier.Simplify(result.Expression);

            result.SetResultExpression(expression);
            return result;
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            return base.VisitMethodCall(m);
        }

        /// <summary>
        /// Converts the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        protected ExpressionConversionResult Convert(ExpressionConversionResult source)
        {
            this.result = new ExpressionConversionResult(source);
            Expression expression = this.Visit(source.Expression);
            this.result.SetResultExpression(expression);
            return this.result;
        }

        /// <summary>
        /// Visits the method call skip projections.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected Expression VisitMethodCallSkipProjections(MethodCallExpression m)
        {
            var selectMatch = ExpressionUtil.MatchSelectCall(m);
            if (selectMatch != null)
            {
                Expression source = this.Visit(selectMatch.Source);
                return this.Annotations.PropagateResourceType(
                    selectMatch.Lambda,
                    Expression.Call(selectMatch.MethodCall.Method, source, selectMatch.Lambda));
            }
            else
            {
                return base.VisitMethodCall(m);
            }
        }

        /// <summary>
        /// Converts the inner expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="contextMapping">The context mapping.</param>
        /// <returns></returns>
        private static ExpressionConversionResult ConvertInnerExpression(
            Expression expression,
            ExpressionResourceAnnotations annotations,
            DSPMetadata contextMapping)
        {
            Debug.Assert(expression != null, "expression != null");
            Debug.Assert(annotations != null, "annotations != null");
            Debug.Assert(contextMapping != null, "contextMapping != null");

            ExpressionConversionResult result = new ExpressionConversionResult(expression);

            FilterConverter filterConverter = new FilterConverter(annotations, contextMapping);
            result = filterConverter.Convert(result);

            return result;
        }
    }
}
