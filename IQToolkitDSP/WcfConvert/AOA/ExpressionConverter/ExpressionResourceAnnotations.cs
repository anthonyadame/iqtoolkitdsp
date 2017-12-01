
using System;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.Linq.Expressions;

namespace IQToolkitDSP
{
    internal class ExpressionResourceAnnotations : Annotations<Expression>
    {
        public const string ResourcePropertyAnnotationName = "ResourceProperty";
        public const string ResourceTypeAnnotationName = "ResourceType";

        /// <summary>
        /// Annotates the resource property.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public Expression AnnotateResourceProperty(Expression expr, ResourceProperty property)
        {
            this.SetAnnotation(expr, ResourcePropertyAnnotationName, property);
            this.SetAnnotation(expr, ResourceTypeAnnotationName, property.ResourceType);
            return expr;
        }

        /// <summary>
        /// Annotates the type of the resource.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Expression AnnotateResourceType(Expression expr, ResourceType type)
        {
            this.SetAnnotation(expr, ResourceTypeAnnotationName, type);
            return expr;
        }

        /// <summary>
        /// Propagates the type of the resource.
        /// </summary>
        /// <param name="exprSource">The expr source.</param>
        /// <param name="exprTarget">The expr target.</param>
        /// <returns></returns>
        public Expression PropagateResourceType(Expression exprSource, Expression exprTarget)
        {
            ResourceType type = this.GetResourceType(exprSource);
            return this.AnnotateResourceType(exprTarget, type);
        }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        public ResourceType GetResourceType(Expression expr)
        {
            return this[expr, ResourceTypeAnnotationName] as ResourceType;
        }
    }
}