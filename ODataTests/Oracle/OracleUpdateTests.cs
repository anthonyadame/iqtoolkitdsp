
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Services.Client;
using System.Collections;
using IQDSP = ODataTests.OracleSR;

namespace ODataGeneratedTests
{
    [TestClass]
    public class IQDSPSRTests
    {
        private IQDSP.OracleService context;
        private int nextId = 100;


        [TestInitialize()]
        public void TestInitialize()
        {
            this.context = new IQDSP.OracleService(new Uri(WcfTestUtil.ORAServiceRoot, UriKind.Absolute));
        }

        [TestMethod]
        public void QueryEntitySets()
        {
            Assert.AreEqual(8, context.CreateQuery<IQDSP.Category>("Categories").ToList().Count());
            Assert.AreEqual(0, context.CreateQuery<IQDSP.CustomerCustomerDemo>("CustomerCustomerDemos").ToList().Count());
            Assert.AreEqual(0, context.CreateQuery<IQDSP.CustomerDemographic>("CustomerDemographics").ToList().Count());
            Assert.AreEqual(91, context.CreateQuery<IQDSP.Customer>("Customers").ToList().Count());
            Assert.AreEqual(9, context.CreateQuery<IQDSP.Employee>("Employees").ToList().Count());
            Assert.AreEqual(49, context.CreateQuery<IQDSP.EmployeeTerritory>("EmployeeTerritories").ToList().Count());
            Assert.AreEqual(2155, context.CreateQuery<IQDSP.OrderDetail>("OrderDetails").ToList().Count());
            Assert.AreEqual(830, context.CreateQuery<IQDSP.Order>("Orders").ToList().Count());
            Assert.AreEqual(77, context.CreateQuery<IQDSP.Product>("Products").ToList().Count());
            Assert.AreEqual(4, context.CreateQuery<IQDSP.Region>("Regions").ToList().Count());
            Assert.AreEqual(3, context.CreateQuery<IQDSP.Shipper>("Shippers").ToList().Count());
            Assert.AreEqual(29, context.CreateQuery<IQDSP.Supplier>("Suppliers").ToList().Count());
            Assert.AreEqual(53, context.CreateQuery<IQDSP.Territory>("Territories").ToList().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void UpdateFailure()
        {
            var cust = context.Customers.FirstOrDefault();

            context.UpdateObject(cust);
            context.SaveChanges();
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void UpdateFailure2()
        {
            //var cust = NWWCF.Customer.CreateCustomer("ALFKI2", "MS");
            var cust = IQDSP.Customer.CreateCustomer("ALFKI2");
            context.AddObject("Customers", cust);
            context.SaveChanges();
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void UpdateFailure3()
        {
            var cust = context.Customers.FirstOrDefault();

            context.DeleteObject(cust);
            context.SaveChanges();
        }

        [TestMethod]
        public void UpdateEntity_Validate()
        {
            context.MergeOption = MergeOption.OverwriteChanges;
            IQDSP.Customer cust = context.CreateQuery<IQDSP.Customer>("Customers").FirstOrDefault();
            cust.CompanyName = string.Empty;
            context.UpdateObject(cust);

            try
            {
                context.SaveChanges();
                Assert.Fail("ValidationContext RequiredAttribute 'CompanyName' failed to throw");
            }
            catch (DataServiceRequestException ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("CompanyName is a required field."));
            }
        }

        [TestMethod]
        public void UpdateEntity_Validate_StringLength()
        {
            context.MergeOption = MergeOption.OverwriteChanges;
            IQDSP.Customer cust = context.CreateQuery<IQDSP.Customer>("Customers").FirstOrDefault();
            cust.City = "Alfreds Futterkiste Town";
            context.UpdateObject(cust);

            try
            {
                context.SaveChanges();
                Assert.Fail("ValidationContext StringLength 'City' failed to throw");
            }
            catch (DataServiceRequestException ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("maximum length exceed"));
            }
        }

        [TestMethod]
        public void UpdateEntityTest()
        {
            context.MergeOption = MergeOption.OverwriteChanges;
            IQDSP.Category cate = context.CreateQuery<IQDSP.Category>("Categories").FirstOrDefault();
            cate.CategoryName = "something else";
            context.UpdateObject(cate);
            context.SaveChanges();

            cate = context.CreateQuery<IQDSP.Category>("Categories").FirstOrDefault();
            Assert.AreEqual("something else", cate.CategoryName);

            //reset name
            IQDSP.Category cate2 = context.CreateQuery<IQDSP.Category>("Categories").FirstOrDefault();
            cate2.CategoryName = "Beverages";
            context.UpdateObject(cate2);
            context.SaveChanges();

            cate2 = context.CreateQuery<IQDSP.Category>("Categories").FirstOrDefault();
            Assert.AreEqual("Beverages", cate2.CategoryName);

        }


        [TestMethod]
        public void InsertDelete_EntityTest()
        {
            int precount = context.CreateQuery<IQDSP.Product>("Products").ToList().Count();
            
            IQDSP.Product newProduct = CreateProduct();

            context.AddObject("Products", newProduct);
            context.SaveChanges();

            IQDSP.Product product = RunQuery<IQDSP.Product>("/Products?$filter=ProductName eq 'Apple'").FirstOrDefault();
            Assert.AreEqual("Apple", product.ProductName);

            context.DeleteObject(product);
            context.SaveChanges();

            int postcount = context.CreateQuery<IQDSP.Product>("Products").ToList().Count();
            Assert.AreEqual(precount, postcount, "Product count pre: " + precount.ToString() + " post: " + postcount.ToString() +" did not match.");     
        }


        [TestMethod]
        public void UpdateEntityTest_ETag()
        {
            IQDSP.Customer c = this.CreateCustomer();
            context.MergeOption = MergeOption.OverwriteChanges;
            context.AddObject("Customers", c);
            context.SaveChanges();

            var outdatedContext = new DataServiceContext(context.BaseUri);
            IQDSP.Customer outdatedSupplier = outdatedContext.CreateQuery<IQDSP.Customer>("Customers").Where(_c => _c.CustomerID == c.CustomerID).FirstOrDefault();

            c.CompanyName = "something else";
            context.UpdateObject(c);
            context.SaveChanges();

            context.UpdateObject(c);
            context.SaveChanges();

            c = context.CreateQuery<IQDSP.Customer>("Customers").Where(_c => _c.CustomerID == c.CustomerID).FirstOrDefault();
            Assert.AreEqual("something else", c.CompanyName);

            outdatedContext.UpdateObject(outdatedSupplier);
            try
            {
                outdatedContext.SaveChanges();
                Assert.Fail("Concurrency failed to throw");
            }
            catch (DataServiceRequestException ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("Concurrency: precondition failed for property 'CompanyName'"));
            }
        }

        private IQDSP.Customer CreateCustomer()
        {
            return new IQDSP.Customer()
            {
                Address = "1200 6th Ave",
                City = "New York",
                CompanyName = "Alfreds Futterkiste US",
                ContactName = "Mario Anders",
                ContactTitle = "Sales Representative",
                Country = "USA",
                CustomerID = "ALFUS",
                Fax = "1 800 555-1212",
                Phone = "1 800 555-2222",
                PostalCode = "12345",
                Region = "NY"

            };
        }

        private IQDSP.Product CreateProduct()
        {
            return new IQDSP.Product()
            {
                ProductName = "Apple",
                SupplierID = 24,
                CategoryID = 7,
                QuantityPerUnit = "12 - 1 lb pkgs.",
                UnitPrice = 10.0m,
                UnitsInStock = 10,
                UnitsOnOrder = 0,
                ReorderLevel = 5,
                Discontinued = false
            };
        }

        private IEnumerable<TElement> RunQuery<TElement>(string queryUri)
        {
            return this.context.Execute<TElement>(new Uri(queryUri, UriKind.Relative));
        }


        [TestMethod]
        public void QueryAllCustomersWithOrders()
        {
            var q = context.CreateQuery<IQDSP.Customer>("Customers").Expand("Orders");

            int totalCustomerCount = q.Count();
            int totalOrdersCount = context.CreateQuery<IQDSP.Order>("Orders").Count();

            var qor = q.Execute() as QueryOperationResponse<IQDSP.Customer>;

            DataServiceQueryContinuation<IQDSP.Customer> nextCustLink = null;
            int custCount = 0;
            int orderCount = 0;
            do
            {
                ICollection previousOrderCollection = null;

                foreach (var c in qor)
                {
                    try
                    {
                        if (previousOrderCollection != null)
                        {
                            qor.GetContinuation(previousOrderCollection);
                            Assert.Fail("Out of scope collection did not throw");
                        }
                    }
                    catch (ArgumentException)
                    {
                    }

                    var nextOrderLink = qor.GetContinuation(c.Orders);
                    while (nextOrderLink != null)
                    {
                        if (custCount % 2 == 0)
                        {
                            var innerQOR = context.Execute<IQDSP.Order>(nextOrderLink) as QueryOperationResponse<IQDSP.Order>;
                            foreach (var innerOrder in innerQOR)
                            {
                                context.AttachLink(c, "Orders", innerOrder);
                                c.Orders.Add(innerOrder);
                            }
                            nextOrderLink = innerQOR.GetContinuation();
                        }
                        else
                        {
                            nextOrderLink = context.LoadProperty(c, "Orders", nextOrderLink).GetContinuation();
                        }
                    }

                    previousOrderCollection = c.Orders;

                    orderCount += c.Orders.Count;
                    custCount++;
                }

                nextCustLink = qor.GetContinuation();
                if (nextCustLink != null)
                {
                    qor = context.Execute<IQDSP.Customer>(nextCustLink) as QueryOperationResponse<IQDSP.Customer>;
                }

            } while (nextCustLink != null);

            Assert.AreEqual(totalCustomerCount, custCount);
            Assert.AreEqual(totalOrdersCount, orderCount);
            Assert.AreEqual(totalOrdersCount, context.Links.Count);
            Assert.AreEqual(totalCustomerCount + totalOrdersCount, context.Entities.Count);
        }


        private int GetNextId()
        {
            return nextId++;
        }
    
    }
}
