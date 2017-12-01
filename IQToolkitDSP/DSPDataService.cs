
using System;
using System.Data.Services;
using System.Data.Services.Providers;

namespace IQToolkitDSP
{

    public abstract class DSPDataService<T> : DataService<T>, IServiceProvider where T : class
    {
        private DSPMetadata metadata;
        private DSPResourceQueryProvider resourceQueryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPDataService&lt;T&gt;"/> class.
        /// </summary>
        public DSPDataService(){}

        /// <summary>
        /// Abstract method used by a derived class to create the DSP metadata.
        /// </summary>
        /// <returns>The metadata definition</returns>
        protected abstract DSPMetadata CreateDSPMetadata();

        /// <summary>
        /// Gets the metadata. Creates the metadata if null and sets it to readonly
        /// </summary>
        protected DSPMetadata Metadata
        {
            get
            {
                if (this.metadata == null)
                {
                    this.metadata = CreateDSPMetadata();
                    this.metadata.SetReadOnly();
                    this.resourceQueryProvider = new DSPResourceQueryProvider(this.metadata);
                }

                return this.metadata;
            }
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        public virtual object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataServiceMetadataProvider))
            {
                return this.Metadata;
            }
            else if (serviceType == typeof(IDataServiceQueryProvider))
            {
                return this.resourceQueryProvider;
            }
            else if (serviceType == typeof(IDataServiceUpdateProvider))
            {
                return new DSPIQUpdateProvider(this.resourceQueryProvider.DbEntityProvider , this.Metadata);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }

}
