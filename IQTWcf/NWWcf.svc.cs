using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;

using System.Diagnostics;


namespace IQTWcf
{
    #if DEBUG
    [System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    #endif
    public class NWWcf : DataService<Northwind.NORTHWINDEntities>
    {
        public NWWcf()
        {
            if (HttpContext.Current != null)
            {
                Trace.WriteLine("URL: " + HttpContext.Current.Request.Url);
            }
        }

        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            #if DEBUG
            config.UseVerboseErrors = true;
            #endif

            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }
    }
}
