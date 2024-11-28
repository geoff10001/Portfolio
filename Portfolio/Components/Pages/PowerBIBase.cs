using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Portfolio.Models;
using Portfolio.Repository;
using Portfolio.Services;
using System.Security.Claims;
using System.Timers;

namespace Portfolio.Components.Pages
{
    public class PowerBIBase : ComponentBase
    {
        // A dictionary to store report data (ID, page name, and page role)
        private static readonly Dictionary<string, (string PageName, string PageRole)> ReportData = new()
        {
            // Example: { "ReportID", ("PageName", "PageRole") }
            // Projects menu
            { "c5459af1-dd86-426b-8894-bce17758c7f2", ("Projects", "Labour_Projects") },
            { "db27230f-4ea2-432d-b333-2bc5ea63c4d7", ("ProjectsPL", "PL_Projects") },
            { "5eabe9f7-06cf-46bf-a3f7-facdd3daad66", ("JobProfitSummary", "PL_Production_Jobs") },
            // Energy menu
            { "a5360d6b-04bd-4908-aa80-961756477923", ("EnergyAssetUtilisation", "Energy_Asset_Utilisation") },
            { "9d868f1f-bf88-4ef0-adc7-d424e8036b3f", ("EnergyAssetUtilisationNew", "Energy_Asset_Utilisation") },
            { "95120663-becf-441c-a2e7-d48ded29d7de", ("JobProfitSummary", "PL_Energy_Jobs") },
            { "b175c631-6334-4154-bd0f-9d898c1c9733", ("EnergyPL", "PL_Energy") },
            { "4b7d1da4-1376-4c74-8505-4dd8d94f8b5c", ("EnergyPLGoldfields", "PL_Energy_Kal") },
            { "a0c166e1-4f63-4a37-92da-83426f2dfc53", ("EnergyIndirectLabour", "Labour_Energy") },
            // Production menu
            { "27a3b0ca-77d1-4f2a-aa6a-7714e95f3317", ("Production", "Labour_Production") },
            { "081321f5-9401-40b7-8b74-bdd6b8d15ecb", ("ProductionPL", "PL_Production") },
            { "7c94ba1c-e7bf-4ece-937d-4aca113a31c1", ("ProductionStats", "Production_Statistics") },
            { "48c12070-7604-4ece-8603-6e1b80929eb9", ("JN12091Summary", "Production_Statistics") },
            { "01921f73-09a8-467f-9179-3a48c3af31ba", ("JobProfitSummary", "PL_Production_Jobs") },
            // Executive menu
            { "0330bc55-3671-4a8e-b3a7-1f1b25d1ba48", ("UONPL", "PL_UON") },
            { "ae3f098c-34a0-4466-9eac-3e44e72dd273", ("JobProfitSummary", "PL_Jobs") },
            // HR menu
            { "3b7f8716-ecf0-4769-a945-f97a2701fa26", ("HR", "HR_HeadCount") },
            { "e8433a48-1e7e-4182-aaed-70c1f5847a22", ("Safety", "HSE_Safety") },
            { "f323804c-45f7-4419-b9d7-50a24306f20b", ("RD", "RD_Costs") },
            { "6de02bf8-12e5-432d-9638-04b5a9ca1a41", ("RDPL", "PL_RD") },
            { "11f5dc0f-775c-4603-8ce3-546db2cfdb00", ("ISC", "ISC_Costs") },
            { "f67310ef-bc3f-419f-8747-076affd24489", ("SalesPipeline", "BD_Sales") },
            { "9d948af2-a3f1-45a7-b11e-0a01793bd9cc", ("BidManagement", "BD_Sales") }
            // Add other reports as required...
        };


        // Constants for report and role default values
        private const string DefaultPageName = "default_pagename";
        private const string DefaultPageRole = "default_pagerole";

        // Declare pageName and pageRole
        private string reportId;
        protected static string pageName = DefaultPageName;
        protected static string pageRole = DefaultPageRole;
        protected string pageRoles = $"[Page: {pageName} Permission: {pageRole}]";
        protected DateTime? lastRefreshDate;
        private System.Timers.Timer? refreshTimer;

