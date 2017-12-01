using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQDSP = ODataTests.MSSqlSvrSR;

namespace ODataGeneratedTests
{
    [TestClass]
    public class MSSqlSvrQueryTests
    {
        private static IQDSP.MSSQLSVRService service;
        private DataServiceContext ctx;


        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            service = new IQDSP.MSSQLSVRService(new Uri(WcfTestUtil.SQLServiceRoot));
        }


        [TestInitialize()]
        public void TestInitialize()
        {
            this.ctx = new DataServiceContext(new Uri(WcfTestUtil.SQLServiceRoot));
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
            var q = this.ctx.CreateQuery<IQDSP.Product>("Products").ToList();
            Assert.AreEqual(77, q.Count, "The service returned unexpected number of results.");
            Assert.AreEqual(3, q[2].ProductID, "The ProductID is not correctly filled.");
            Assert.AreEqual("Chang", q[1].ProductName, "The ProductName is not correctly filled.");
            Assert.AreEqual(39, q[0].UnitsInStock, "Unexpected UnitsInStock value.");
        }

        [TestMethod]
        public void OrderBy()
        {
            var products = RunQuery<IQDSP.Product>("/Products?$orderby=ProductName").ToList();
            Assert.AreEqual("Alice Mutton", products[0].ProductName, "The first ProductName is not correct.");
            Assert.AreEqual("Zaanse koeken", products[76].ProductName, "The last ProductName is not correct.");

            var productsortreleated = RunQuery<IQDSP.Product>("/Products?$orderby=SupplierID,Category/CategoryName desc").ToList();
            Assert.AreEqual(3, productsortreleated[0].ProductID, "The first ProductID is not correct.");
            Assert.AreEqual(61, productsortreleated[76].ProductID, "The last ProductID is not correct.");
        }

        [TestMethod]
        public void Top()
        {
            var products = RunQuery<IQDSP.Product>("/Products?$top=5").ToList();
            Assert.AreEqual(1, products[0].ProductID, "The first ProductID is not correct.");
            Assert.AreEqual(5, products[4].ProductID, "The last ProductID is not correct.");

            var productstopdesc = RunQuery<IQDSP.Product>("/Products?$orderby=ProductName desc&$top=5").ToList();
            Assert.AreEqual(47, productstopdesc[0].ProductID, "The first ProductID is not correct.");
            Assert.AreEqual(7, productstopdesc[4].ProductID, "The last ProductID is not correct.");
        }

        [TestMethod]
        public void Skip()
        {
            var products = RunQuery<IQDSP.Product>("/Categories(1)/Products?$skip=2").ToList();
            Assert.AreEqual(24, products[0].ProductID, "The first ProductID is not correct.");
            Assert.AreEqual(39, products[4].ProductID, "The last ProductID is not correct.");

            var productstopdesc = RunQuery<IQDSP.Product>("/Categories(1)/Products?$skip=2&top2&$orderby=ProductName").ToList();
            Assert.AreEqual(39, productstopdesc[0].ProductID, "The first ProductID is not correct.");
            Assert.AreEqual(76, productstopdesc[4].ProductID, "The last ProductID is not correct.");
        }


