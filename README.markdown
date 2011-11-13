# EzShopifyDiscounts is a tool for bulk handling of Shopify discount codes

Ok, so you have thousands of discount codes that you want to add to your 
Shopify store. How hard can that be? Well, its pretty darn hard since Shopify 
doesn't have any bulk import/export functionality for its discount codes. How 
about you just enter those one at a time by hand using their web interface? I 
didn't think so.

EzShopifyDiscounts is command line tool that allows you to create, read, and
delete Shopify discount codes in bulk via CSV files.

## What platform does it run on?
Microsoft Windows with .NET Framework 4

## How do I Get Started?

* Grab the latest pre-built package [here](https://github.com/pmolchanov/EzShopifyDiscounts/downloads)
* Extract the zip file and open a command window in the application directory.
* Issue the following command: <em>ezsd.exe YourShopifyStoreName YourShopifyUsername YourShopifyPassword /READ CurrentDiscountCodes.csv</em><br/>
(This will download all your current Shopify discount codes to the CurrentDiscountCodes.csv file)
* You can then use this file as a template for CREATE or DELETE operations. HINT: Editing this file is easy in Microsoft Excel.

## How do I Create New Discount Codes?

* Issue the following command: <em>ezsd.exe YourShopifyStoreName YourShopifyUsername YourShopifyPassword /CREATE NewDiscountCodes.csv</em><br/>
(This will create all discount codes contained in the NewDiscountCodes.csv file)
* If you are unsure of how to create a particular type of disount using the CSV format, It's easy to get an example by creating the desired code
using the Shopify web interface and then read it out using this tool.

## How do I Delete Discount Codes?
* First, read a list of discount codes from shopify. This is important because the ID column is required to delete discount codes.
* Now, remove any discounts that you do not want to delete from the CSV file and save it.
* Lastly, issue the following command: <em>ezsd.exe YourShopifyStoreName YourShopifyUsername YourShopifyPassword /DELETE RemoveDiscountCodes.csv</em><br/>
(This will delete all discount codes contained in the RemoveDiscountCodes.csv file)

## I got an error. How can I get more info? 
* Errors are written out to a log file (ezsd.log.txt) in the application directory. Open this to view error details.
* File a bug report and I will fix as soon as I can.

## CSV file format

The CSV file has the following format:

Id, CODE, TYPE, VALUE, APPLIES_TO_TYPE, MINIMUM_ORDER_AMOUNT, APPLIES_TO_ID, STARTS_AT, ENDS_AT, USAGE_COUNT, USAGE_LIMIT, ENABLED

<table>
<tr><th>Field</th><th>Description</th><th>Format</th><th>Create</th><th>Read</th><th>Delete</th></tr>
<tr><td>Id</td><td>Shopify's discount ID.</td><td>Integer.</td><td>Ignored</td><td>Yes</td><td>Required</td></tr>
<tr><td>CODE</td><td>Discount Code</td><td>AlphaNumeric</td><td>Required</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>TYPE</td><td>Discount Type</td><td>fixed_amount, percentage, shipping </td><td>Required</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>VALUE</td><td>Discount Value</td><td>Number. E.g. 99.9 or 99</td><td>Required</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>APPLIES_TO_TYPE</td><td>Discount Restriction</td><td>Country, Collection, Product, minimum_order_amount</td><td>Optional</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>MINIMUM_ORDER_AMOUNT</td><td>Minumum Order Amount</td><td>Number. E.g. 99.9 or 99</td><td>Required</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>APPLIES_TO_ID</td><td>Shopify's Product ID, Collection ID or Country ID.</td><td>Integer</td><td>Optional</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>STARTS_AT</td><td>Discount Start Date</td><td>Date (yyyy-MM-dd)<br/> E.g. 2012-01-01</td><td>Optional</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>ENDS_AT</td><td>Discount End Date</td><td>Date (yyyy-MM-dd)<br/> E.g. 2012-01-01</td><td>Optional</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>USAGE_COUNT</td><td>The number of times a discount has been used</td><td>Integer</td><td>Ignored</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>USAGE_LIMIT</td><td>The maximum number of times that a discount can be used.</td><td>Integer</td><td>Optional</td><td>Yes</td><td>Ignored</td></tr>
<tr><td>ENABLED</td><td>True if a discount is currently enabled, False otherwise. </td><td>Boolean</td><td>Ignored</td><td>Yes</td><td>Ignored</td></tr>
</table>

