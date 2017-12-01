
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Services.Providers;

namespace IQToolkitDSP
{   
    using System.Diagnostics;

    internal class DSPIQALinqQuery<T> : IOrderedQueryable<T>
    {
        private Expression queryExpression;
        private DSPIQALinqQueryProvider queryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPIQALinqQuery&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="queryProvider">The query provider.</param>
        /// <param name="queryExpression">The query expression.</param>
        internal DSPIQALinqQuery(DSPIQALinqQueryProvider queryProvider, Expression queryExpression)
        {
            this.queryProvider = queryProvider;
            this.queryExpression = queryExpression;
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            ExpressionConversionResult result = this.queryProvider.ConvertExpression(this.queryExpression);

            IQueryable query = this.queryProvider.CreateSourceQuery(result.Expression);
            
            if (this.queryProvider.Context.DbEntityProvider.Log != null) 
            {
                this.queryProvider.Context.DbEntityProvider.Log.WriteLine("DSPIQALinqQuery Underlying: {0}", query.ToString());
                Trace.WriteLine("Underlying: " + query.ToString());
            }
            
            return (IEnumerator<T>)query.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.queryProvider.ExecuteQuery<T>(this.queryExpression);
        }

        #endregion

        #region IQueryable Members

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
        public Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.</returns>
        public Expression Expression
        {
            get { return this.queryExpression; }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.</returns>
        public IQueryProvider Provider
        {
            get { return this.queryProvider; }
        }

        #endregion
    }

}