        [TestMethod]
        public void Filters()
        {
            //Eq	Equal
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=ProductID eq 1", 1);

            //Ne	Not equal
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=ProductID ne 1", 76);
            
            //Gt	Greater than
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=UnitsInStock gt 10", 63);

            //Ge	Greater than or equal
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=ProductID ge 1", 77);

            //Lt	Less than
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=ProductID lt 77", 76);
            
            //Le	Less than or equal	/Products?$filter=Price le 100
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=ProductID lt 76", 75);
            
            //And	Logical and	/Products?$filter=Price le 200 and Price gt 3.5
            VerifyEntityCount<IQDSP.Product>("/Products()?$filter=(UnitPrice gt 10M) and (UnitPrice lt 20M)", 25);

            //Or	Logical or	/Products()?$filter=(UnitPrice lt 10M) or (UnitPrice gt 20M)
            VerifyEntityCount<IQDSP.Product>("/Products()?$filter=(UnitPrice lt 10M) or (UnitPrice gt 20M)", 48);

            //Not	Logical negation
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=not endswith(ProductName,'ng')", 74);


            //Arithmetic Operators
            //Add	Addition	/Products?$filter=Price add 5 gt 10
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=UnitPrice add 5M gt 10M", 75);

            //Sub	Subtraction	/Products?$filter=Price sub 5 gt 10
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=UnitPrice sub 5M gt 10M", 51);

            //Mul	Multiplication	/Products?$filter=Price mul 2 gt 2000
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=UnitPrice mul 2M gt 200M", 2);
            
            //Div	Division
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=UnitPrice div 2M gt 4M", 71);
            
            //Mod	Modulo
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=UnitPrice mod 2M eq 0M", 25);

            
            //Grouping Operators
            //( )	Precedence grouping
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=(UnitPrice sub 5M) gt 10M", 51);

        }


        [TestMethod]
        public void Filters_String_Functions()
        {
            //bool substringof(string po, string p1)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=substringof('Anton',ProductName) eq true", 2);

            //bool endswith(string p0, string p1)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=endswith(ProductName,'ng') eq true", 3);
        
            //bool endswith(string p0, string p1)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=startswith(ProductName,'Ch') eq true", 6);

            //int length(string p0 )
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=length(ProductName) eq 4", 2);
        
            //int indexof(string p0, string p1)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=indexof(ProductName,'Anton') eq 5", 2);

            //string replace(string p0, string find, string replace)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=replace(ProductName,' ','') eq 'AniseedSyrup'", 1);

            //string substring(string p0, int pos)
            //VerifyEntityCount<IQDSP.Product>("/Products?$filter=substringof(ProductName,1) eq 'niseed Syrup'", 1);

            //string substring(string p0, int pos, int length)
            //VerifyEntityCount<IQDSP.Product>("/Products?$filter=substringof(ProductName,1,2) eq 'ni'", 1);
            

            //string tolower(string p0)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=tolower(ProductName) eq 'aniseed syrup'", 1);
            
            //string toupper(string p0)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=toupper(ProductName) eq 'ANISEED SYRUP'", 1);
        
            //string trim(string p0)
            VerifyEntityCount<IQDSP.Product>("/Products?$filter=trim(ProductName) eq 'Aniseed Syrup'", 1);

            //string concat(string p0, string p1)
            //VerifyEntityCount<IQDSP.Employee>("/Customers?$filter=concat(concat(City, ', '), Country) eq 'Berlin, Germany'", 1);

            //Date Functions
            //int day(DateTime p0)
            VerifyEntityCount<IQDSP.Employee>("/Employees?$filter=day(BirthDate) eq 8", 1);
            
            //int hour(DateTime p0)
            VerifyEntityCount<IQDSP.Employee>("/Employees?$filter=hour(BirthDate) eq 0", 9);
            
            //int minute(DateTime p0)
            VerifyEntityCount<IQDSP.Employee>("/Employees?$filter=minute(BirthDate) eq 0", 9);

            //int month(DateTime p0)
            VerifyEntityCount<IQDSP.Employee>("/Employees?$filter=month(BirthDate) eq 12", 1);

            //int second(DateTime p0)
            VerifyEntityCount<IQDSP.Employee>("/Employees?$filter=second(BirthDate) eq 0", 9);

            //int year(DateTime p0)
            VerifyEntityCount<IQDSP.Employee>("/Employees?$filter=year(BirthDate) eq 1948", 1);

            //Math Functions
            //double round(double p0)
            VerifyEntityCount<IQDSP.Order>("/Orders?$filter=round(Freight) eq 32", 11);
            
            //decimal round(decimal p0)
            VerifyEntityCount<IQDSP.Order>("/Orders?$filter=round(Freight) eq 32", 11);

            //double floor(double p0)
            VerifyEntityCount<IQDSP.Order>("/Orders?$filter=floor(Freight) eq 32", 12);

            //decimal floor(decimal p0)
            VerifyEntityCount<IQDSP.Order>("/Orders?$filter=floor(Freight) eq 32", 12);

            //double ceiling(double p0)
            VerifyEntityCount<IQDSP.Order>("/Orders?$filter=ceiling(Freight) eq 32", 7);
            
            //decimal ceiling(decimal p0)
            VerifyEntityCount<IQDSP.Order>("/Orders?$filter=ceiling(Freight) eq 32", 7);
            
            //Type Functions
            //bool IsOf(type p0)
            //VerifyEntityCount<IQDSP.Order>("/Orders?$filter=isof('IQTWcf.Order')", 830);
            
            //bool IsOf(expression p0, type p1)
            //VerifyEntityCount<IQDSP.Order>("/Orders?$filter=isof(ShipCountry, 'Edm.String')", 0);
            
        }


