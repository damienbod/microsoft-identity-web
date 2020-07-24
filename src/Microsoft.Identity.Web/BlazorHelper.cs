// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Helper class for processing the user challenge in a Blazor app.
    /// </summary>
    public static class BlazorHelper
    {
        /// <summary>
        /// Used on a controller action to trigger incremental consent.
        /// </summary>
        /// <param name="exception">Original exception.</param>
        /// <param name="authenticationStateProvider">AuthenticationStateProvider specific for Blazor.</param>
        /// <param name="navigationManager">NavigationManager.</param>
        /// <param name="scopes">Scopes to request.</param>
        /// <param name="nameOfPage">Name of Blazor page.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task ChallengeUserAsync(
            Exception exception,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            string[] scopes,
            string nameOfPage)
        {
            if (authenticationStateProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationStateProvider));
            }

            if (navigationManager == null)
            {
                throw new ArgumentNullException(nameof(navigationManager));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            MsalUiRequiredException? msalUiRequiredException =
                  (exception as MsalUiRequiredException)
                  ?? (exception?.InnerException as MsalUiRequiredException);

            if (msalUiRequiredException != null &&
               IncrementalConsentAndConditionalAccessHelper.CanBeSolvedByReSignInOfUser(msalUiRequiredException))
            {
                var user = (await authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false)).User;
                var properties = IncrementalConsentAndConditionalAccessHelper.BuildAuthenticationProperties(
                    scopes,
                    msalUiRequiredException,
                    user);

                // string redirectUri, string scope, string loginHint, string domainHint, string claims
                string redirectUri = navigationManager.BaseUri + $"/{nameOfPage}";
                List<string> scope = properties.Parameters.ContainsKey(Constants.Scope) ? (List<string>)properties.Parameters[Constants.Scope] : new List<string>();
                string loginHint = properties.Parameters.ContainsKey(Constants.LoginHint) ? (string)properties.Parameters[Constants.LoginHint] : string.Empty;
                string domainHint = properties.Parameters.ContainsKey(Constants.DomainHint) ? (string)properties.Parameters[Constants.DomainHint] : string.Empty;
                string claims = properties.Parameters.ContainsKey(Constants.Claims) ? (string)properties.Parameters[Constants.Claims] : string.Empty;
                string url = $"{navigationManager.BaseUri}{Constants.BlazorChallengeUri}{redirectUri}"
                + $"&{Constants.Scope}={string.Join(" ", scope)}&{Constants.LoginHint}={loginHint}"
                + $"&{Constants.DomainHint}={domainHint}&{Constants.Claims}={claims}";

                navigationManager.NavigateTo(url, true);
            }
        }

        /// <summary>
        /// Used on a controller action to trigger incremental consent.
        /// </summary>
        /// <param name="microsoftIdentityWebChallengeUserException">MicrosoftIdentityWebChallengeUserException from the user challenge.</param>
        /// <param name="authenticationStateProvider">AuthenticationStateProvider specific for Blazor.</param>
        /// <param name="navigationManager">NavigationManager.</param>
        /// /// <param name="nameOfPage">Name of Blazor page.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task ChallengeUserAsync(
            MicrosoftIdentityWebChallengeUserException microsoftIdentityWebChallengeUserException,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            string nameOfPage)
        {
            await ChallengeUserAsync(
                microsoftIdentityWebChallengeUserException,
                authenticationStateProvider,
                navigationManager,
                microsoftIdentityWebChallengeUserException.Scopes,
                nameOfPage).ConfigureAwait(false);
        }
    }
}
