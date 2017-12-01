
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Services.Client;
using System.Collections;
using NWWCF = ODataTests.NWWCFSR;

namespace ODataGeneratedTests
{
    [TestClass]
    public class NWWCFSRTests
    {
        private NWWCF.NORTHWINDEntities context;

        [TestInitialize()]
        public void TestInitialize()
        {
            this.context = new NWWCF.NORTHWINDEntities(new Uri(WcfTestUtil.NWServiceRoot, UriKind.Absolute));
        }

        [TestMethod]
        public void QueryEntitySets()
        {
            Assert.AreEqual(8, context.CreateQuery<NWWCF.Category>("Categories").ToList().Count());
            //Assert.AreEqual(0, context.CreateQuery<NWWCF.CustomerCustomerDemos>("CustomerCustomerDemos").ToList().Count());
            Assert.AreEqual(0, context.CreateQuery<NWWCF.CustomerDemographic>("CustomerDemographics").ToList().Count());
            Assert.AreEqual(91, context.CreateQuery<NWWCF.Customer>("Customers").ToList().Count());
            Assert.AreEqual(9, context.CreateQuery<NWWCF.Employee>("Employees").ToList().Count());
            //Assert.AreEqual(49, context.CreateQuery<NWWCF.EmployeeTerritory>("EmployeeTerritories").ToList().Count());
            Assert.AreEqual(2155, context.CreateQuery<NWWCF.Order_Detail>("Order_Details").ToList().Count());
            Assert.AreEqual(830, context.CreateQuery<NWWCF.Order>("Orders").ToList().Count());
            Assert.AreEqual(77, context.CreateQuery<NWWCF.Product>("Products").ToList().Count());
            Assert.AreEqual(4, context.CreateQuery<NWWCF.Region>("Regions").ToList().Count());
            Assert.AreEqual(3, context.CreateQuery<NWWCF.Shipper>("Shippers").ToList().Count());
            Assert.AreEqual(29, context.CreateQuery<NWWCF.Supplier>("Suppliers").ToList().Count());
            Assert.AreEqual(53, context.CreateQuery<NWWCF.Territory>("Territories").ToList().Count());
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
            var cust = NWWCF.Customer.CreateCustomer("ALFKI2", "MS");
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
        public void QueryAllCustomersWithOrders()
        {
            var q = context.CreateQuery<NWWCF.Customer>("Customers").Expand("Orders");

            int totalCustomerCount = q.Count();
            int totalOrdersCount = context.CreateQuery<NWWCF.Order>("Orders").Count();

            var qor = q.Execute() as QueryOperationResponse<NWWCF.Customer>;

            DataServiceQueryContinuation<NWWCF.Customer> nextCustLink = null;
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
                            var innerQOR = context.Execute<NWWCF.Order>(nextOrderLink) as QueryOperationResponse<NWWCF.Order>;
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
                    qor = context.Execute<NWWCF.Customer>(nextCustLink) as QueryOperationResponse<NWWCF.Customer>;
                }

            } while (nextCustLink != null);

            Assert.AreEqual(totalCustomerCount, custCount);
            Assert.AreEqual(totalOrdersCount, orderCount);
            Assert.AreEqual(totalOrdersCount, context.Links.Count);
            Assert.AreEqual(totalCustomerCount + totalOrdersCount, context.Entities.Count);
        }
    }
}
