using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Brio.Docs.Api.Validators;
using Brio.Docs.Utility.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Brio.Docs.Api
{
    public class Startup
    {
        private static readonly string SWAGGER_DOCUMENT_ID = "document-management";

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connection))
            {
                Log.Fatal("DefaultConnection is not defined in configuration");
                Log.Information("Terminating");
                Environment.Exit(1);
                return;
            }

            services.AddHttpClient("ApiClient", client =>
            {
                // Устанавливаем базовый адрес API из конфигурации
                client.BaseAddress = new Uri(Configuration["DMApi:BaseAddress"]);
                // Устанавливаем таймаут запроса
                client.Timeout = TimeSpan.FromSeconds(30);
                // Другие настройки HttpClient
                // client.DefaultRequestHeaders.Add("HeaderName", "HeaderValue");
            });

            services.AddDbContext<Database.DMContext>(options =>
            {
                options.UseSqlite(connection);
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddMvc()
                    .AddDataAnnotationsLocalization(options =>
                    {
                        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedLocalization));
                    });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = false,
                    };
                });

            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddCors();

            services.AddSwaggerGen(c =>
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                c.SwaggerDoc(SWAGGER_DOCUMENT_ID, new OpenApiInfo
                {
                    Version = assemblyName.Version?.ToString(),
                    Title = assemblyName.Name,
                    Description = "Documentation for Brio.Docs.API",
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{assemblyName.Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            services.AddDocumentManagement();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{SWAGGER_DOCUMENT_ID}/swagger.json", "Brio.Docs.API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            var supportedCultures = new[]
            {
                new CultureInfo("ru-RU"),
                new CultureInfo("en-US"),
            };

            app.UseRequestLocalization(options =>
            {
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.DefaultRequestCulture = new RequestCulture("en-US");
            });

            // TODO: uncomment and add Authenticate attribute to all controllers when roles are ready
            // app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
