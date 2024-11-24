namespace Portfolio.Services
{
    //using AspNetCorePowerBI;
    using Microsoft.Identity.Client;
    //using NuGet.Configuration;
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    // using Portal;
    public class AadService
    {
        private static readonly string m_authorityUrl = "https://login.microsoftonline.com/organizations";//ConfigurationManager.AppSettings["authorityUrl"];
        private static readonly string[] m_scope = "https://analysis.windows.net/powerbi/api/.default".Split(';');//ConfigurationManager.AppSettings["scope"].Split(';');

        private static readonly string m_applicationId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        /// <summary>
        /// Get Access token
        /// </summary>
        /// <returns>Access token</returns>
        public static async Task<string> GetAccessToken()
        {
            

            AuthenticationResult authenticationResult = null;
            IPublicClientApplication clientApp = PublicClientApplicationBuilder
                                                                    .Create(m_applicationId)
                                                                    .WithAuthority(m_authorityUrl)
                                                                    .Build();
            
            authenticationResult = await clientApp.AcquireTokenByUsernamePassword(m_scope, "username@domain.com", "complexpassword").ExecuteAsync();
 
            return authenticationResult.AccessToken;
        }

        private static readonly string m_authorityUrlAzure = "https://login.microsoftonline.com/organizations";//ConfigurationManager.AppSettings["authorityUrl"];
        //private static readonly string[] m_scopeAzure = "https://management.azure.com/user_impersonation".Split(';');// https://analysis.windows.net/powerbi/api/.default".Split(';');//ConfigurationManager.AppSettings["scope"].Split(';');
        private static readonly string[] m_scopeAzure = "https://management.azure.com/.default".Split(';');// https://analysis.windows.net/powerbi/api/.default".Split(';');//ConfigurationManager.AppSettings["scope"].Split(';');

        //private static readonly string[] m_scopeAzure = "https://management.azure.com/.default; https://analysis.windows.net/powerbi/api/.default".Split(';');


        private static readonly string m_applicationIdAzure = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        //tenentID
        private static readonly string m_tenentId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

        public static async Task<string> GetAzureAccessToken()
        {
            try
            {

                //need to passin azure logn details?

                AuthenticationResult authenticationResult = null;
                IPublicClientApplication clientApp = PublicClientApplicationBuilder
                                                                    .Create(m_applicationIdAzure)
                                                                    .WithAuthority(m_authorityUrlAzure, true)
                                                                    .Build();
                authenticationResult = await clientApp.AcquireTokenByUsernamePassword(m_scopeAzure, "username@domain.com", "complexpassword").ExecuteAsync();

                return authenticationResult.AccessToken;
            }
            catch (Exception exception)
            {
                //error
                var x = exception;
                return null;
            }

        }
    }
}
