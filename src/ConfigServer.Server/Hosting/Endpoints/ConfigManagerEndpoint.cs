﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ConfigServer.Server
{
    internal class ConfigManagerEndpoint : IEndpoint
    {
        private readonly IHttpResponseFactory httpResponseFactory;

        public ConfigManagerEndpoint(IHttpResponseFactory httpResponseFactory)
        {
            this.httpResponseFactory = httpResponseFactory;
        }

        public Task Handle(HttpContext context, ConfigServerOptions options)
        {
            var managerPath = context.Request.PathBase.Value;
            var basePath = GetBasePath(managerPath);
            if (!CheckMethodAndAuthentication(context, options))
                return Task.FromResult(true);
            return context.Response.WriteAsync(Shell(basePath, managerPath));
        }

        private bool CheckMethodAndAuthentication(HttpContext context, ConfigServerOptions options)
        {
            if (context.Request.Method == "GET")
            {
                return context.ChallengeUser(options.ClientAdminClaimType, new HashSet<string>(new[] { ConfigServerConstants.AdminClaimValue, ConfigServerConstants.ConfiguratorClaimValue }, StringComparer.OrdinalIgnoreCase), options.AllowAnomynousAccess, httpResponseFactory);
            }
            else
            {
                httpResponseFactory.BuildMethodNotAcceptedStatusResponse(context);
                return false; ;
            }
        }

        private string GetBasePath(string managerPath)
        {
            var basePath = managerPath;
            var desiredLength = basePath.Length - HostPaths.Manager.Length;
            var trimmedBasePath = basePath.Substring(0, desiredLength);
            return trimmedBasePath;
        }

        private string Shell(string basePath, string managerPath) => $@"
            <html>
            <head>
                <title>Configuration manager</title>
                <meta charset=""UTF-8"">
                <meta name = ""viewport"" content=""width=device-width, initial-scale=1"">
                <link href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"" rel=""stylesheet"" />
                <link rel = ""stylesheet"" href=""{basePath}/Assets/styles.css"">
                <!-- 1. Load libraries -->
                <!-- Polyfill(s) for older browsers -->
                <script src = ""https://unpkg.com/core-js/client/shim.min.js"" ></script>
                <script src=""https://unpkg.com/zone.js@0.6.25?main=browser""></script>
                <script src = ""https://unpkg.com/reflect-metadata@0.1.3""></script>
                <script src=""https://unpkg.com/systemjs@0.19.27/dist/system.src.js""></script>
                <!-- 2. Configure SystemJS -->
                <script>
                {Config(basePath)}
                </script>
                <script>
                  System.import('{basePath}/Assets/app.min.js').catch(function(err) {{ console.error(err); }});
                </script>
                <base href=""{managerPath}"" />
            </head>
            <!-- 3. Display the application -->
            <body>
                <config-server-shell>Loading...</config-server-shell>
            </body>
            </html>
            ";
        private string Config(string basePath) => $@"
            (function(global) {{
                System.config({{
        
                    paths: {{
                        // paths serve as alias
                        'npm:': 'https://unpkg.com/'
                    }},
                    map: {{
                        // angular bundles
                        '@angular/core': 'npm:@angular/core/bundles/core.umd.js',
                        '@angular/common': 'npm:@angular/common/bundles/common.umd.js',
                        '@angular/compiler': 'npm:@angular/compiler/bundles/compiler.umd.js',
                        '@angular/platform-browser': 'npm:@angular/platform-browser/bundles/platform-browser.umd.js',
                        '@angular/platform-browser-dynamic': 'npm:@angular/platform-browser-dynamic/bundles/platform-browser-dynamic.umd.js',
                        '@angular/http': 'npm:@angular/http/bundles/http.umd.js',
                        '@angular/router': 'npm:@angular/router/bundles/router.umd.js',
                        '@angular/forms': 'npm:@angular/forms/bundles/forms.umd.js',
                        '@angular/upgrade': 'npm:@angular/upgrade/bundles/upgrade.umd.js',

                        // other libraries
                        'rxjs': 'npm:rxjs',
                        'angular2-in-memory-web-api': 'npm:angular2-in-memory-web-api'

                    }}
                }});
            }})(this);";
    }
}
