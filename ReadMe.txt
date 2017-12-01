1.11.2011	IQToolkitDSP

Basic premise is to build a OData provider that converts Wcf/OData expressions into a IQToolKit usable expressions.

Plug in a IQToolkit provider, add a data mapping and basic POCO class to expose your data.

 IQToolKit providers...
 Oracle
 Sql Server
 Sql Server CE
 SQLite
 MySql
 Access

Resources: (Thanks... )

OData SDK
http://www.odata.org/developers/odata-sdk

IQToolkit by mattwar
http://www.codeplex.com/IQToolkit
http://blogs.msdn.com/mattwar/pages/linq-links.aspx

IQToolkit Contrib by TomBrothers
http://iqtoolkitcontrib.codeplex.com/
http://www.tpisolutions.com/blog/CategoryView,category,IQToolkitContrib.aspx

IQToolkit.Data.OracleClient by WiCKY Hu
http://code.google.com/p/iqtoolkit-oracle/


Know Issues/Items to note:

IQToolkit source vs iqtoolkit-oracle
===========================
This build includes updates to IQToolkit to accomidate IQToolkit.Data.OracleClient.
A simple code compare on the source available the following sites should reveal the differences. 

http://www.codeplex.com/IQToolkit
http://code.google.com/p/iqtoolkit-oracle/


IQToolkit.Data.OracleClient
===========================
A SkipToRowNumberRewriterOraXE has been added, the default SkipToRowNumberRewriter column name "_row_num"
would not process in Oracle XE


History:
Change Set Date		Comment
Apr 06				Added database level concurrency updatecheck and extended the IQToolkit EntitySession
Jan 27				Moved IQToolkit dll's to resource location
Jan 27 				Update to add DataAnnotations validation support and basic comments to source
Jan 25				ETag additions, basic concurrency check
Jan 21				Init svn IQToolkitDSP




Common issues with Odata/Wcf and IQToolkit:

Metadata model used in Odata adds an additional level of complexity
===================================================================
http://iqtoolkit.codeplex.com/Thread/View.aspx?ThreadId=208434
Dynamically generated entity Class Vs. Property Bag   

I'm trying to create a WCF Data Service that leverages IQToolkit's SqlClient 
to access "resources" in the database.  My problem is I don't have existing 
CLR objects that represent my entities.  I can think of two routes to take to 
utilize IQToolkit:

	1. Dynamically generate entity classes based off of my metadata, then translate 
	the incoming query into a query based on these classes...let IQToolkit work its magic.

	2. Rework the SQLProvider to work against untyped objects (well, against a generic 
	"Resource" object that contains a property bag), then translate the incoming query 
	into a query based on this generic "Resource" type.

It would appear option #1 would be the easier of the options to accomplish, 
but option 2 seems more general.

Any thoughts?



WCF Data Service mangled my select statement…
===================================================================
http://www.tpisolutions.com/blog/2010/02/24/WCFDataServiceMangledMySelectStatement.aspx


URL: http://localhost:3879/IQTORA.svc/Customers()?$filter=toupper(trim(CustomerID)) eq 'ALFKI'
Before: Query(IQTWcf.Customer).Where(it => (Convert(GetValue(it, value(System.Data.Services.Providers.ResourceProperty))).Trim().ToUpper() == "ALFKI"))
After: Query(IQTWcf.Customer).Where(it => (it.CustomerID.Trim().ToUpper() == "ALFKI"))
SELECT t0."ADDRESS", t0."CITY", t0."COMPANYNAME", t0."CONTACTNAME", t0."CONTACTTITLE", t0."COUNTRY", t0."CUSTOMERID", t0."FAX", t0."PHONE", t0."POSTALCODE", t0."REGION"
FROM "NORTHWIND"."CUSTOMERS" t0
WHERE (UPPER(TRIM(t0."CUSTOMERID")) = :p0)
-- p0 = [ALFKI]



IQToolkit + WCF Data Service  
===================================================================
http://iqtoolkit.codeplex.com/Thread/View.aspx?ThreadId=231157

I’m trying to use the IQToolkit along with WCF data services. 
They work very well together, but the IQToolkit isn’t able to process the following command:
...

URL: http://localhost:3879/IQTORA.svc/Categories(2)/Products()
Before: Query(IQTWcf.Category).Where(element => (Convert(GetValue(element, value(System.Data.Services.Providers.ResourceProperty))) == 2)).SelectMany(element => GetSequenceValue(element, value(System.Data.Services.Providers.ResourceProperty)))
After: Query(IQTWcf.Category).Where(element => (element.CategoryID == 2)).SelectMany(element => element.Products)

SELECT t1.[CategoryID], t1.[Discontinued], t1.[ProductID], t1.[ProductName], t1.[QuantityPerUnit], t1.[ReorderLevel], t1.[SupplierID], t1.[UnitPrice], t1.[UnitsInStock], t1.[UnitsOnOrder]
FROM [NORTHWIND].[dbo].[Categories] AS t0
INNER JOIN [NORTHWIND].[dbo].[Products] AS t1
  ON (t1.[CategoryID] = t0.[CategoryID])
WHERE (t0.[CategoryID] = 2)


