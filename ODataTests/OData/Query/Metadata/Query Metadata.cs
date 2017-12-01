
// <auto-generated>
//     This code was generated by a tool from a template stored in:
//     '..\odatasdkcodesamples\ODataValidationToolkit\4.0\ODataTestGen\TestTemplates\Query\Metadata\Query Metadata.tt'
//     Generation time: 12/08/2010 16:12:40
// </auto-generated>
	





namespace ODataGeneratedTests
{
    	
    using System;
    using System.Linq;
    using System.IO;
    using System.Data.Services;
    using System.Xml.Linq;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Text;
    using AtomPayloadAnalyzer;
    using System.Xml;
    
    [TestClass]
    public class QueryMetadata
    {
        /// <summary>
        /// Query the metadata from service root
        /// </summary>
        [TestMethod]
        public void QueryMetadataFromSQLServiceRoot()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot + "$metadata";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Metadata;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }

    }
}