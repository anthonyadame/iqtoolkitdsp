using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NWWCF = ODataTests.NWWCFSR;

namespace ODataGeneratedTests
{
    [TestClass]
    public class NWQueryTests
    {
        
        private static NWWCF.NORTHWINDEntities service;
        private DataServiceContext ctx;


        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            service = new NWWCF.NORTHWINDEntities(new Uri(WcfTestUtil.NWServiceRoot, UriKind.Absolute));
        }


        [TestInitialize()]
        public void TestInitialize()
        {
            this.ctx = new DataServiceContext(new Uri(WcfTestUtil.NWServiceRoot));
        }

        [TestMethod]
        public void Metadata()
        {
            // Verify that we can get the metadata (as the server performs lot of verifications upon that request)
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.ctx.GetMetadataUri());
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "The $metadata didn't return success.");
        }


        [TestMethod]
        public void AllEntities()
        {
            // Intentionally using the server side type so that we can check that the Price property is not filled
            var q = this.ctx.CreateQuery<NWWCF.Product>("Products").ToList();
            Assert.AreEqual(77, q.Count, "The service returned unexpected number of results.");
            Assert.AreEqual(3, q[2].ProductID, "The ProductID is not correctly filled.");
            Assert.AreEqual("Chang", q[1].ProductName, "The ProductName is not correctly filled.");
            Assert.AreEqual(39, q[0].UnitsInStock, "Unexpected UnitsInStock value.");
        }

        [TestMethod]
        public void Filters()
        {
            VerifyEntityCount<NWWCF.Product>("/Products?$filter=ProductID eq 1", 1);
            VerifyEntityCount<NWWCF.Product>("/Products?$filter=ProductID ge 0", 77);
            VerifyEntityCount<NWWCF.Product>("/Products?$filter=length(ProductName) eq 4", 2);
            VerifyEntityCount<NWWCF.Product>("/Products?$filter=UnitsInStock gt 10", 63);
            VerifyEntityCount<NWWCF.Product>("/Products?$filter=false", 0);
            VerifyEntityCount<NWWCF.Product>("/Products?$filter=(ProductID gt 1) and (length(ProductName) eq 4)", 1);
        }

        [TestMethod]
        public void Projections()
        {
            VerifySelectedProperties<NWWCF.Product>("/Products?$select=ProductID&$filter=ProductID gt 0", "ProductID");
            VerifySelectedProperties<NWWCF.Product>("/Products?$select=ProductName", "ProductName");
            VerifySelectedProperties<NWWCF.Product>("/Products?$select=UnitsInStock", "UnitsInStock");
            VerifySelectedProperties<NWWCF.Product>("/Products?$select=ProductName,QuantityPerUnit,UnitsOnOrder", "ProductName", "QuantityPerUnit", "UnitsOnOrder");
            VerifySelectedProperties<NWWCF.Product>("/Products?$select=*&$filter=ProductID eq 2", "ProductID", "ProductName", "SupplierID", "CategoryID", "QuantityPerUnit", "UnitPrice", "UnitsInStock", "UnitsOnOrder", "ReorderLevel", "Discontinued");
        }

        [TestMethod]
        public void ResourceReferenceProperty()
        {
            List<NWWCF.Category> categories = ctx.CreateQuery<NWWCF.Category>("Categories").ToList();
            var category = RunSingleResultQuery<NWWCF.Category>("/Products(1)/Category");
            Assert.AreEqual(categories[0], category, "The CategoryID 1 should be in the first category.");
        }

        [TestMethod]
        public void ResourceSetReferenceProperty()
        {
            List<NWWCF.Product> products = ctx.CreateQuery<NWWCF.Product>("Products").ToList();
            var relatedProducts = RunQuery<NWWCF.Product>("/Categories(1)/Products").ToList();
            Assert.AreEqual(12, relatedProducts.Count, "Category 1 should have just 12 products.");
            Assert.IsTrue(relatedProducts.Contains(products[0]), "The category 1 should have product 1 in it.");
            relatedProducts = RunQuery<NWWCF.Product>("/Categories(1)/Products").ToList();
            Assert.AreEqual(12, relatedProducts.Count, "Category 1 should have 12 products.");
            Assert.IsTrue(relatedProducts.Contains(products[1]), "The category 1 should have product" + products[1].ProductID.ToString() + " in it.");
            Assert.IsTrue(relatedProducts.Contains(products[2]), "The category 1 should have product" + products[2].ProductID.ToString() + " in it.");
            relatedProducts = RunQuery<NWWCF.Product>("/Categories(2)/Products").ToList();
            Assert.AreEqual(14, relatedProducts.Count, "Category 2 should have 14 products.");
        }

        private void VerifyEntityCount<TElement>(string queryUri, int expectedEntityCount)
        {
            var q = RunQuery<TElement>(queryUri);
            Assert.AreEqual(expectedEntityCount, q.Count(), "Query '" + queryUri + "' didn't return expected number of entities.");
        }

        private void VerifySelectedProperties<TElement>(string queryUri, params string[] selectedProperties)
        {
            MergeOption mergeOptions = this.ctx.MergeOption;
            // No tracking as we need new instances returned each time we query (so that properties which were not in the response get their default values)
            this.ctx.MergeOption = MergeOption.NoTracking;
            try
            {

                var q = RunQuery<TElement>(queryUri);

                foreach (TElement item in q)
                {
                    foreach (var property in typeof(TElement).GetProperties())
                    {
                        object propertyValue = property.GetValue(item, null);
                        object defaultValue = GetDefaultValueForType(property.PropertyType);

                        if (selectedProperties.Contains(property.Name))
                        {
                            Assert.AreNotEqual(defaultValue, propertyValue, "Property '" + property.Name + "' has default value even though it was selected.");
                        }
                        else
                        {
                            Assert.AreEqual(defaultValue, propertyValue, "Property '" + property.Name + "' doesn't have default value even though it was not selected.");
                        }
                    }
                }
            }
            finally
            {
                this.ctx.MergeOption = mergeOptions;
            }
        }

        private static MethodInfo GetDefaultValueForTypeInnerMethod = typeof(NWQueryTests).GetMethod("GetDefaultValueForTypeInner", BindingFlags.NonPublic | BindingFlags.Static);

        private static object GetDefaultValueForType(Type t)
        {
            return GetDefaultValueForTypeInnerMethod.MakeGenericMethod(t).Invoke(null, null);
        }

        private static object GetDefaultValueForTypeInner<T>()
        {
            return default(T);
        }

        private IEnumerable<TElement> RunQuery<TElement>(string queryUri)
        {
            return this.ctx.Execute<TElement>(new Uri(queryUri, UriKind.Relative));
        }

        private TElement RunSingleResultQuery<TElement>(string queryUri)
        {
            return RunQuery<TElement>(queryUri).AsEnumerable().Single();
        }
    }
}
