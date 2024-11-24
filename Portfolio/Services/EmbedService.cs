namespace Portfolio.Services
{
    using Azure.Core;
    using Microsoft.PowerBI.Api;
    using Microsoft.PowerBI.Api.Models;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using Portfolio.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class EmbedService
    {
        

        // Use a dictionary to store user credentials
        private static readonly Dictionary<string, TokenCredentials> userCredentialsCache = new Dictionary<string, TokenCredentials>();
        private static readonly Dictionary<string, DateTime> tokenExpirationTimes = new Dictionary<string, DateTime>();
        private static readonly TimeSpan defaultTtl = TimeSpan.FromMinutes(30); // Adjust the TTL as needed - lives for 60 but setting it for 30

        private static readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com";
        public static async Task<string> StartPowerBIAzureService(string action)
        {
            using (var client = new HttpClient())
            {
                string accessToken = await AadService.GetAzureAccessToken();
                var apiUrl = "https://management.azure.com/subscriptions/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/resourceGroups/PowerBI/providers/Microsoft.PowerBIDedicated/capacities/myportfolioembeddedcapacity/" + action + "?api-version=2021-01-01";


                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.PostAsync(apiUrl, null);

                // Check the response status and handle accordingly
                if (response.IsSuccessStatusCode)
                {
                    var stringData = "Power BI capacity resumed successfully.";
                    return JsonConvert.DeserializeObject<string>(stringData);
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    var stringData = $"Failed to resume Power BI capacity. Status code: {response.StatusCode}";
                    stringData = stringData + $" Error message: {errorMessage}";
                    return JsonConvert.DeserializeObject<string>(stringData);
                }
            }
        }

        public static async Task<PowerBIClient> GetPowerBiClient()
        {
            // Check if the cached credentials are available
            if (userCredentialsCache.TryGetValue("default", out TokenCredentials cachedCredentials))
            {
                // Check if the token is still valid
                if (tokenExpirationTimes.TryGetValue("default", out var expirationTime) && expirationTime > DateTime.UtcNow)
                {
                    return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), cachedCredentials);
                }
                else
                {
                    // Token has expired, remove it from the cache
                    userCredentialsCache.Remove("default");
                    tokenExpirationTimes.Remove("default");
                }
            }

            // Create new credentials since either they weren't cached or have expired
            TokenCredentials newCredentials = new TokenCredentials(await AadService.GetAccessToken(), "Bearer");

            // Ensure no duplicate key before adding to the cache
            if (!userCredentialsCache.ContainsKey("default"))
            {
                userCredentialsCache.Add("default", newCredentials);
            }
            if (!tokenExpirationTimes.ContainsKey("default"))
            {
                tokenExpirationTimes.Add("default", DateTime.UtcNow + defaultTtl);
            }

            return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), newCredentials);
        }


        public static async Task<ReportEmbedConfig> GetEmbedParams(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId)
        {
            using (var pbiClient = await GetPowerBiClient())
            {

                //got a forbidden error when trying to get the report - so has the token a limited life and timing out 
                //can i trap this error and get a new token and try again?


                // Get report info
                var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

                /*
                Check if dataset is present for the corresponding report
                If no dataset is present then it is a RDL report 
                */
                bool isRDLReport = String.IsNullOrEmpty(pbiReport.DatasetId);

                EmbedToken embedToken;

                if (isRDLReport)
                {
                    // Get Embed token for RDL Report
                    embedToken = await GetEmbedTokenForRDLReport(workspaceId, reportId);
                }
                else
                {
                    // Create list of dataset
                    var datasetIds = new List<Guid>();

                    // Add dataset associated to the report
                    datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

                    // Append additional dataset to the list to achieve dynamic binding later
                    if (additionalDatasetId != Guid.Empty)
                    {
                        datasetIds.Add(additionalDatasetId);
                    }

                    pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

                    // Get Embed token multiple resources
                    embedToken = await GetEmbedToken(reportId, datasetIds, workspaceId);
                }

                // Add report data for embedding
                var embedReports = new List<EmbedReport>() {
                    new EmbedReport
                    {
                        ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl
                    }
                };

                // Capture embed params
                var embedParams = new ReportEmbedConfig
                {
                    EmbedReports = embedReports,
                    EmbedToken = embedToken
                };

                return embedParams;
            }
        }

        /// <summary>
        /// Get embed params for multiple reports for a single workspace
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for multiple reports</returns>
        /// <remarks>This function is not supported for RDL Report</remakrs>
        public static async Task<ReportEmbedConfig> GetEmbedParams(Guid workspaceId, IList<Guid> reportIds, [Optional] IList<Guid> additionalDatasetIds)
        {

            // Note: This method is an example and is not consumed in this sample app

            using (var pbiClient = await GetPowerBiClient())
            {
                // Create mapping for reports and Embed URLs
                var embedReports = new List<EmbedReport>();

                // Create list of datasets
                var datasetIds = new List<Guid>();

                // Get datasets and Embed URLs for all the reports
                foreach (var reportId in reportIds)
                {
                    // Get report info
                    var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

                    // Append to existing list of datasets to achieve dynamic binding later
                    datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

                    // Add report data for embedding
                    embedReports.Add(new EmbedReport { ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl });
                }

                // Append to existing list of datasets to achieve dynamic binding later
                if (additionalDatasetIds != null)
                {
                    datasetIds.AddRange(additionalDatasetIds);
                }

                // Get Embed token multiple resources
                var embedToken = await GetEmbedToken(reportIds, datasetIds, workspaceId);

                // Capture embed params
                var embedParams = new ReportEmbedConfig
                {
                    EmbedReports = embedReports,
                    EmbedToken = embedToken
                };

                return embedParams;
            }
        }

        /// <summary>
        /// Get Embed token for single report, multiple datasets, and an optional target workspace
        /// </summary>
        /// <returns>Embed token</returns>
        /// <remarks>This function is not supported for RDL Report</remakrs>
        public static async Task<EmbedToken> GetEmbedToken(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
        {
            using (var pbiClient = await GetPowerBiClient())
            {
                
                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },

                datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

                targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }
        }

        /// <summary>
        /// Get Embed token for multiple reports, datasets, and an optional target workspace
        /// </summary>
        /// <returns>Embed token</returns>
        /// <remarks>This function is not supported for RDL Report</remakrs>
        public static async Task<EmbedToken> GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
        {
            // Note: This method is an example and is not consumed in this sample app

            using (var pbiClient = await GetPowerBiClient())
            {
                // Convert reports to required types
                var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

                // Convert datasets to required types
                var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                    datasets: datasets,

                    reports: reports,

                    targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }
        }

        /// <summary>
        /// Get Embed token for multiple reports, datasets, and optional target workspaces
        /// </summary>
        /// <returns>Embed token</returns>
        /// <remarks>This function is not supported for RDL Report</remakrs>
        public static async Task<EmbedToken> GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] IList<Guid> targetWorkspaceIds)
        {
            // Note: This method is an example and is not consumed in this sample app

            using (var pbiClient = await GetPowerBiClient())
            {
                // Convert report Ids to required types
                var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

                // Convert dataset Ids to required types
                var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

                // Convert target workspace Ids to required types
                IList<GenerateTokenRequestV2TargetWorkspace> targetWorkspaces = null;
                if (targetWorkspaceIds != null)
                {
                    targetWorkspaces = targetWorkspaceIds.Select(targetWorkspaceId => new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId)).ToList();
                }

                // Create a request for getting Embed token 
                // This method works only with new Power BI V2 workspace experience
                var tokenRequest = new GenerateTokenRequestV2(

                    datasets: datasets,

                    reports: reports,

                    targetWorkspaces: targetWorkspaceIds != null ? targetWorkspaces : null
                );

                // Generate Embed token
                var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

                return embedToken;
            }
        }

        /// <summary>
        /// Get Embed token for RDL Report
        /// </summary>
        /// <returns>Embed token</returns>
        public static async Task<EmbedToken> GetEmbedTokenForRDLReport(Guid targetWorkspaceId, Guid reportId, string accessLevel = "view")
        {
            using (var pbiClient = await GetPowerBiClient())
            {

                // Generate token request for RDL Report
                var generateTokenRequestParameters = new GenerateTokenRequest(
                    accessLevel: accessLevel
                );

                // Generate Embed token
                var embedToken = pbiClient.Reports.GenerateTokenInGroup(targetWorkspaceId, reportId, generateTokenRequestParameters);

                return embedToken;
            }
        }

        /// <summary>
        /// Get embed params for a dashboard
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL for single dashboard</returns>
        public static async Task<DashboardEmbedConfig> EmbedDashboard(Guid workspaceId)
        {
            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = await GetPowerBiClient())
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(workspaceId);

                // Get the first report in the workspace.
                var dashboard = dashboards.Value.FirstOrDefault();

                if (dashboard == null)
                {
                    throw new NullReferenceException("Workspace has no dashboards");
                }

                // Generate Embed Token.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Dashboards.GenerateTokenInGroupAsync(workspaceId, dashboard.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    throw new NullReferenceException("Failed to generate embed token");
                }

                // Generate Embed Configuration.
                var dashboardEmbedConfig = new DashboardEmbedConfig
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = dashboard.EmbedUrl,
                    DashboardId = dashboard.Id
                };

                return dashboardEmbedConfig;
            }
        }

        /// <summary>
        /// Get embed params for a tile
        /// </summary>
        /// <returns>Wrapper object containing Embed token, Embed URL for single tile</returns>
        public static async Task<TileEmbedConfig> EmbedTile(Guid workspaceId)
        {
            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = await GetPowerBiClient())
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(workspaceId);

                // Get the first report in the workspace.
                var dashboard = dashboards.Value.FirstOrDefault();

                if (dashboard == null)
                {
                    throw new NullReferenceException("Workspace has no dashboards");
                }

                var tiles = await client.Dashboards.GetTilesInGroupAsync(workspaceId, dashboard.Id);

                // Get the first tile in the workspace.
                var tile = tiles.Value.FirstOrDefault();

                // Generate Embed Token for a tile.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Tiles.GenerateTokenInGroupAsync(workspaceId, dashboard.Id, tile.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    throw new NullReferenceException("Failed to generate embed token");
                }

                // Generate Embed Configuration.
                var tileEmbedConfig = new TileEmbedConfig()
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = tile.EmbedUrl,
                    TileId = tile.Id,
                    DashboardId = dashboard.Id
                };

                return tileEmbedConfig;
            }
        }


        public static async Task<DateTime?> GetLastRefreshDateAsync(Guid workspaceId, Guid datasetId)
        {
            var pbiClient = await GetPowerBiClient();

            //var myReport = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
            //var lastRefreshDate2= await pbiClient.Datasets.GetRefreshHistoryInGroupAsync(workspaceId, datasetId.ToString());

            var lastRefreshDate = await pbiClient.Datasets.GetRefreshHistoryAsync(workspaceId, datasetId.ToString(),1);

            var utcDateTime = lastRefreshDate.Value.FirstOrDefault()?.EndTime;

            // Check if the retrieved UTC date is not null
            if (utcDateTime.HasValue)
            {
                //// Convert to local time
                //var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, TimeZoneInfo.Local);
                //return localDateTime;

                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Perth");

                // Convert to specified time zone
                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, timeZone);
                return localDateTime;
            }

            return null; // Return null if there's no last refresh date
        }

        public static async Task<string> GetDatasetIdFromReportAsync(Guid workspaceId, Guid reportId)
        {
            using (var pbiClient = await GetPowerBiClient())
            {
                // Retrieve the report from the specified workspace
                var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);

                // Return the DatasetId from the report
                return report.DatasetId;
            }
        }

    }
}
