using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Gov.Lclb.Cllb.Interfaces
{
    public static class DynamicsSetupUtil
    {
        /// <summary>
        /// Retrieves <see cref="ServiceClientCredentials"/> for the Dynamics client using Dataverse authentication.
        /// </summary>
        /// <param name="Configuration">The application configuration (IConfiguration).</param>
        /// <returns>ServiceClientCredentials or null if credentials could not be resolved.</returns>
        public static ServiceClientCredentials GetDataverseTokenCredentials(
            IConfiguration Configuration
        )
        {
            // Cloud - x.dynamics.com
            string aadTenantId = Configuration["DYNAMICS_AAD_TENANT_ID"]; // Cloud AAD Tenant ID
            string serverAppIdUri = Configuration["DYNAMICS_SERVER_APP_ID_URI"]; // Cloud Server App ID URI
            string appRegistrationClientKey = Configuration["DYNAMICS_APP_REG_CLIENT_KEY"]; // Cloud App Registration Client Key
            string appRegistrationClientId = Configuration["DYNAMICS_APP_REG_CLIENT_ID"]; // Cloud App Registration Client Id

            if (
                string.IsNullOrEmpty(appRegistrationClientId)
                || string.IsNullOrEmpty(appRegistrationClientKey)
                || string.IsNullOrEmpty(serverAppIdUri)
                || string.IsNullOrEmpty(aadTenantId)
            )
            {
                // Missing required configuration settings
                return null;
            }

            // New v2.0 OAuth flow for Dataverse
            var tokenUrl = $"https://login.microsoftonline.com/{aadTenantId}/oauth2/v2.0/token";
            using var httpClient = new HttpClient();
            var pairs = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", appRegistrationClientId },
                { "client_secret", appRegistrationClientKey },
                { "scope", $"{serverAppIdUri}/.default" }
            };

            var content = new FormUrlEncodedContent(pairs);

            var response = httpClient.PostAsync(tokenUrl, content).Result;
            var resultContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Dataverse token request failed: {resultContent}");
            }

            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultContent);
            string token = result["access_token"].ToString();

            return new TokenCredentials(token);
        }

        /// <summary>
        /// Get the ServiceClientCredentials for the Dynamics client using the ADFS 2016 (for On Prem) authentication method.
        /// </summary>
        /// <param name="Configuration">The application configuration (IConfiguration).</param>
        /// <returns>ServiceClientCredentials or null if credentials could not be resolved.</returns>
        public static ServiceClientCredentials GetOnPremTokenCredentials(
            IConfiguration Configuration
        )
        {
            // One Premise ADFS (2016)
            string adfsOauth2Uri = Configuration["ADFS_OAUTH2_URI"]; // ADFS OAUTH2 URI - usually /adfs/oauth2/token on STS
            string applicationGroupResource = Configuration["DYNAMICS_APP_GROUP_RESOURCE"]; // ADFS 2016 Application Group resource (URI)
            string applicationGroupClientId = Configuration["DYNAMICS_APP_GROUP_CLIENT_ID"]; // ADFS 2016 Application Group Client ID
            string applicationGroupSecret = Configuration["DYNAMICS_APP_GROUP_SECRET"]; // ADFS 2016 Application Group Secret
            string serviceAccountUsername = Configuration["DYNAMICS_USERNAME"]; // Service account username
            string serviceAccountPassword = Configuration["DYNAMICS_PASSWORD"]; // Service account password

            if (
                string.IsNullOrEmpty(adfsOauth2Uri)
                || string.IsNullOrEmpty(applicationGroupResource)
                || string.IsNullOrEmpty(applicationGroupClientId)
                || string.IsNullOrEmpty(applicationGroupSecret)
                || string.IsNullOrEmpty(serviceAccountUsername)
                || string.IsNullOrEmpty(serviceAccountPassword)
            )
            {
                // Missing required configuration settings
                return null;
            }

            // create a new HTTP client that is just used to get a token.
            var stsClient = new HttpClient();

            //stsClient.DefaultRequestHeaders.Add("x-client-SKU", "PCL.CoreCLR");
            //stsClient.DefaultRequestHeaders.Add("x-client-Ver", "5.1.0.0");
            //stsClient.DefaultRequestHeaders.Add("x-ms-PKeyAuth", "1.0");

            stsClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
            stsClient.DefaultRequestHeaders.Add("return-client-request-id", "true");
            stsClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Construct the body of the request
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("resource", applicationGroupResource),
                new KeyValuePair<string, string>("client_id", applicationGroupClientId),
                new KeyValuePair<string, string>("client_secret", applicationGroupSecret),
                new KeyValuePair<string, string>("username", serviceAccountUsername),
                new KeyValuePair<string, string>("password", serviceAccountPassword),
                new KeyValuePair<string, string>("scope", "openid"),
                new KeyValuePair<string, string>("response_mode", "form_post"),
                new KeyValuePair<string, string>("grant_type", "password")
            };

            // This will also set the content type of the request
            var content = new FormUrlEncodedContent(pairs);
            // send the request to the ADFS server
            var _httpResponse = stsClient
                .PostAsync(adfsOauth2Uri, content)
                .GetAwaiter()
                .GetResult();
            var _responseContent = _httpResponse
                .Content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
            // response should be in JSON format.
            try
            {
                Dictionary<string, string> result = JsonConvert.DeserializeObject<
                    Dictionary<string, string>
                >(_responseContent);
                string token = result["access_token"];

                return new TokenCredentials(token);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " " + _responseContent);
            }
        }

        /// <summary>
        /// Get the ServiceClientCredentials for the Dynamics client using the BASIC authentication method.
        /// </summary>
        /// <param name="Configuration">The application configuration (IConfiguration).</param>
        /// <returns>ServiceClientCredentials or null if credentials could not be resolved.</returns>
        public static ServiceClientCredentials GetBasicTokenCredentials(
            IConfiguration Configuration
        )
        {
            // API Gateway to NTLM user.  This is used in v8 environments.  Note that the SSG Username and password are not the same as the NTLM user.
            string ssgUsername = Configuration["SSG_USERNAME"]; // BASIC authentication username
            string ssgPassword = Configuration["SSG_PASSWORD"]; // BASIC authentication password

            if (string.IsNullOrEmpty(ssgUsername) || string.IsNullOrEmpty(ssgPassword))
            {
                // Missing required configuration settings
                return null;
            }

            return new BasicAuthenticationCredentials()
            {
                UserName = ssgUsername,
                Password = ssgPassword
            };
        }

        /// <summary>
        /// Get the ServiceClientCredentials for the Dynamics client.
        /// </summary>
        /// <param name="Configuration">The application configuration (IConfiguration).</param>
        /// <returns>ServiceClientCredentials</returns>
        public static ServiceClientCredentials GetServiceClientCredentials(
            IConfiguration Configuration
        )
        {
            // Get the client credentials based on the available configuration settings.
            ServiceClientCredentials serviceClientCredentials =
                GetDataverseTokenCredentials(Configuration)
                ?? GetOnPremTokenCredentials(Configuration)
                ?? GetBasicTokenCredentials(Configuration);

            if (serviceClientCredentials == null)
            {
                throw new Exception(
                    "Failed to get ServiceClientCredentials. Check your configuration settings."
                );
            }

            return serviceClientCredentials;
        }

        // public static ServiceClientCredentials GetServiceClientCredentials(
        //     IConfiguration configuration
        // )
        // {
        //     // Cloud - x.dynamics.com
        //     string aadTenantId = configuration["DYNAMICS_AAD_TENANT_ID"]; // Cloud AAD Tenant ID
        //     string serverAppIdUri = configuration["DYNAMICS_SERVER_APP_ID_URI"]; // Cloud Server App ID URI
        //     string appRegistrationClientKey = configuration["DYNAMICS_APP_REG_CLIENT_KEY"]; // Cloud App Registration Client Key
        //     string appRegistrationClientId = configuration["DYNAMICS_APP_REG_CLIENT_ID"]; // Cloud App Registration Client Id

        //     // One Premise ADFS (2016)
        //     string adfsOauth2Uri = configuration["ADFS_OAUTH2_URI"]; // ADFS OAUTH2 URI - usually /adfs/oauth2/token on STS
        //     string applicationGroupResource = configuration["DYNAMICS_APP_GROUP_RESOURCE"]; // ADFS 2016 Application Group resource (URI)
        //     string applicationGroupClientId = configuration["DYNAMICS_APP_GROUP_CLIENT_ID"]; // ADFS 2016 Application Group Client ID
        //     string applicationGroupSecret = configuration["DYNAMICS_APP_GROUP_SECRET"]; // ADFS 2016 Application Group Secret
        //     string serviceAccountUsername = configuration["DYNAMICS_USERNAME"]; // Service account username
        //     string serviceAccountPassword = configuration["DYNAMICS_PASSWORD"]; // Service account password

        //     // API Gateway to NTLM user.  This is used in v8 environments.  Note that the SSG Username and password are not the same as the NTLM user.
        //     string ssgUsername = configuration["SSG_USERNAME"]; // BASIC authentication username
        //     string ssgPassword = configuration["SSG_PASSWORD"]; // BASIC authentication password

        //     ServiceClientCredentials serviceClientCredentials = null;
        //     if (
        //         !string.IsNullOrEmpty(appRegistrationClientId)
        //         && !string.IsNullOrEmpty(appRegistrationClientKey)
        //         && !string.IsNullOrEmpty(serverAppIdUri)
        //         && !string.IsNullOrEmpty(aadTenantId)
        //     )
        //     // Cloud authentication - using an App Registration's client ID, client key.  Add the App Registration to Dynamics as an Application User.
        //     {
        //         var authenticationContext = new AuthenticationContext(
        //             "https://login.windows.net/" + aadTenantId
        //         );
        //         ClientCredential clientCredential = new ClientCredential(
        //             appRegistrationClientId,
        //             appRegistrationClientKey
        //         );
        //         var task = authenticationContext.AcquireTokenAsync(
        //             serverAppIdUri,
        //             clientCredential
        //         );
        //         task.Wait();
        //         var authenticationResult = task.Result;
        //         string token = authenticationResult
        //             .CreateAuthorizationHeader()
        //             .Substring("Bearer ".Length);
        //         serviceClientCredentials = new TokenCredentials(token);
        //     }
        //     if (
        //         !string.IsNullOrEmpty(adfsOauth2Uri)
        //         && !string.IsNullOrEmpty(applicationGroupResource)
        //         && !string.IsNullOrEmpty(applicationGroupClientId)
        //         && !string.IsNullOrEmpty(applicationGroupSecret)
        //         && !string.IsNullOrEmpty(serviceAccountUsername)
        //         && !string.IsNullOrEmpty(serviceAccountPassword)
        //     )
        //     // ADFS 2016 authentication - using an Application Group Client ID and Secret, plus service account credentials.
        //     {
        //         // create a new HTTP client that is just used to get a token.
        //         var stsClient = new HttpClient();

        //         //stsClient.DefaultRequestHeaders.Add("x-client-SKU", "PCL.CoreCLR");
        //         //stsClient.DefaultRequestHeaders.Add("x-client-Ver", "5.1.0.0");
        //         //stsClient.DefaultRequestHeaders.Add("x-ms-PKeyAuth", "1.0");

        //         stsClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
        //         stsClient.DefaultRequestHeaders.Add("return-client-request-id", "true");
        //         stsClient.DefaultRequestHeaders.Add("Accept", "application/json");

        //         // Construct the body of the request
        //         var pairs = new List<KeyValuePair<string, string>>
        //         {
        //             new KeyValuePair<string, string>("resource", applicationGroupResource),
        //             new KeyValuePair<string, string>("client_id", applicationGroupClientId),
        //             new KeyValuePair<string, string>("client_secret", applicationGroupSecret),
        //             new KeyValuePair<string, string>("username", serviceAccountUsername),
        //             new KeyValuePair<string, string>("password", serviceAccountPassword),
        //             new KeyValuePair<string, string>("scope", "openid"),
        //             new KeyValuePair<string, string>("response_mode", "form_post"),
        //             new KeyValuePair<string, string>("grant_type", "password")
        //         };

        //         // This will also set the content type of the request
        //         var content = new FormUrlEncodedContent(pairs);
        //         // send the request to the ADFS server
        //         var _httpResponse = stsClient
        //             .PostAsync(adfsOauth2Uri, content)
        //             .GetAwaiter()
        //             .GetResult();
        //         var _responseContent = _httpResponse
        //             .Content.ReadAsStringAsync()
        //             .GetAwaiter()
        //             .GetResult();
        //         // response should be in JSON format.
        //         try
        //         {
        //             Dictionary<string, string> result = JsonConvert.DeserializeObject<
        //                 Dictionary<string, string>
        //             >(_responseContent);
        //             string token = result["access_token"];
        //             // set the bearer token.
        //             serviceClientCredentials = new TokenCredentials(token);
        //         }
        //         catch (Exception e)
        //         {
        //             throw new Exception(e.Message + " " + _responseContent);
        //         }
        //     }
        //     else if (!string.IsNullOrEmpty(ssgUsername) && !string.IsNullOrEmpty(ssgPassword))
        //     // Authenticate using BASIC authentication - used for API Gateways with BASIC authentication.  Add the NTLM user associated with the API gateway entry to Dynamics as a user.
        //     {
        //         serviceClientCredentials = new BasicAuthenticationCredentials
        //         {
        //             UserName = ssgUsername,
        //             Password = ssgPassword
        //         };
        //     }
        //     else
        //     {
        //         throw new Exception("No configured connection to Dynamics.");
        //     }

        //     return serviceClientCredentials;
        // }

        /// <summary>
        /// Setup a Dynamics client.
        /// </summary>
        /// <param name="Configuration"></param>
        /// <returns></returns>
        public static IDynamicsClient SetupDynamics(IConfiguration Configuration)
        {
            string dynamicsOdataUri = Configuration["DYNAMICS_ODATA_URI"]; // Dynamics ODATA endpoint

            if (string.IsNullOrEmpty(dynamicsOdataUri))
            {
                throw new Exception("configuration setting DYNAMICS_ODATA_URI is blank.");
            }

            ServiceClientCredentials serviceClientCredentials = GetServiceClientCredentials(
                Configuration
            );

            IDynamicsClient client = new DynamicsClient(
                new Uri(dynamicsOdataUri),
                serviceClientCredentials
            );

            // set the native client URI.  This is required if you have a reverse proxy or IFD in place and the native URI is different from your access URI.
            if (string.IsNullOrEmpty(Configuration["DYNAMICS_NATIVE_ODATA_URI"]))
            {
                client.NativeBaseUri = new Uri(Configuration["DYNAMICS_ODATA_URI"]);
            }
            else
            {
                client.NativeBaseUri = new Uri(Configuration["DYNAMICS_NATIVE_ODATA_URI"]);
            }

            return client;
        }

        public static IDynamicsClient SetupDynamics(
            HttpClient httpClient,
            IConfiguration Configuration
        )
        {
            IDynamicsClient client = new DynamicsClient(httpClient, Configuration);

            return client;
        }
    }
}
