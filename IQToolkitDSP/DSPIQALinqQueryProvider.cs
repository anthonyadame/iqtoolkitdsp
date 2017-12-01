
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Data.Services.Providers;

namespace IQToolkitDSP
{
    using System.Diagnostics;

    internal class DSPIQALinqQueryProvider : IQueryProvider
    {
        private IQueryProvider underlyingQueryProvider;
        private IContext context;
        private ExpressionResourceAnnotations annotations;
        private ResourceSet resourceSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPIQALinqQueryProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="underlyingQueryProvider">The underlying query provider.</param>
        /// <param name="resourceSet">The resource set.</param>
        public DSPIQALinqQueryProvider(IContext context, IQueryProvider underlyingQueryProvider, ResourceSet resourceSet)
        {
            this.underlyingQueryProvider = underlyingQueryProvider;
            this.context = context;
            this.annotations = new ExpressionResourceAnnotations();
            this.resourceSet = resourceSet;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPIQALinqQueryProvider"/> class.
        /// </summary>
        /// <param name="underlyingQueryProvider">The underlying query provider.</param>
        public DSPIQALinqQueryProvider(IQueryProvider underlyingQueryProvider)
        {
            this.underlyingQueryProvider = underlyingQueryProvider;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DSPIQALinqQueryProvider"/> class from being created.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="underlyingQueryProvider">The underlying query provider.</param>
        private DSPIQALinqQueryProvider(IContext context, IQueryProvider underlyingQueryProvider)
        {
            Debug.Assert(context != null, "context != null");
            Debug.Assert(underlyingQueryProvider != null, "underlyingQueryProvider != null");
            this.context = context;
            this.underlyingQueryProvider = underlyingQueryProvider;
            this.annotations = new ExpressionResourceAnnotations();
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        internal IContext Context
        {
            get { return this.context; }
        }

        /// <summary>
        /// Gets the expression annotations.
        /// </summary>
        internal ExpressionResourceAnnotations ExpressionAnnotations
        {
            get { return this.annotations; }
        }

        /// <summary>
        /// Sets the resource type annotation.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="resourceType">Type of the resource.</param>
        internal void SetResourceTypeAnnotation(Expression expression, ResourceTypeMapping resourceType)
        {
            this.annotations.AnnotateResourceType(expression, resourceType);
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <param name="underlyingQuery">The underlying query.</param>
        /// <returns></returns>
        public static IQueryable CreateQuery(IQueryable underlyingQuery)
        {
            DSPIQALinqQueryProvider provider = new DSPIQALinqQueryProvider(underlyingQuery.Provider);
            return provider.CreateQuery(underlyingQuery.Expression);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        internal IEnumerator<TElement> ExecuteQuery<TElement>(Expression expression)
        {
            expression = this.ProcessExpression(expression);
            return this.Context.Provider.CreateQuery<TElement>(expression).GetEnumerator();
        }

        #region IQueryProvider Members

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DSPIQALinqQuery<TElement>(this, expression);
        }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable"/> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable"/> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        public IQueryable CreateQuery(Expression expression)
        {
            if (expression == null)
                throw new ArgumentException("The specified expression is null.");
            
            Type et = TypeSystem.GetIEnumerableElementType(expression.Type);
            Type qt = typeof(DSPIQALinqQuery<>).MakeGenericType(et);
            object[] args = new object[] { this, expression };

            ConstructorInfo ci = qt.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(DSPIQALinqQueryProvider), typeof(Expression) },
                null);

            return (IQueryable)ci.Invoke(args);
        }

        /// <summary>
        /// Executes the specified expression.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            expression = this.ProcessExpression(expression);
            return this.underlyingQueryProvider.Execute<TResult>(expression);
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        public object Execute(Expression expression)
        {
            expression = this.ProcessExpression(expression);
            return this.underlyingQueryProvider.Execute(expression);
        }

        #endregion

        /// <summary>
        /// Creates the source query.
        /// </summary>
        /// <param name="sourceQueryExpression">The source query expression.</param>
        /// <returns></returns>
        internal IQueryable CreateSourceQuery(Expression sourceQueryExpression)
        {
            return this.underlyingQueryProvider.CreateQuery(sourceQueryExpression);
        }

        /// <summary>
        /// Creates the source query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="sourceQueryExpression">The source query expression.</param>
        /// <returns></returns>
        internal IQueryable<TElement> CreateSourceQuery<TElement>(Expression sourceQueryExpression)
        {
            return this.underlyingQueryProvider.CreateQuery<TElement>(sourceQueryExpression);
        }

        /// <summary>
        /// Processes the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private Expression ProcessExpression(Expression expression)
        {
            if (this.ExpressionAnnotations == null)
            {
                SetResourceTypeAnnotation(expression, (ResourceTypeMapping)resourceSet.ResourceType);
            }
            return ConvertExpression(expression).Expression;
        }

        /// <summary>
        /// Converts the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        internal ExpressionConversionResult ConvertExpression(Expression expression)
        {
            if (this.context.DbEntityProvider.Log != null)
            {
                this.context.DbEntityProvider.Log.WriteLine("ConvertExpression Before: {0}", expression.ToString());  
                Trace.WriteLine("Before: " + expression.ToString());
            }
            
            ExpressionConversionResult result = ExpressionConverter.Convert(expression, this.ExpressionAnnotations, this.Context.ContextMapping);
            
            if (this.context.DbEntityProvider.Log != null)
            {
                this.context.DbEntityProvider.Log.WriteLine("ConvertExpression After: {0}", result.Expression.ToString());
                Trace.WriteLine("After: " + result.Expression.ToString());
            }
            return result;
        }
    }

}