        protected ReportEmbedConfig myEmbedReport { get; private set; }
        protected ElementReference PowerBIElement;

        protected bool IsProcessing { get; private set; }
        protected bool IsAllowed { get; private set; }
        protected bool HasRole { get; private set; }
        private bool isReportNotFound = false;

        protected bool statusmessage = false;

        private const string workspaceId = "7a286bcf-53b0-45c9-ab0d-c2987910278d";

        protected bool isClicked = false;
        protected string ButtonAzureText => isClicked ? "Clicked..." : "Start PowerBI Server";

        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ILogger<PowerBI> Logger { get; set; }

        protected IEnumerable<Claim> claims = Enumerable.Empty<Claim>();

        protected ClaimsPrincipal user;
        [TempData] protected string StatusMessage { get; set; }

        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

        [Inject] protected IPowerBIDataRepository PowerBIDataRepository { get; set; }
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            if (IsProcessing) return;

            IsProcessing = true;

            try
            {
                IsAllowed = false;
                HasRole = false;

                //await InitializeUserSettingsAsync();

                await CheckPermissionsAsync();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            // Set parameters in the component from the supplied ParameterView instance
            parameters.SetParameterProperties(this);

            if (!_initialized)
            {
                await InitializeUserSettingsAsync();
            }

            if (pageName == "BidManagement")
            {
                SetInitialTimer();
            }

            await base.SetParametersAsync(ParameterView.Empty);
        }

