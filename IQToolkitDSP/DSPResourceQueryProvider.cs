
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace IQToolkitDSP
{
    using IQToolkit;
    using IQToolkit.Data;

    internal class DSPResourceQueryProvider : IDataServiceQueryProvider
    {
        private IContext dataSource;
        private DSPMetadata metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPResourceQueryProvider"/> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public DSPResourceQueryProvider(DSPMetadata metadata)
        {
            this.metadata = metadata;
        }

        /// <summary>
        /// Gets the db entity provider.
        /// </summary>
        public DbEntityProvider DbEntityProvider
        {
            get { return this.dataSource.DbEntityProvider; }
        }

        #region IDataServiceQueryProvider Members

        /// <summary>
        /// The data source object from which data is provided.
        /// </summary>
        /// <returns>The data source.</returns>
        public object CurrentDataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (this.dataSource != null)
                {
                    throw new InvalidOperationException("CurrentDataSource should only be set once.");
                }

                this.dataSource = (IContext)value;
                this.dataSource.ContextMapping = this.metadata;
            }
        }

        /// <summary>
        /// Gets the value of the open property.
        /// </summary>
        /// <param name="target">Instance of the type that declares the open property.</param>
        /// <param name="propertyName">Name of the open property.</param>
        /// <returns>
        /// The value of the open property.
        /// </returns>
        public object GetOpenPropertyValue(object target, string propertyName)
        {
            throw new NotSupportedException("Open types are not yet supported.");
        }

        /// <summary>
        /// Gets the name and values of all the properties that are defined in the given instance of an open type.
        /// </summary>
        /// <param name="target">Instance of the type that declares the open property.</param>
        /// <returns>
        /// A collection of name and values of all the open properties.
        /// </returns>
        public IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target)
        {
            throw new NotSupportedException("Open types are not yet supported.");
        }


        /// <summary>
        /// Get the value of the strongly typed property.
        /// </summary>
        /// <param name="target">Instance of the type that declares the property.</param>
        /// <param name="resourceProperty">resource property describing the property.</param>
        /// <returns>
        /// Value for the property.
        /// </returns>
        public object GetPropertyValue(object target, ResourceProperty resourceProperty)
        {
            if (target == null)
                throw new ArgumentException("The specified target is null.");
            if (resourceProperty == null)
                throw new ArgumentException("The specified resource property is null.");

            Type t = target.GetType();

            PropertyInfo pi = TypeSystem.GetPropertyInfoForType(t, resourceProperty.Name, false);

            object value = null;

            try
            {
                value = pi.GetValue(target, null);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Failed getting property {0} value", resourceProperty.Name), ex);
            }

            return value;
        }

        /// <summary>
        /// Gets the typed query root for resource set.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="resourceSet">The resource set.</param>
        /// <returns></returns>
        private System.Linq.IQueryable GetTypedQueryRootForResourceSet<TElement>(ResourceSet resourceSet)
        {
            Query<TElement> sourceQuery = new Query<TElement>(this.dataSource.Provider);
            DSPIQALinqQueryProvider queryProvider = new DSPIQALinqQueryProvider(this.dataSource,sourceQuery.Provider, resourceSet);
           
            return queryProvider.CreateQuery(sourceQuery.Expression).AsQueryable();
        }

        /// <summary>
        /// Gets the <see cref="T:System.Linq.IQueryable`1"/> that represents the container.
        /// </summary>
        /// <param name="resourceSet">The resource set.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1"/> that represents the resource set, or a null value if there is no resource set for the specified <paramref name="resourceSet"/>.
        /// </returns>
        public System.Linq.IQueryable GetQueryRootForResourceSet(ResourceSet resourceSet)
        {
            if (resourceSet == null)
                throw new ArgumentException("The specified Resource Set is null.");

            MethodInfo getTypedQueryRootForResourceSetMethod = 
                typeof(DSPResourceQueryProvider).GetMethod(
                    "GetTypedQueryRootForResourceSet", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
            return (IQueryable)getTypedQueryRootForResourceSetMethod.MakeGenericMethod(resourceSet.ResourceType.InstanceType).Invoke(this, new object[] { resourceSet } );
        }

        /// <summary>
        /// Gets the resource type for the instance that is specified by the parameter.
        /// </summary>
        /// <param name="target">Instance to extract a resource type from.</param>
        /// <returns>
        /// The <see cref="T:System.Data.Services.Providers.ResourceType"/> of the supplied object.
        /// </returns>
        public ResourceType GetResourceType(object target)
        {
            if (target == null)
                throw new ArgumentException("The specified resource type target is null.");

            Type targetType = target.GetType();
            return this.metadata.Types.Single(rt => rt.InstanceType == targetType);
        }

        /// <summary>
        /// Invokes the given service operation and returns the results.
        /// </summary>
        /// <param name="serviceOperation">Service operation to invoke.</param>
        /// <param name="parameters">Values of parameters to pass to the service operation.</param>
        /// <returns>
        /// The result of the service operation, or a null value for a service operation that returns void.
        /// </returns>
        public object InvokeServiceOperation(ServiceOperation serviceOperation, object[] parameters)
        {
            throw new NotSupportedException("Service operations are not yet supported.");
        }

        /// <summary>
        /// Gets a value that indicates whether null propagation is required in expression trees.
        /// </summary>
        /// <returns>A <see cref="T:System.Boolean"/> value that indicates whether null propagation is required.</returns>
        public bool IsNullPropagationRequired
        {
            get { return false; }
        }

        #endregion

    }

}
