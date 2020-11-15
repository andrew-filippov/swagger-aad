using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace swagger_aad
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration, subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);

            services
                .AddControllers(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                });


            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v0.1", new OpenApiInfo
                    {
                        Title = "Weather Forecast API",
                        Version = "v0.1",
                    });

                    var instance = configuration["AzureAd:Instance"];
                    var tenantId = configuration["AzureAd:TenantId"];
                    var clientId = configuration["AzureAd:ClientId"];

                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.OAuth2,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header,
                        OpenIdConnectUrl = new Uri($"{instance}{tenantId}/v2.0/.well-known/openid-configuration"),
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{instance}{tenantId}/oauth2/v2.0/authorize"),
                                Scopes = new Dictionary<string, string>{
                                    { $"api://{clientId}/access_to_api", "Access to Weather Forecast API"},
                                    },
                                TokenUrl = new Uri($"{instance}{tenantId}/oauth2/v2.0/token"),
                            }
                        }
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "oauth2",
                                }
                            },
                            new string[] { }
                        }
                    });

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });
        }

/// <summary>
/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
/// </summary>
/// <param name="app"></param>
/// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId(configuration["Swagger:ClientId"]);
                c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "Weather Forecast API v0.1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
