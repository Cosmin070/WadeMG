using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFGraph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string aadInstance = "https://login.microsoftonline.com/{0}";
        private static string tenant = "jumpybugsoutlook.onmicrosoft.com";
        private static string clientId = "72068b6d-d05e-44b2-863c-bf1b85f98692";
        Uri redirectUri = new Uri("http://localhost:4200");
        private static string serviceResourceID = "https://graph.microsoft.com";
        private AuthenticationContext authContext = null;
        private AuthenticationResult result = null;
        public MainWindow()
        {
            InitializeComponent();
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            authContext = new AuthenticationContext(authority, new FileCache());
        }

        private async void callServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (result == null)
            {
                serviceResult.Text = "sign in first";
                return;
            }
            try
            {
                result = await authContext.AcquireTokenSilentAsync(serviceResourceID, clientId);
            }
            catch (AdalException ex)
            {
                serviceResult.Text = ex.ToString();
                return;
            }

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.AccessToken);

            HttpResponseMessage response =
                await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            if (response.IsSuccessStatusCode)
            {
                string s = await response.Content.ReadAsStringAsync();
                serviceResult.Text = s;
            }
            else
            {
                serviceResult.Text = "An error occured: " + response.ReasonPhrase;
            }
        }

        private async void signInButton_Click(object sender, RoutedEventArgs e)
        {
            if (signInButton.Content.ToString() == "Sign out")
            {
                authContext.TokenCache.Clear();
                ClearCookies();
                signInButton.Content = "Sign in";
                return;
            }
            try
            {
                var platformParams = new PlatformParameters(PromptBehavior.Always);
                result = await authContext.AcquireTokenAsync(serviceResourceID, clientId, redirectUri, platformParams);
                signInButton.Content = "Sign out";
            }
            catch (AdalException ex)
            {
                serviceResult.Text = ex.ToString();
            }
        }

        private void ClearCookies()
        {
            const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
    }
}
