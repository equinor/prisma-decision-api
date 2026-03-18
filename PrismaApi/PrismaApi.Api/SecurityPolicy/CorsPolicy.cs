using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PrismaApi.Api.SecurityPolicy
{
    public static class CorsPolicy
    {
        public const string AllowOriginsPolicy = "AllowOriginsPolicy";
        public static void AddCorsPolicy(CorsPolicyBuilder policy, string[] allowedOrigins)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }

    }
}
