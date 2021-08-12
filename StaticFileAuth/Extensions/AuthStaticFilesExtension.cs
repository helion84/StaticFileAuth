using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using StaticFileAuth.Cache;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace StaticFileAuth.Extensions
{
    public static class AuthStaticFilesExtension
    {
        private static IKeyCache _keyCache;
        private const string KeyName = "key";
        private const string DirectoryName = "static";

        public static IApplicationBuilder UseAuthStaticFiles(this IApplicationBuilder app, IKeyCache keyCache)
        {
            _keyCache = keyCache;

            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = GetOptions();

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<StaticFileMiddleware>(Options.Create(options));
        }

        public static StaticFileOptions GetOptions()
        {
            var staticFilePath = Path.Combine(Directory.GetCurrentDirectory(), DirectoryName);

            if (!Directory.Exists(staticFilePath))
            {
                Directory.CreateDirectory(staticFilePath);
            }

            return new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Request.Query.TryGetValue(KeyName, out StringValues key);

                    if (_keyCache.IsValid(key))
                    {
                        ctx.Context.Response.Cookies.Append(KeyName, key, new CookieOptions
                        {
                            SameSite = SameSiteMode.None,
                            Secure = true,
                            HttpOnly = true
                        });
                    }
                    else if (!ctx.Context.Request.Cookies.ContainsKey(KeyName) || !_keyCache.IsValid(ctx.Context.Request.Cookies[KeyName]))
                    {
                        ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }
                    ctx.Context.Response.Headers.Add(HeaderNames.CacheControl, CacheControlHeaderValue.NoStoreString);
                },

                FileProvider = new PhysicalFileProvider(staticFilePath),
                RequestPath = $"/{DirectoryName}"
            };
        }
    }
}