        //[TestMethod]
        //public void Projections()
        //{
        //    VerifySelectedPropertiesNoMerge<IQDSP.Product>("/Products?$select=ProductID&$filter=ProductID gt 0", "ProductID");
        //    VerifySelectedPropertiesNoMerge<IQDSP.Product>("/Products?$select=ProductName", "ProductName");
        //    VerifySelectedPropertiesNoMerge<IQDSP.Product>("/Products?$select=UnitsInStock", "UnitsInStock");
        //    VerifySelectedProperties<IQDSP.Product>("/Products?$select=ProductName,QuantityPerUnit,UnitsOnOrder", "ProductName", "QuantityPerUnit", "UnitsOnOrder");
        //}

        [TestMethod]
        public void ResourceReferenceProperty()
        {
            List<IQDSP.Category> categories = ctx.CreateQuery<IQDSP.Category>("Categories").ToList();
            var category = RunSingleResultQuery<IQDSP.Category>("/Products(1)/Category");
            Assert.AreEqual(categories[0], category, "The CategoryID 1 should be in the first category.");
        }

        [TestMethod]
        public void ResourceSetReferenceProperty()
        {
            List<IQDSP.Product> products = ctx.CreateQuery<IQDSP.Product>("Products").ToList();
            var relatedProducts = RunQuery<IQDSP.Product>("/Categories(1)/Products").ToList();
            Assert.AreEqual(12, relatedProducts.Count, "Category 1 should have just 12 products.");
            Assert.IsTrue(relatedProducts.Contains(products[0]), "The category 1 should have product 1 in it.");
            relatedProducts = RunQuery<IQDSP.Product>("/Categories(1)/Products").ToList();
            Assert.AreEqual(12, relatedProducts.Count, "Category 1 should have 12 products.");
            Assert.IsTrue(relatedProducts.Contains(products[1]), "The category 1 should have product " + products[1].ProductID.ToString() + " in it.");
            Assert.IsTrue(relatedProducts.Contains(products[23]), "The category 1 should have product " + products[23].ProductID.ToString() + " in it.");
            relatedProducts = RunQuery<IQDSP.Product>("/Categories(2)/Products").ToList();
            Assert.AreEqual(12, relatedProducts.Count, "Category 2 should have 12 products.");
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


        private void VerifySelectedPropertiesNoMerge<TElement>(string queryUri, params string[] selectedProperties)
        {
            //MergeOption mergeOptions = this.ctx.MergeOption;
            // No tracking as we need new instances returned each time we query (so that properties which were not in the response get their default values)
            //this.ctx.MergeOption = MergeOption.NoTracking;
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
                //this.ctx.MergeOption = mergeOptions;
            }
        }

        private static MethodInfo GetDefaultValueForTypeInnerMethod = typeof(MSSqlSvrQueryTests).GetMethod("GetDefaultValueForTypeInner", BindingFlags.NonPublic | BindingFlags.Static);

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
