// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Test;

namespace Sample.Identity
{
    public static class Config
    {
        private const string ApiName = "Api1";

        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource(ApiName, "My Api"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // machine to machine client (from quickstart 1)
                new Client
                {
                    ClientId = "client1",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // no interactive user, use ClientId & secret
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    AllowedScopes = {ApiName}
                },
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RequireConsent = false,
                    RequirePkce = true,

                    // interactive user
                    AllowedGrantTypes = GrantTypes.Code,

                    // where to redirect to after login => points to mvc client urls
                    RedirectUris = {"https://localhost:6002/signin-oidc"},

                    // where to redirect to after logout => points to mvc client urls
                    PostLogoutRedirectUris = {"https://localhost:6002/signout-callback-oidc"},

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile, 
                        ApiName // can access the api1
                    },
                    
                    AllowOfflineAccess = true, //enables refresh token
                },
            };


        public static List<TestUser> TestUsers =>
            new List<TestUser>
            {
                new TestUser
                {
                    Username = "shahab", Password = "ganji",
                    SubjectId = Guid.NewGuid().ToString()
                }
            };
    }
}
