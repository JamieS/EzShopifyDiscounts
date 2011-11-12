using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FileHelpers;
using System.IO;

// Configure log4net using the App.config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace EzShopifyDiscounts
{
    public static class Program
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Main program entry point
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains) so we can debug https in Fiddler
                //ServicePointManager.ServerCertificateValidationCallback = ((sender2, certificate, chain, sslPolicyErrors) => true);

                string helpText = String.Empty;
                helpText += "\r\n";
                helpText += "EZSD.EXE storename username password [/CREATE | /READ | /DELETE] csvfile\r\n";
                helpText += "  storename Specifies the Shopify store name\r\n";
                helpText += "  username  Specifies the user name used to _log into Shopify\r\n";
                helpText += "  password  Specifies the password used to _log into Shopify\r\n";
                helpText += "  /CREATE   Causes discounts to be created in Shopify\r\n";
                helpText += "  /READ     Causes discounts to be read from Shopify\r\n";
                helpText += "  /DELETE   Causes discounts to be deleted in Shopify\r\n";
                helpText += "  csvfile   Specifies the CSV csvFile to be read from or written to\r\n";
                helpText += "\r\n";

                // Validate Arguments Length
                if (args.Length != 5)
                {
                    Console.WriteLine(helpText);
                    return;
                }

                // Validate StoreName, Username, and Password
                if (args[0].Length < 1 || args[1].Length < 1 || args[2].Length < 1)
                {
                    Console.WriteLine(helpText);
                    return;
                }

                // Validate Action
                string action = args[3].ToUpper();
                if (action != "/CREATE" && action != "/READ" && action != "/DELETE")
                {
                    Console.WriteLine(helpText);
                    return;
                }

                // Validate Filename
                string csvFile = null;
                try
                {
                    csvFile = Path.GetFullPath(args[4]);
                } 
                catch(Exception) {
                    Console.WriteLine(helpText);
                    return;
                }

                // Authenticate with shopify
                AuthInfo authInfo = new AuthInfo() { StoreName = args[0], Username = args[1], Password = args[2] };
                Console.WriteLine("Authenticating...");
                AuthWrapper.Authenticate(authInfo);

                switch (action)
                {
                    case "/CREATE":
                        Console.WriteLine("Creating Discounts...");
                        CreateDiscounts(authInfo, csvFile);
                        break;
                    case "/READ":
                        Console.WriteLine("Reading Discounts...");
                        ReadDiscounts(authInfo, csvFile);
                        break;
                    case "/DELETE":
                        Console.WriteLine("Deleting Discounts...");
                        DeleteDiscounts(authInfo, csvFile);
                        break;
                }
                Console.WriteLine("Done!");

            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Console.WriteLine("Done, but with errors. Check ezsd.log.txt for details.");
            }
        }

        /// <summary>
        /// Helper method to create discounts
        /// </summary>
        private static void CreateDiscounts(AuthInfo authInfo, string csvFile)
        {

            // Read in discount codes from csvFile
            FileHelperEngine fileHelper = new FileHelperEngine(typeof(Discount));
            Discount[] discounts = fileHelper.ReadFile(csvFile) as Discount[];

            // Add discount codes for csvFile
            DiscountsWrapper.CreateDiscounts(authInfo, discounts);
        }

        /// <summary>
        /// Helper method to read discounts
        /// </summary>
        private static void ReadDiscounts(AuthInfo authInfo, string csvFile)
        {
            List<Discount> discounts = new List<Discount>();
            DiscountsWrapper.ReadDiscounts(authInfo, discounts);

            Console.WriteLine("Writing Discounts to file...");
            using (TextWriter writer = new StreamWriter(csvFile))
            {
                // Write file header
                writer.WriteLine(Discount.CsvFileHeader);

                // Write discounts
                FileHelperEngine fileHelper = new FileHelperEngine(typeof(Discount));
                fileHelper.WriteStream(writer, discounts);
            }
        }

        /// <summary>
        /// Helper method to delete discounts
        /// </summary>
        private static void DeleteDiscounts(AuthInfo authInfo, string csvFile)
        {
            // Read in discount codes from csvFile
            FileHelperEngine fileHelper = new FileHelperEngine(typeof(Discount));
            Discount[] discounts = fileHelper.ReadFile(csvFile) as Discount[];

            // Add discount codes for csvFile
            DiscountsWrapper.DeleteDiscounts(authInfo, discounts);
        }

    }
}
