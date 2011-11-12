using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using HtmlAgilityPack;

namespace EzShopifyDiscounts
{
    /// <summary>
    /// Extension methods to the String Class
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parses delimited values. For example "[test]".GetFirstDelimitedValue("[","]") returns "test"
        /// TODO: Enhance the robustness of this method by using Regex's
        /// </summary>
        public static string GetFirstDelimitedValue(this string str, string lDelim, string rDelim)
        {
            string value = null;

            try
            {
                int leftIdx = str.IndexOf(lDelim);
                int rightIdx = str.IndexOf(rDelim, leftIdx);
                value = str.Substring((leftIdx + lDelim.Length), (rightIdx - leftIdx - lDelim.Length));
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting delimited value",ex);
            }

            return value;
        }
    }

    /// <summary>
    /// Extension methods to the HttpWebResponse Class
    /// </summary>
    public static class HttpWebResponseExtensions
    {

        /// <summary>
        /// Gets the response as a string
        /// </summary>
        public static string GetResponseText(this HttpWebResponse response)
        {
            string responseText = null;

            try
            {
                using (StreamReader responseBodyStream = new StreamReader(response.GetResponseStream()))
                {
                    responseText = responseBodyStream.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting response text",ex);
            }

            return responseText;
        }

        /// <summary>
        /// Gets the response as a HtmlDocument
        /// </summary>
        public static HtmlDocument GetResponseHtml(this HttpWebResponse response)
        {
            HtmlDocument htmlDoc = null;

            try
            {
                htmlDoc = new HtmlDocument();
                htmlDoc.Load(response.GetResponseStream());
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing response stream into HTML Document", ex);
            }

            return htmlDoc;
        }          

    }

    /// <summary>
    /// Extension methods to the HttpWebRequest Class
    /// </summary>
    public static class HttpWebRequestExtensions
    {

        /// <summary>
        /// Utility method simplify POSTing data. Sets all required headers and encodes post data.
        /// </summary>
        public static HttpWebResponse Post(this HttpWebRequest request, IDictionary<string, string> postData)
        {
            HttpWebResponse response = null;
            
            try
            {
                // Make this request a POST
                request.Method = "POST";

                // URL Encode each property in the post data
                string formEncodedData = String.Empty;
                foreach (var kvpair in postData)
                {
                    // URL Encode
                    formEncodedData += String.Format("{0}={1}&", HttpUtility.UrlEncode(kvpair.Key), HttpUtility.UrlEncode(kvpair.Value));
                }

                // Set some headers
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.ContentLength = formEncodedData.Length;

                // Add the post data to thre request body
                using (Stream writeStream = request.GetRequestStream())
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] bytes = encoding.GetBytes(formEncodedData);
                    writeStream.Write(bytes, 0, bytes.Length);
                }

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                throw new Exception("Error making POST request", ex);
            }

            return response;
        }
    }
}
