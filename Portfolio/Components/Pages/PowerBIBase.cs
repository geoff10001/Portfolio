using Azure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json.Linq;
using Portfolio.Components.Account;
using Portfolio.Models;
using Portfolio.Repository;
using Portfolio.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Timers;
//using System.Timers;

namespace Portfolio.Components.Pages
{
    public class PowerBIBase : ComponentBase
    {
        protected DateTime? lastRefreshDate;
        private System.Timers.Timer? refreshTimer;

        //change below three field for each new report
        protected static string reportId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        protected static string pageName = "MyPowerBISummary";
        protected static string pageRole = "RoleName_For_This_Page_Access";

        static string myreport1_reportid = "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy";
        static string myreport1_pagename = "MyReport1";
        static string myreport1_pagerole = "RoleName_MyReport1";

        static string myreport2_reportid = "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy";
        static string myreport2_pagename = "MyReport1";
        static string myreport2_pagerole = "RoleName_MyReport1";


        static string myreport3_reportid = "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy";
        static string myreport3_pagename = "MyReport1";
        static string myreport3_pagerole = "RoleName_MyReport1";

        //...etc


        protected string pageRoles = $"[Page: {pageName} Permission: {pageRole}]";

        protected ReportEmbedConfig myEmbedReport { get; set; }
        protected ElementReference PowerBIElement;
        private const string workspaceId = "zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz";

        protected bool NotSent = true;

        //PowerBI Service is charged per hour so I typically shut it down after a number of minutes, if it has stopped this button click allows it to be strted by the suer
        protected string ButtonAzureText => NotSent ? "Start PowerBI Server" : "Started - (Click again to request a restart)";

        //[Inject] protected IEmailSender EmailSender { get; set; }
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        //[Inject] IdentityRedirectManager RedirectManager { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ILogger<PowerBI> Logger { get; set; }

        protected IEnumerable<Claim> claims = Enumerable.Empty<Claim>();

        protected ClaimsPrincipal user;

        protected bool allowed = false;
        protected bool hasRole= false;
        protected bool spinning = false;
        protected bool statusmessage = false;
        [TempData]
        protected string StatusMessage { get; set; }
        [TempData]
        protected string StatusMessageLn2 { get; set; }
        [TempData]
        protected string StatusMessageLn3 { get; set; }
        [TempData]
        protected string StatusMessageLn4 { get; set; }
        [TempData]
        protected string StatusMessageLn5 { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? authenticationState { get; set; }

        [Inject] NavigationManager navManager { get; set; }

        //string reportid;
        //string pagename;
        //string pagerole;
        [Inject] protected IPowerBIDataRepository PowerBIDataRepository { get; set; }
        private bool _initialized;

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            
            // Must do this immediately.  This sets the parameters in the component from the supplied ParameterView instance
            parameters.SetParameterProperties(this);
            if (!_initialized)
            {
                var authState = await authenticationState;
                var user = authState?.User;
                var uri = navManager.ToAbsoluteUri(navManager.Uri);

                if (user == null || !user.Identity.IsAuthenticated)
                {
                    //RedirectManager.RedirectTo("./Account/LogIn");
                    NavigationManager.NavigateTo($"/Account/LogIn");

                }

                if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("reportid", out var ReportID))
                {
                    reportId = ReportID;
                }
                GetPageRoleForReportID(reportId, out pageRole, out pageName);

                var results = await PowerBIDataRepository.CreateUserURLAudit(user.Identity.Name.ToLower(), uri.ToString() + "&PageName=" + pageName + "&PageRole=" + pageRole);

                _initialized = true;
            }

            //IF a KEEP ALIVE REPORT..TO STOP AUTO LOG OFF IN WINDOWS DESKTOP 
            if (pageName == "A_PagenName_To_Keep_Alive")
            {
                SetInitialTimer();
            }

            await base.SetParametersAsync(ParameterView.Empty);
        }
        protected override async Task OnInitializedAsync()
        {
            allowed = false;
            hasRole = false;
            spinning = true;
            statusmessage = false;
           await CheckPermissionsAsync();
        }

