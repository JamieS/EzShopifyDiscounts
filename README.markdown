EzShopifyDiscounts is a tool for bulk handling of Shopify discount codes
========================================================================

Ok, so you have thousands of discount codes that you want to add to your 
Shopify store. How hard can that be? Well, its pretty darn hard since Shopify 
doesn't have any bulk import/export functionality for its discount codes. How 
about you just enter those by hand one at a time using their web interface? I 
didn't think so.

EzShopifyDiscounts is command line tool that allows you to create, read, and
delete Shopify discount codes in bulk via CSV files.

What platform does it run on?
-----------------------------
Windows with .NET Framework 4

How do I Install it?
--------------------
Just unzip the zip file. All dependencies are included.

How do I use it?
----------------
The easiest way to get started is to create a few discount codes in Shopify,
and then export the codes with the following command:

  ezsd.exe MyShopifyStoreName MyUserName MyPassword /READ MyShopifyDiscounts.csv
  
This reads the discount codes from Shopify and stores then in the 
MyShopifyDiscounts.csv file. You can then use this file as a template for
adding or deleting other discount codes.
