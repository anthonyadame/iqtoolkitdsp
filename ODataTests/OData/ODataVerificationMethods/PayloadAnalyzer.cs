
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AtomPayloadAnalyzer
{
    public enum UriResultType { Metadata, Feed, Entry, Property, PropertyValue, None };
	


    public class PayloadAnalyzer
    {
        public static XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
		public static XNamespace ODataMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
		public static XNamespace DataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";
		public static XNamespace EdmxNamespace = "http://schemas.microsoft.com/ado/2007/06/edmx";
        /// <summary>
        /// This function examine the name of a xml element and return the type of payload.
        /// </summary>
        /// <param name="content">The Atom format payload of response</param>
        /// <returns></returns>
        public static UriResultType getResultType(XElement payload)
        {
			if (payload.Name == AtomNamespace + "feed")
				return UriResultType.Feed;
			else if (payload.Name == AtomNamespace + "entry")
				return UriResultType.Entry;
			else if (payload.Name == EdmxNamespace + "Edmx")
				return UriResultType.Metadata;

			return UriResultType.Property;      
        }      
    }
	
	
}