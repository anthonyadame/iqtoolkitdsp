
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Services;
using System.Data.Services.Common;
using System.ComponentModel.DataAnnotations;

namespace IQTWcf
{

    using IQToolkit;
    using IQToolkit.Data;
    using IQToolkit.Data.Mapping;
    

    [DataServiceKey("CategoryID")]
    [Serializable]
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public IList<Product> Products { get; set; }
    }

    [DataServiceKey("CustomerID","CustomerTypeID")]
    [Serializable]
    public partial class CustomerCustomerDemo
    {
        public string CustomerID { get; set; }
        public string CustomerTypeID { get; set; }
        public Customer Customer { get; set; }
        public IList<CustomerDemographic> CustomerDemographics { get; set; }
    }

    [DataServiceKey("CustomerTypeID")]
    [Serializable]
    public partial class CustomerDemographic
    {
        public string CustomerDesc { get; set; }
        public string CustomerTypeID { get; set; }
        public IList<CustomerCustomerDemo> CustomerCustomerDemos { get; set; }
    }

    [DataServiceKey("CustomerID")]
    [ETagAttribute("Address", "City", "CompanyName", "ContactTitle", "Country")]
    [Serializable]
    public class Customer
    {
        [StringLength(60, ErrorMessage = "Address maximum length 60, maximum length exceed.")]
        public string Address { get; set; }
        [StringLength(15, ErrorMessage = "City maximum length 15, maximum length exceed.")]
        public string City { get; set; }
        [StringLength(40, ErrorMessage = "CompanyName maximum length 15, maximum length exceed."), 
         Required(AllowEmptyStrings = false, ErrorMessage = "CompanyName is a required field.")]
        public string CompanyName { get; set; }
        [StringLength(30, ErrorMessage = "ContactName maximum length 30, maximum length exceed.")]
        public string ContactName { get; set; }
        [StringLength(30, ErrorMessage = "ContactTitle maximum length 30, maximum length exceed.")]
        public string ContactTitle { get; set; }
        [StringLength(15, ErrorMessage = "Country maximum length 15, maximum length exceed.")]
        public string Country { get; set; }
        [StringLength(5, ErrorMessage = "CustomerID maximum length 5, maximum length exceed."), 
         Required(AllowEmptyStrings = false, ErrorMessage = "CustomerID is a required field.")]
        public string CustomerID { get; set; }
        [StringLength(24, ErrorMessage = "Fax maximum length 24, maximum length exceed.")]
        public string Fax { get; set; }
        [StringLength(24, ErrorMessage = "Phone maximum length 24, maximum length exceed.")]
        public string Phone { get; set; }
        [StringLength(10, ErrorMessage = "PostalCode maximum length 10, maximum length exceed.")]
        public string PostalCode { get; set; }
        [StringLength(15, ErrorMessage = "Region maximum length 15, maximum length exceed.")]
        public string Region { get; set; }
        public IList<CustomerCustomerDemo> CustomerCustomerDemos { get; set; }
        public IList<Order> Orders { get; set; }
    }

	[DataServiceKey("EmployeeID")]
	[Serializable]
	public partial class Employee {
		public string Address { get; set; }
		public DateTime BirthDate { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
		public int EmployeeID { get; set; }
		public string Extension { get; set; }
		public string FirstName { get; set; }
		public System.Nullable<DateTime> HireDate { get; set; }
		public string HomePhone { get; set; }
		public string LastName { get; set; }
		public string Notes { get; set; }
		public byte[] Photo { get; set; }
		public string PhotoPath { get; set; }
		public string PostalCode { get; set; }
		public string Region { get; set; }
		public int ReportsTo { get; set; }
		public string Title { get; set; }
		public string TitleOfCourtesy { get; set; }
        public Employee ReportsToEmployee { get; set; }
        public IList<EmployeeTerritory> EmployeeTerritories { get; set; }
        public IList<Order> Orders { get; set; }
        public IList<Employee> ReportsToChildren { get; set; }
	}

    [DataServiceKey("EmployeeID","TerritoryID")]
    [Serializable]
    public partial class EmployeeTerritory
    {
        public int EmployeeID { get; set; }
        public string TerritoryID { get; set; }
        public Employee Employee { get; set; }
        public Territory Territory { get; set; }
    }

    [DataServiceKey("OrderID","ProductID")]
    [Serializable]
    public partial class OrderDetail
    {
        public Single Discount { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
    }

	[DataServiceKey("OrderID")]
	[Serializable]
	public partial class Order {
		public string CustomerID { get; set; }
		public int EmployeeID { get; set; }
		public decimal Freight { get; set; }
		public DateTime OrderDate { get; set; }
		public int OrderID { get; set; }
		public DateTime RequiredDate { get; set; }
		public string ShipAddress { get; set; }
		public string ShipCity { get; set; }
		public string ShipCountry { get; set; }
		public string ShipName { get; set; }
		public DateTime ShippedDate { get; set; }
		public string ShipPostalCode { get; set; }
		public string ShipRegion { get; set; }
		public int ShipVia { get; set; }
		public Customer Customer { get; set; }
		public Employee Employee { get; set; }
        public IList<OrderDetail> OrderDetails { get; set; }
        public Shipper ShipViaShipper { get; set; }					
	}

    [DataServiceKey("ProductID")]
    [Serializable]
    public class Product
    { 
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public int ReorderLevel { get; set; } 
        public bool Discontinued { get; set; }
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public IList<OrderDetail> OrderDetails { get; set; }
    }

    [DataServiceKey("RegionID")]
    [Serializable]
    public partial class Region
    {
        public string RegionDescription { get; set; }
        public int RegionID { get; set; }
        public IList<Territory> Territories { get; set; }
    }

    [DataServiceKey("ShipperID")]
    [Serializable]
    public partial class Shipper
    {
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public int ShipperID { get; set; }
        public IList<Order> ShipViaOrders { get; set; }
    }

    [DataServiceKey("SupplierID")]
    [Serializable]
    public class Supplier
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string HomePage { get; set; }
        public IList<Product> Products { get; set; }
    }

    [DataServiceKey("TerritoryID")]
    [Serializable]
    public partial class Territory
    {
        public int RegionID { get; set; }
        public string TerritoryDescription { get; set; }
        public string TerritoryID { get; set; }
        public Region Region { get; set; }
        public IList<EmployeeTerritory> EmployeeTerritories { get; set; }
    }

}
