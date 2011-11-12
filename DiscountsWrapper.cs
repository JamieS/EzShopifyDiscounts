using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace EzShopifyDiscounts
{
    /// <summary>
    /// Contains logic for creating, reading, and deleting shopify discounts
    /// </summary>
    public static class DiscountsWrapper
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string _discountsUrl = "https://{0}.myshopify.com/admin/discounts/{1}";
        private static string _marketingUrl = "https://{0}.myshopify.com/admin/marketing?page={1}";

        /// <summary>
        /// Creates a single discount in Shopify
        /// </summary>
        public static void CreateDiscount(AuthInfo authInfo, Discount discount)
        {
            try
            {
                Console.Write("Creating {0}: ", discount.Code);

                string discountsUrl = String.Format(_discountsUrl, authInfo.StoreName,"");
                HttpWebRequest request = HttpWebRequest.Create(discountsUrl) as HttpWebRequest;

                // Set required HTTP headers
                request.Accept = "*/*";
                request.Headers["Accept-Language"] = "en-US";
                request.Headers["Accept-Charset"] = "utf-8";
                request.Headers["X-Requested-With"] = "XMLHttpRequest";
                request.Headers["X-Prototype-Version"] = "1.7";
                request.Headers["Cookie"] = String.Format("_secure_session_id={0}", authInfo.SessionId);
                request.Headers["X-CSRF-Token"] = authInfo.CsrfToken;

                // POST the create discount form
                IDictionary<string, string> postData = new Dictionary<string, string>();
                postData["authenticity_token"] = authInfo.CsrfToken;
                postData["discount[code]"] = discount.Code;
                postData["type"] = discount.Type;
                postData["discount[value]"] = discount.Value;
                postData["discount[applies_to_type]"] = discount.AppliesToType;
                postData["discount[minimum_order_amount]"] = discount.MinimumOrderAmount;
                postData["discount[applies_to_id]"] = discount.AppliesToId;
                postData["discount[starts_at]"] = discount.StartsAt;
                postData["discount[ends_at]"] = discount.EndsAt;
                postData["discount[usage_limit]"] = discount.UsageLimit;
                HttpWebResponse response = request.Post(postData);

                // Read response in.
                string responseText = response.GetResponseText();

                // Check for application level errors
                if (responseText.Contains("Messenger.error(\""))
                {
                    string errorMessage = responseText.GetFirstDelimitedValue("Messenger.error(\"", "\");");
                    throw new Exception(errorMessage);
                }

                // Check for success
                string successNotice = String.Format("Messenger.notice(\"Successfully created the discount {0}\\u0026hellip;\");", discount.Code);
                if (!responseText.Contains(successNotice))
                {
                    throw new Exception("Sanity check failed");
                }

                Console.WriteLine("OK");

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");

                string errorMessage = String.Format("Error Creating Discount ({0})", ( discount == null ? "" : discount.Code) );
                throw new Exception(errorMessage, ex);
            }

        }

        /// <summary>
        /// Creates multiple discounts in Shopify
        /// </summary>
        public static void CreateDiscounts(AuthInfo authInfo, IEnumerable<Discount> discounts)
        {
            bool errors = false;
            foreach (Discount discount in discounts)
            {
                // Log errors and continue. We rethrow later.
                try
                {
                    DiscountsWrapper.CreateDiscount(authInfo, discount);
                }
                catch (Exception ex)
                {
                    errors = true;
                    _log.Error(ex);
                }
            }
            if(errors)
            {
                throw new Exception("One or more errors occurred while creating discounts");
            }

        }

        /// <summary>
        /// Reads all discounts from Shopify
        /// </summary>
        public static void ReadDiscounts(AuthInfo authInfo, IList<Discount> discounts)
        {
            bool errors = false;

            for (int pageNumber = 1; true; pageNumber++)
            {
                try
                {
                    Console.WriteLine("Processing Page {0}", pageNumber);

                    string marketingUrl = String.Format(_marketingUrl, authInfo.StoreName, pageNumber);
                    HttpWebRequest request = HttpWebRequest.Create(marketingUrl) as HttpWebRequest;

                    // Set required HTTP headers
                    request.Accept = "*/*";
                    request.Headers["Accept-Language"] = "en-US";
                    request.Headers["Accept-Charset"] = "utf-8";
                    request.Headers["Cookie"] = String.Format("_secure_session_id={0}", authInfo.SessionId);
                    request.Headers["X-CSRF-Token"] = authInfo.CsrfToken;

                    // Read response
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    HtmlDocument html = response.GetResponseHtml();

                    // Parse out the discounts rows from the Coupons table
                    HtmlNodeCollection discountRows = html.DocumentNode.SelectNodes("//table[@id=\"coupons\"]/tbody/tr");

                    // If the first row does not have 4 columns, we no discounts for this page.
                    if (discountRows[0].SelectNodes("td").Count != 4)
                    {
                        break;
                    }

                    // Each row is a discount.
                    foreach (HtmlNode discountRow in discountRows)
                    {
                        // Log errors and continue. We rethrow later.
                        try
                        {
                            Discount discount = ParseDiscount(discountRow);
                            discounts.Add(discount);
                        }
                        catch (Exception ex)
                        {
                            errors = true;
                            _log.Error(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = String.Format("Error Processing Page {0}", pageNumber);
                    throw new Exception(errorMessage, ex);
                }
            }

            if (errors)
            {
                throw new Exception("One or more errors occurred while reading discounts");
            }
        }

        /// <summary>
        /// Helper method to parse the HTML screen scraped from the Shopify discounts page.
        /// </summary>
        private static Discount ParseDiscount(HtmlNode discountRow)
        {
            Discount discount = new Discount();

            Console.Write("Parsing Discount: ");
            try
            {
                string temp = null;

                // Id
                discount.Id = discountRow.Attributes["id"].Value.Replace("discount-", "");

                // Code
                discount.Code = discountRow.SelectSingleNode("td[position()=1]//strong").InnerHtml.Trim();

                // MinimumOrderAmount
                discount.MinimumOrderAmount = "0"; // Required

                temp = discountRow.SelectSingleNode("td[position()=2]").InnerHtml.Trim();
                if (temp.Contains("free shipping to"))
                {
                    // Type
                    discount.Type = "shipping";

                    // Value
                    Match match = Regex.Match(temp, "\\$[0-9]+\\.[0-9]{2}");
                    if (match.Success)
                    {
                        discount.Value = match.Value.Replace("$", "");
                    }

                    temp = discountRow.SelectSingleNode("td[position()=2]//strong").InnerHtml.Trim();
                    switch (temp)
                    {
                        case "Anywhere":

                            // AppliesToId
                            discount.AppliesToId = String.Empty;

                            break;
                        case "Rest of World":

                            // AppliesToId
                            discount.AppliesToId = "2848952";

                            // AppliesToType
                            discount.AppliesToType = "Country";

                            break;
                        case "United States":

                            // AppliesToId
                            discount.AppliesToId = "2848942";

                            // AppliesToType
                            discount.AppliesToType = "Country";

                            break;
                        // TODO: Other countries here???
                    }
                }
                else
                {
                    temp = discountRow.SelectSingleNode("td[position()=2]//strong").InnerHtml.Trim();
                    if (temp.Contains('$'))
                    {
                        // Type
                        discount.Type = "fixed_amount";

                        // Value
                        discount.Value = temp.Replace("$", "");
                    }
                    else if (temp.Contains('%'))
                    {
                        // Type
                        discount.Type = "percentage";

                        // Value
                        discount.Value = temp.Replace("%", "").Trim();
                    }

                    temp = discountRow.SelectSingleNode("td[position()=2]").OuterHtml.GetFirstDelimitedValue("</strong>", "</td>").ToLower();
                    if (temp.Contains("off of the collection"))
                    {
                        // AppliesToType
                        discount.AppliesToType = "Collection"; // Collection
                    }
                    else if (temp.Contains("off of"))
                    {
                        // AppliesToType
                        discount.AppliesToType = "Product"; // Product
                    }
                    else if (temp.Contains("off orders equal or above"))
                    {
                        // AppliesToType
                        discount.AppliesToType = "minimum_order_amount";

                        // MinimumOrderAmount
                        discount.MinimumOrderAmount = temp.Substring(temp.IndexOf('$') + 1).Trim();
                    }


                    if (discount.AppliesToType == "Product" || discount.AppliesToType == "Collection")
                    {
                        temp = discountRow.SelectSingleNode("td[position()=2]//a").Attributes["href"].Value;

                        // AppliesToId
                        discount.AppliesToId = temp.Substring(temp.LastIndexOf('/') + 1).Trim();
                    }
                }

                HtmlNodeCollection details = discountRow.SelectNodes("td[position()=3]//li");
                if (details != null)
                {
                    foreach (HtmlNode li in details)
                    {
                        if (Regex.IsMatch(li.InnerText, "Used [0-9]+ time[s]?"))
                        {
                            // UsageCount
                            discount.UsageCount = Regex.Match(li.InnerText, "[0-9]+").Value;
                        }
                        else if (Regex.IsMatch(li.InnerText, "[0-9]+ use[s]? remaining"))
                        {
                            // UsageLimit
                            temp = Regex.Match(li.InnerText, "[0-9]+").Value; // Uses Remaining
                            discount.UsageLimit = (Int32.Parse(discount.UsageCount) + Int32.Parse(temp)).ToString();
                        }
                        else if (Regex.IsMatch(li.InnerText, "Starts [A-Za-z0-9 ]+, ends [A-Za-z0-9 ]+"))
                        {
                            // StartsAt
                            discount.StartsAt = (DateTime.Parse(Regex.Matches(li.InnerText, "[A-Za-z]{3} [0-9]{2}( [0-9]{4})?")[0].Value)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                            // EndsAt
                            discount.EndsAt = (DateTime.Parse(Regex.Matches(li.InnerText, "[A-Za-z]{3} [0-9]{2}( [0-9]{4})?")[1].Value)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (li.InnerText.StartsWith("Starts"))
                        {
                            // StartAt
                            discount.StartsAt = (DateTime.Parse(Regex.Match(li.InnerText, "[A-Za-z]{3} [0-9]{2}( [0-9]{4})?").Value)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (li.InnerText.StartsWith("Ends"))
                        {
                            // EndsAt
                            discount.EndsAt = (DateTime.Parse(Regex.Match(li.InnerText, "[A-Za-z]{3} [0-9]{2}( [0-9]{4})?").Value)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        }

                    }
                }

                // Enabled
                discount.Enabled = discountRow.SelectSingleNode("td[position()=4]//a[position()=1]").InnerHtml.Equals("Disable discount").ToString();

                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");

                string errorMessage = String.Format("Error Parsing Discount ({0})", (discount == null ? "" : discount.Code));
                throw new Exception(errorMessage, ex);
            }
            return discount;
        }

        /// <summary>
        /// Deletes a discount in Shopify
        /// </summary>
        public static void DeleteDiscount(AuthInfo authInfo, Discount discount)
        {
            try
            {
                Console.Write("Deleting {0}: ", discount.Code);

                string discountsUrl = String.Format(_discountsUrl, authInfo.StoreName, discount.Id);
                HttpWebRequest request = HttpWebRequest.Create(discountsUrl) as HttpWebRequest;

                // Set required HTTP headers
                request.Accept = "*/*";
                request.Headers["Accept-Language"] = "en-US";
                request.Headers["Accept-Charset"] = "utf-8";
                request.Headers["X-Requested-With"] = "XMLHttpRequest";
                request.Headers["X-Prototype-Version"] = "1.7";
                request.Headers["Cookie"] = String.Format("_secure_session_id={0}", authInfo.SessionId);
                request.Headers["X-CSRF-Token"] = authInfo.CsrfToken;

                // POST the create discount form
                IDictionary<string, string> postData = new Dictionary<string, string>();
                postData["authenticity_token"] = authInfo.CsrfToken;
                postData["_method"] = "delete";
                HttpWebResponse response = request.Post(postData);

                // Read response in.
                string responseText = response.GetResponseText();

                // Check for application level errors
                if (responseText.Contains("Messenger.error(\""))
                {
                    string errorMessage = responseText.GetFirstDelimitedValue("Messenger.error(\"", "\");");
                    throw new Exception(errorMessage);
                }

                // Check for success
                string successNotice = String.Format("Messenger.notice(\"Deleted discount {0}\\u0026hellip;\");", discount.Code);
                if (!responseText.Contains(successNotice))
                {
                    throw new Exception("Sanity check failed");
                }

                Console.WriteLine("OK");

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");

                string errorMessage = String.Format("Error Deleting Discount ({0})", (discount == null ? "" : discount.Code));
                throw new Exception(errorMessage, ex);
            }
        }

        /// <summary>
        /// Deletes multiple discounts in Shopify
        /// </summary>
        public static void DeleteDiscounts(AuthInfo authInfo, IEnumerable<Discount> discounts)
        {
            bool errors = false;
            foreach (Discount discount in discounts)
            {
                // Log errors and continue. We rethrow later.
                try
                {
                    DiscountsWrapper.DeleteDiscount(authInfo, discount);
                }
                catch (Exception ex)
                {
                    errors = true;
                    _log.Error(ex);
                }
            }
            if (errors)
            {
                throw new Exception("One or more errors occurred while deleting discounts");
            }
        }
    }
}
