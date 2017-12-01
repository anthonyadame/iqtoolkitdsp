// <auto-generated>
//     This code was generated by a tool from a template stored in:
//     '..\odatasdkcodesamples\ODataValidationToolkit\4.0\ODataTestGen\TestTemplates\Query\Select\Query First Two Properties of All Entity Sets.tt'
//     Generation time: 12/08/2010 16:12:49
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
    public class QueryFirstTwoProperties
    {
        
        /// <summary>
        /// Query the first two properties from Categories
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromCategories()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Categories?$select=CategoryID,CategoryName";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from CustomerCustomerDemos
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromCustomerCustomerDemos()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "CustomerCustomerDemos?$select=CustomerID,CustomerTypeID";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from CustomerDemographics
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromCustomerDemographics()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "CustomerDemographics?$select=CustomerDesc,CustomerTypeID";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Customers
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromCustomers()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Customers?$select=Address,City";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Employees
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromEmployees()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Employees?$select=Address,BirthDate";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from EmployeeTerritories
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromEmployeeTerritories()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "EmployeeTerritories?$select=EmployeeID,TerritoryID";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from OrderDetails
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromOrderDetails()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "OrderDetails?$select=Discount,OrderID";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Orders
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromOrders()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Orders?$select=CustomerID,EmployeeID";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Products
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromProducts()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Products?$select=ProductID,ProductName";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Regions
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromRegions()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Regions?$select=RegionDescription,RegionID";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Shippers
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromShippers()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Shippers?$select=CompanyName,Phone";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Suppliers
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromSuppliers()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Suppliers?$select=SupplierID,CompanyName";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


        /// <summary>
        /// Query the first two properties from Territories
        /// </summary>
        [TestMethod]
        public void QueryFirstTwoPropertiesFromTerritories()
        {
            string strQuery =  WcfTestUtil.OdataServiceRoot  + "Territories?$select=RegionID,TerritoryDescription";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            using(XmlReader xrResponse = XmlReader.Create(response.GetResponseStream()))
            {
                XElement payload = XElement.Load(xrResponse);
                UriResultType expectedType = UriResultType.Feed;
                UriResultType actualType = PayloadAnalyzer.getResultType(payload);
                Assert.AreEqual(expectedType, actualType);
            }
            response.Close();
        }


    }
}