        protected void GetPageRoleForReportID(string reportid, out string pagerole, out string pagename)
        {
            /* menu 1 */
            if (reportid == myreport1_reportid)
            {
                pagerole = myreport1_pagerole;
                pagename = myreport1_pagename;
            }
            // menu 2
            else if (reportid == myreport1_reportid)
            {
                pagerole = myreport1_pagerole;
                pagename = myreport2_pagename;
            }
            else if (reportid == myreport3_reportid)
            {
                pagerole = myreport3_pagerole;
                pagename = myreport3_pagename;
            }
            
            else
            {
                // Default values if reportid doesn't match any of the predefined values
                pagerole = "default_pagerole";
                pagename = "default_pagename";
            }

            
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (allowed && hasRole)
            {
                if (firstRender)
                {
                    var datasetId = await EmbedService.GetDatasetIdFromReportAsync(GetParamGuid(workspaceId), GetParamGuid(reportId));
                    lastRefreshDate = await EmbedService.GetLastRefreshDateAsync(GetParamGuid(workspaceId), GetParamGuid(datasetId));

                    //IF BID MANAGER REPORT..
                    if (pageName == "A_PagenName_To_Keep_Alive")
                    {
                        // Call JavaScript to request the wake lock
                        await RequestWakeLock();
                    }

                    myEmbedReport = await EmbedService.GetEmbedParams(GetParamGuid(workspaceId), GetParamGuid(reportId));

                    await Interop.CreateReport(
                        JSRuntime,
                        PowerBIElement,
                        myEmbedReport.EmbedToken.Token,
                        myEmbedReport.EmbedReports[0].EmbedUrl,
                        myEmbedReport.EmbedReports[0].ReportId.ToString());
                }
                spinning = false;
                
                StateHasChanged();
                
            }
            spinning = false;
        }

        private static Guid GetParamGuid(string param)
        {
            if (Guid.TryParse(param, out Guid paramGuid))
            {
                return paramGuid;
            }

            // Handle parsing failure gracefully
            return Guid.Empty;
        }

        protected async Task SubmitAzureAsync()
        {
            try
            {
                string embedResult = await EmbedService.StartPowerBIAzureService("resume");

                //string SupportEmail = "Support@domain.com";
                //var authState = await authenticationState;
                //var user = authState?.User;

                //string emailbodytext = "";

                //string emailsubject = user.Identity.Name + $" has requested a PowerBI Server Start";
                //string emailbody = user.Identity.Name + $" has requested a PowerBI Server Start" + emailbodytext;
                //await EmailSender.SendEmailAsync(ITSupportEmail, emailsubject, emailbody);


                NotSent = false;
            }
            catch (Exception ex)
            {
                // Handle exceptions, log, or notify the user
                Logger.LogError(ex, "Error occurred during PowerBI Azure service request.");
            }
        }

        private async Task CheckPermissionsAsync()
        {
            var authState = await authenticationState;
            var user = authState?.User;

            if (user?.Identity is not null && user.Identity.IsAuthenticated)
            {
                var userHasRequiredPermissions = HasRequiredPermissions(user);
                if (!userHasRequiredPermissions)
                {
                    Logger.LogInformation("User does not have the required permissions to access this page.");
                    SetPermissionErrorMessage();
                }
                else if (userHasRequiredPermissions)
                {
                       hasRole = true;
                }
            }

            var claimTwoFactorEnabled = user.Claims.FirstOrDefault(t => t.Type == "TwoFactorEnabled");

            if (claimTwoFactorEnabled != null && "true".Equals(claimTwoFactorEnabled.Value))
            {
                allowed = true;
            }
            else
            {
                NavigationManager.NavigateTo($"/Account/Manage/TwoFactorAuthentication");
            }
        }