        private async Task InitializeUserSettingsAsync()
        {
            // Simulate authenticated user
            user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, "testuser@example.com"),
        new Claim(ClaimTypes.Role, "Superuser"), // Adjust role as needed for your test
    }, "TestAuthentication"));

            IsAllowed = true; // Allow access for testing
            HasRole = true;   // Assume user has the required role

            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

            // Extract report ID from query parameters
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("reportid", out var reportID))
            {
                reportId = reportID;
            }

            // Get page role and name for the report ID
            GetPageRoleForReportID(reportId, out pageRole, out pageName);

            _initialized = true;

            // Optional logging or mock repository call
            await PowerBIDataRepository.CreateUserURLAudit(
                "testuser@example.com",
                $"{uri}&PageName={pageName}&PageRole={pageRole}");

            _initialized = true;
        }


        //private async Task InitializeUserSettingsAsync()
        //{
        //    var authState = await authenticationState;
        //    var user = authState?.User;
        //    var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

        //    // Redirect to login if user is not authenticated
        //    if (user?.Identity?.IsAuthenticated != true)
        //    {
        //        NavigationManager.NavigateTo("/Account/LogIn");
        //        return;
        //    }

        //    // Extract report ID from query parameters
        //    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("reportid", out var reportID))
        //    {
        //        reportId = reportID;
        //    }

        //    // Get page role and name for the report ID
        //    GetPageRoleForReportID(reportId, out pageRole, out pageName);

        //    // Log the URL for auditing purposes
        //    await PowerBIDataRepository.CreateUserURLAudit(user.Identity.Name.ToLower(),
        //        $"{uri}&PageName={pageName}&PageRole={pageRole}");

        //    _initialized = true;
        //}

        private async Task CheckPermissionsAsync()
        {
            // Simulate permissions for testing
            IsAllowed = true;
            HasRole = true;

            Logger.LogInformation("Bypassing permissions for testing.");
        }


        //private async Task CheckPermissionsAsync()
        //{
        //    var authState = await authenticationState;
        //    var user = authState?.User;

        //    if (user?.Identity?.IsAuthenticated == true)
        //    {
        //        var userHasRequiredPermissions = HasRequiredPermissions(user);
        //        if (!userHasRequiredPermissions)
        //        {
        //            Logger.LogInformation("User does not have the required permissions to access this page.");
        //            SetPermissionErrorMessage();
        //        }
        //        else
        //        {
        //            HasRole = true;
        //        }
        //    }

        //    var claimTwoFactorEnabled = user?.Claims?.FirstOrDefault(t => t.Type == "TwoFactorEnabled")?.Value;

        //    if (claimTwoFactorEnabled == "true")
        //    {
        //        IsAllowed = true;
        //    }
        //    else
        //    {
        //        NavigationManager.NavigateTo("/Account/Manage/TwoFactorAuthentication");
        //    }
        //}

        private bool HasRequiredPermissions(ClaimsPrincipal user)
        {
            return user.IsInRole("Superuser") || user.IsInRole(pageRole);
        }

        private void SetPermissionErrorMessage()
        {
            if (ReportData.TryGetValue(reportId, out var reportInfo))
            {
                pageRole = reportInfo.PageRole;
                pageName = reportInfo.PageName;
            }

            NavigationManager.NavigateTo($"Error?messageid=missingrole&pagerole={pageRole}&pagename={pageName}");
        }

        private void GetPageRoleForReportID(string reportid, out string pagerole, out string pagename)
        {
            if (!ReportData.TryGetValue(reportid, out var pageData))
            {
                pagerole = DefaultPageRole;
                pagename = DefaultPageName;
                return;
            }

            pagerole = pageData.PageRole;
            pagename = pageData.PageName;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if (!IsAllowed || !HasRole) return;
            IsProcessing = true;

            var workspaceGuid = GetParamGuid(workspaceId);
            var reportGuid = GetParamGuid(reportId);

            if (workspaceGuid == null || reportGuid == null)
            {
                isReportNotFound = true;
                LogWarning("Invalid WorkspaceId or ReportId provided.");
            }
            else
            {
                try
                {
                    var datasetId = await EmbedService.GetDatasetIdFromReportAsync(workspaceGuid, reportGuid);
                    if (string.IsNullOrWhiteSpace(datasetId))
                    {
                        LogWarning("Dataset ID is null or empty.");
                        isReportNotFound = true;
                    }
                    else
                    {
                        lastRefreshDate = await EmbedService.GetLastRefreshDateAsync(workspaceGuid, Guid.Parse(datasetId));
                    }
                }
                catch (Microsoft.Rest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    LogError($"Report or Workspace not found: WorkspaceId = {workspaceGuid}, ReportId = {reportGuid}", ex);
                    isReportNotFound = true;
                }
                catch (Exception ex)
                {
                    LogError("Unexpected error while fetching dataset ID or refresh date.", ex);
                    isReportNotFound = true;
                }
            }

            if (pageName == "BidManagement")
            {
                await RequestWakeLock();
            }
            else
            {
                // If it's not the BidManagement page, dispose of the timer to prevent memory leaks
                DisposeTimer();
            }

            await EmbedReport(isReportNotFound);

            IsProcessing = false;
            StateHasChanged();
        }

        private async Task EmbedReport(bool isPublicReport)
        {
            if (isPublicReport)
            {
                // Handle public report embedding
                string publicEmbedUrl = "https://app.powerbi.com/view?r=eyJrIjoiZWE4ZjU1MjAtZDA3MS00MzA3LTg3NWQtOTBmYTA0YTQzYTI2IiwidCI6IjNmYjhiOWZkLTM1ZTgtNGRkYi1iMjRmLTgzMTRiNjg5ZTgzMiJ9";

                await Interop.CreateReport(
                    JSRuntime,
                    PowerBIElement,
                    null, // No access token needed
                    publicEmbedUrl,
                    null  // No report ID needed
                );
            }
            else
            {
                // Handle private report embedding
                var embedParams = await EmbedService.GetEmbedParams(GetParamGuid(workspaceId), GetParamGuid(reportId));

                if (embedParams?.EmbedReports?.Count > 0)
                {
                    var embedReport = embedParams.EmbedReports[0];
                    await Interop.CreateReport(
                        JSRuntime,
                        PowerBIElement,
                        embedParams.EmbedToken.Token,
                        embedReport.EmbedUrl,
                        embedReport.ReportId.ToString()
                    );
                }
                else
                {
                    LogWarning("No reports found in embed parameters.");
                }
            }
        }

        private void LogWarning(string message)
        {
            Console.WriteLine($"[Warning]: {message}");
        }

        private void LogError(string message, Exception ex)
        {
            Console.WriteLine($"[Error]: {message}\nException: {ex}");
        }

        private static Guid GetParamGuid(string param)
        {
            return Guid.TryParse(param, out Guid paramGuid) ? paramGuid : Guid.Empty;
        }

        private void SetInitialTimer()
        {

            DateTime now = DateTime.Now;

            // Calculate interval until the next 30-minute slot
            double initialInterval = (GetNextTimeSlot(now) - now).TotalMilliseconds;

            // Ensure a minimum interval of 1 second
            initialInterval = Math.Max(initialInterval, 1000);

            // Set the timer for the first refresh
            SetupTimer(initialInterval, FirstRefresh, false);
        }

        private DateTime GetNextTimeSlot(DateTime currentTime)
        {
            // Round down to the nearest half-hour mark
            int baseMinutes = (currentTime.Minute >= 30) ? 30 : 0;
            DateTime baseTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, baseMinutes, 0);

            // Calculate the next time slot (hh:05 or hh:35)
            DateTime nextTimeSlot = baseTime.AddMinutes(35); // Add 35 minutes for the next slot

            // If the next slot is in the next hour, adjust accordingly
            if (currentTime >= nextTimeSlot)
            {
                nextTimeSlot = baseTime.AddHours(1).AddMinutes(5); // Move to hh:05 of the next hour
            }

            return nextTimeSlot;
        }

        private void SetupTimer(double interval, ElapsedEventHandler handler, bool autoReset)
        {
            // Dispose of the old timer, if any
            DisposeTimer();

            // Initialise and configure the new timer
            refreshTimer = new System.Timers.Timer(interval)
            {
                AutoReset = autoReset,
                Enabled = true
            };

            refreshTimer.Elapsed += handler;
        }

        private void RefreshPage()
        {
            try
            {
                InvokeAsync(() =>
                {
                    NavigationManager?.NavigateTo(NavigationManager.Uri, forceLoad: true);
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during page refresh: {ex.Message}");
            }
        }

        private void FirstRefresh(object? sender, ElapsedEventArgs e)
        {
            RefreshPage();
            // Recurring 30-minute refresh schedule
            SetupTimer(30 * 60 * 1000, SubsequentRefresh, true);
        }

        private void SubsequentRefresh(object? sender, ElapsedEventArgs e)
        {
            RefreshPage();
        }

        private async Task RequestWakeLock()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("requestWakeLock");
            }
            catch (JSException ex)
            {
                // Graceful error handling if the wake lock fails
                Console.WriteLine($"Error while requesting wake lock: {ex.Message}");
            }
        }

        public async Task ReleaseWakeLock()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("releaseWakeLock");
            }
            catch (JSException ex)
            {
                // Graceful error handling if the release wake lock fails
                Console.WriteLine($"Error while releasing wake lock: {ex.Message}");
            }
        }
        public void DisposeTimer()
        {
            // Clean up the timer when the component is disposed
            refreshTimer?.Dispose();
            refreshTimer = null;
        }

        protected async Task SubmitAzureAsync()
        {
            try
            {
                // Start the Power BI Azure service asynchronously
                string embedResult = await EmbedService.StartPowerBIAzureService("resume");

                // After successful submission, update state accordingly
                isClicked = false;

                // Optionally log the success of the operation
                Logger.LogInformation("Power BI Azure service request successful. Result: " + embedResult);
            }
            catch (Exception ex)
            {
                // Handle exceptions, log the error, and optionally notify the user
                Logger.LogError(ex, "Error occurred during PowerBI Azure service request.");

                // Optionally, keep NotSent true if the request failed, depending on your logic
                isClicked = true;
            }
        }


    }
}