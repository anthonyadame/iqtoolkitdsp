using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Reflection;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IQTWcf
{

    using IQToolkitDSP;
    using IQToolkit;
    using IQToolkit.Data;
    using IQToolkit.Data.Common;
    using IQToolkit.Data.Mapping;
    using IQTContrib;
    using System.Data.SqlClient;
    using IQToolkit.Data.SqlClient;


    [JSONPSupportBehaviorAttribute]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class IQTMSSQL : DSPDataService<DSPContext>
    {
        private static IQueryProvider provider = null;
        private static DSPContext context = null;
        private static DSPMetadata metadata = null;

        private static bool dbInit = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IQTMSSQL"/> class.
        /// </summary>
        public IQTMSSQL()
        {
            if (HttpContext.Current != null)
            {
                Trace.WriteLine("URL: " + HttpContext.Current.Request.Url);
            }

            if (provider == null)
            {
                provider = (CreateRepository()).Provider;
            }
            
        }

        /// <summary>
        /// Creates the data source.
        /// </summary>
        /// <returns></returns>
        protected override DSPContext CreateDataSource()
        {
            if (context == null)
            {
                context = new DSPContext((IEntityProvider)provider);
            }

            return context;
        }

        /// <summary>
        /// Creates the DSP metadata.
        /// </summary>
        /// <returns></returns>
        protected override DSPMetadata CreateDSPMetadata()
        {
            if (metadata == null)
            {

                metadata = new DSPMetadata("MSSQLSVRService", "IQTWcf");

                metadata.MapResourceSet("Categories", typeof(Category));
                metadata.MapResourceSet("CustomerCustomerDemos", typeof(CustomerCustomerDemo));
                metadata.MapResourceSet("CustomerDemographics", typeof(CustomerDemographic));
                metadata.MapResourceSet("Customers", typeof(Customer));
                metadata.MapResourceSet("Employees", typeof(Employee));
                metadata.MapResourceSet("EmployeeTerritories", typeof(EmployeeTerritory));
                metadata.MapResourceSet("OrderDetails", typeof(OrderDetail));
                metadata.MapResourceSet("Orders", typeof(Order));
                metadata.MapResourceSet("Products", typeof(Product));
                metadata.MapResourceSet("Regions", typeof(Region));
                metadata.MapResourceSet("Shippers", typeof(Shipper));
                metadata.MapResourceSet("Suppliers", typeof(Supplier));
                metadata.MapResourceSet("Territories", typeof(Territory));

                metadata.InitializeMetadataMapping();

            }
            return metadata;
        }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        /// <param name="config">The config.</param>
        public static void InitializeService(DataServiceConfiguration config)
        {

#if DEBUG
            config.UseVerboseErrors = true;
#endif

            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.DataServiceBehavior.AcceptProjectionRequests = true;
        }

        /// <summary>
        /// Creates the repository. Init the provider and establish
        /// a database connection
        /// </summary>
        /// <returns></returns>
        private static DbEntityRepository CreateRepository()
        {
            DbEntityProvider dbentprovider;

            string connectionString = ConfigurationManager.AppSettings["ConnSQL"];
            string mappath = Utils.GetMapPath("MapSQL");
            string provider = ConfigurationManager.AppSettings["ProviderSQL"];

            var mapping = XmlMapping.FromXml(File.ReadAllText(mappath));

            dbentprovider = DbEntityProvider.From(provider, connectionString, mapping, QueryPolicy.Default);
            
            //May not be needed for all databases, 
            //some need a little kick start before serving requests
            DbInit(dbentprovider);

            return new DbEntityRepository(dbentprovider);
        }

        /// <summary>
        /// Db connection init.
        /// </summary>
        /// <param name="provider">The provider.</param>
        private static void DbInit(DbEntityProvider provider)
        {
            if (dbInit == false)
            {
                dbInit = true;
                provider.Connection.Open();
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public override object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataServiceUpdateProvider))
            {
                return new DSPRestrictedUpdateProvider(context.DbEntityProvider, this.Metadata);
            }

            return base.GetService(serviceType);
        }


        #region Update provider with constrain and concurrency support

        /// <summary>
        /// UpdateProvider constraints and defaults
        /// </summary>
        internal class DSPRestrictedUpdateProvider : DSPIQUpdateProvider
        {
            public DSPRestrictedUpdateProvider(DbEntityProvider provider, DSPMetadata metadata)
                : base(provider, metadata, new EntitySessionEx(provider))
            {
            }

            /// <summary>
            /// Applies the resource default value.
            /// </summary>
            /// <param name="targetResource">The target resource.</param>
            protected override void ApplyResourceDefaultValue(object targetResource)
            {
                base.ApplyResourceDefaultValue(targetResource);
            }
            
            /// <summary>
            /// Validates the value(s) before base SetValue.
            /// On validation result, throw DataServiceException
            /// </summary>
            /// <param name="targetResource">The target resource.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="propertyValue">The property value.</param>
            public override void SetValue(object targetResource, string propertyName, object propertyValue)
            {

                object dspTargetResource = ValidateResource(targetResource);

                if (propertyValue != null && (propertyValue.GetType().Name == "JsonObjectRecords" || propertyValue.GetType().Name == "Hashtable"))
                {
                    throw new DataServiceException(500, "Invalid value set for property \"" + propertyName + "\"");
                }

                AddSupportForMetadataAttribute(dspTargetResource.GetType());

                var validationContext = new ValidationContext(dspTargetResource, null, null);
                var result = new List<ValidationResult>();

                validationContext.MemberName = propertyName;
                Validator.TryValidateProperty(propertyValue, validationContext,result);

                if (result.Any())
                    throw new DataServiceException(
                        result
                        .Select(r => r.ErrorMessage)
                        .Aggregate((m1, m2) => String.Concat(m1, Environment.NewLine, m2)));

                base.SetValue(targetResource, propertyName, propertyValue);
            }
        
        
        }

        /// <summary>
        /// Adds the support for metadata attribute.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        private static void AddSupportForMetadataAttribute(Type entityType)
        {
            var metadataAttribute = entityType
                .GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .OfType<MetadataTypeAttribute>().FirstOrDefault();

            if (metadataAttribute != null)
                TypeDescriptor.AddProviderTransparent(
                    new AssociatedMetadataTypeTypeDescriptionProvider(entityType, metadataAttribute.MetadataClassType),
                    entityType);
        }

        #endregion Update provider with constrain and concurrency support

    }
}