        private bool HasRequiredPermissions(ClaimsPrincipal user)
        {
            var userIsSuperUser = user.IsInRole("Superuser");
            var userIsPageRole = user.IsInRole(pageRole);
            return userIsSuperUser || userIsPageRole;
        }

        private void SetPermissionErrorMessage()
        {
            //StatusMessage = $"We apologize, but it seems that you do not currently have the necessary permissions to access this page.";
            //StatusMessageLn2 = $"To obtain access, kindly seek approval from your supervisor and then contact our IT support team.";
            //StatusMessageLn3 = $"Please provide them with the details of this page and the permission required, so that they can assist you with the necessary access.";
            //StatusMessageLn4 = $"" + pageRoles;
            //StatusMessageLn5 = $"Thank you for your cooperation.";

            //statusmessage = true;
            //spinning = false;
            //StateHasChanged();
            GetPageRoleForReportID(reportId, out pageRole, out pageName);
            NavigationManager.NavigateTo($"Error?messageid=missingrole&pagerole=" + pageRole + "&pagename=" + pageName);
        }

        private void SetInitialTimer()
        {
            DateTime now = DateTime.Now;

            // Find the next 30-minute slot
            DateTime firstTrigger = GetNextTimeSlot(now);
            double initialInterval = (firstTrigger - now).TotalMilliseconds;

            if (initialInterval <= 0)
            {
                // If the calculated time has passed (which shouldn't happen), skip to the next 30-minute slot
                //firstTrigger = GetNextTimeSlot(now);
                initialInterval = 1000;
            }

            // Set the timer for the first trigger
            refreshTimer = new System.Timers.Timer(initialInterval);
            refreshTimer.Elapsed += FirstRefresh;
            refreshTimer.AutoReset = false; // Trigger only once for the first refresh
            refreshTimer.Enabled = true;
        }

        private DateTime GetNextTimeSlot(DateTime currentTime)
        {
            int minutes = currentTime.Minute;

            // If we're past the half-hour mark (e.g., 6:45), go to the next full hour (7:05)
            if (minutes >= 35)
            {
                // Check if the current hour is 23; if so, move to the next day
                if (currentTime.Hour == 23)
                {
                    return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day).AddDays(1).AddHours(0).AddMinutes(5);
                }
                else
                {
                    return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour + 1, 5, 0);
                }
            }
            // If it's between xx:05 and xx:34, go to the xx:35 slot
            else if (minutes >= 5)
            {
                return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 35, 0);
            }
            // If it's before xx:05, go to xx:05
            else
            {
                return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 5, 0);
            }
        }

        private void FirstRefresh(object? sender, ElapsedEventArgs e)
        {
            // Refresh the page at the calculated initial time
            //InvokeAsync(() => NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true));
            // Ensure thread-safe operation
            InvokeAsync(() =>
            {
                if (NavigationManager != null)
                {
                    NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
                }
            });

            // After the first refresh, set the timer to refresh every 30 minutes
            refreshTimer = new System.Timers.Timer(30 * 60 * 1000); // 30 minutes in milliseconds
            refreshTimer.Elapsed += SubsequentRefresh;
            refreshTimer.AutoReset = true;  // Keep refreshing every 30 minutes
            refreshTimer.Enabled = true;
        }

        private void SubsequentRefresh(object? sender, ElapsedEventArgs e)
        {
            // Refresh the page every 30 minutes after the initial refresh
            //InvokeAsync(() => NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true));

            // Ensure thread-safe operation
            InvokeAsync(() =>
            {
                if (NavigationManager != null)
                {
                    NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
                }
            });
        }

        public void Dispose()
        {
            // Clean up the timer when the component is disposed
            refreshTimer?.Dispose();
        }
        private async Task RequestWakeLock()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("requestWakeLock");
            }
            catch (JSException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Handle the error here, possibly re-invoke the script or log for further diagnosis
            }
        }

        public async Task ReleaseWakeLock()
        {
            await JSRuntime.InvokeVoidAsync("releaseWakeLock");
        }

    }
}
