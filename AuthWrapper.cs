using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

namespace EzShopifyDiscounts
{
    /// <summary>
    /// Contains logic for authenticating with Shopify
    /// </summary>
    public static class AuthWrapper
    {
        private static string _loginUrl = "https://{0}.myshopify.com/admin/auth/login";

        /// <summary>
        /// Posts the Shopify store login page and populates the authInfo object with the
        /// session and CSRF tokens parsed from the reponse.
        /// </summary>
        public static void Authenticate(AuthInfo authInfo)
        {
            string loginUrl = null;
            try
            {
                loginUrl = String.Format(_loginUrl, authInfo.StoreName);
                HttpWebRequest request = HttpWebRequest.Create(loginUrl) as HttpWebRequest;
                request.CookieContainer = new CookieContainer();

                // POST the login form
                IDictionary<string, string> postData = new Dictionary<string, string>();
                postData["login"] = authInfo.Username;
                postData["password"] = authInfo.Password;
                HttpWebResponse response = request.Post(postData);

                // Get the session cookie
                Cookie sessionIdCookie = request.CookieContainer.GetCookies(new Uri(loginUrl))["_secure_session_id"];
                authInfo.SessionId = sessionIdCookie.Value;

                // Read response in
                HtmlDocument html = response.GetResponseHtml();

                // Do a quick sanity check to make sure that we are logged in.
                // (If there is a logout link, its a safe bet to assume that we are logged in)
                HtmlNode logoutLink = html.DocumentNode.SelectSingleNode("//a[@href=\"/admin/auth/logout\"]");
                if (logoutLink == null)
                {
                    throw new Exception("Login Failed");
                }

                // Pluck out the csrf token.
                HtmlNode hiddenCsrfInput = html.DocumentNode.SelectSingleNode("//meta[@name=\"csrf-token\"]");
                authInfo.CsrfToken = WebUtility.HtmlDecode(hiddenCsrfInput.Attributes["content"].Value);

            }
            catch (Exception ex)
            {
                throw new Exception("Error logging into shopify", ex);
            }
        }

    }
}
