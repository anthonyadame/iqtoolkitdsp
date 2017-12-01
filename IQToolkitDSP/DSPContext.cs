
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Providers;
using System.Net;

namespace IQToolkitDSP
{
    using IQToolkit;
    using IQToolkit.Data;
    using IQToolkit.Data.Common;
    using IQToolkit.Data.Mapping;


    public class DSPContext : IContext
    {
        #region DSPContext Fields

        private IEntityProvider provider;
        private DSPMetadata metadata;

        #endregion DSPContext Fields


        /// <summary>
        /// Initializes a new instance of the <see cref="DSPContext"/> class.
        /// </summary>
        public DSPContext()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPContext"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public DSPContext(IEntityProvider provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        public IEntityProvider Provider
        {
            get { return this.provider; }
        }

        /// <summary>
        /// Gets the db entity provider.
        /// </summary>
        public DbEntityProvider DbEntityProvider
        {
            get { return (DbEntityProvider)this.provider; }
        }

        
        /// <summary>
        /// Gets or sets the context mapping.
        /// </summary>
        /// <value>
        /// The context mapping.
        /// </value>
        public DSPMetadata ContextMapping
        {
            get { return this.metadata; }
            set { this.metadata = value; }
        }

    }

    public interface IContext
    {
        IEntityProvider Provider { get; }
        DbEntityProvider DbEntityProvider { get; }
        DSPMetadata ContextMapping { get; set; }
    }

}
