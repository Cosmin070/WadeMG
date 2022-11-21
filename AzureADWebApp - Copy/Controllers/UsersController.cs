using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;

namespace AzureADWebApp.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string appKey = ConfigurationManager.AppSettings["ida:appKey"];

        // GET: Users
        public async Task<ActionResult> Index()
        {
            string authority = String.Format(
                CultureInfo.InvariantCulture, aadInstance, tenant);
            AuthenticationContext authContext = new AuthenticationContext(authority);
            AuthenticationResult result = null;

            int retryCount = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    result = await authContext.AcquireTokenAsync(
                        "https://graph.microsoft.com",
                        new ClientCredential(clientId, appKey));
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }
                }
            } while ((retry == true) && (retryCount < 3));


            if (result == null)
            {
                ViewBag.ErrorMessage = "UnexpectedError";
                return View("Index");
            }

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string r = await response.Content.ReadAsStringAsync();
                ViewBag.Results = r;
                return View("Index");
            }
            else
            {
                string r = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    authContext.TokenCache.Clear();
                }
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View("Index");
            }
        }
    }
}