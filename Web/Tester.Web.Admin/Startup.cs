using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using REST.DataCore.Contract;
using REST.DataCore.Contract.Provider;
using REST.EfCore.Context;
using REST.EfCore.Contract;
using REST.EfCore.Provider;
using REST.Infrastructure.Contract;
using REST.Infrastructure.Service;
using Tester.Db.Context;
using Tester.Db.Manager;
using Tester.Db.Provider;
using Tester.Db.Store;
using Tester.Infrastructure.Сontracts;
using Tester.Web.Admin.Services;

namespace Tester.Web.Admin
{
    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public Startup([NotNull] ILoggerFactory loggerFactory,
            [NotNull] IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddControllers();

            var defaultApiVersion = new ApiVersion(1, 0);
            services.AddApiVersioning(o =>
                {
                    o.ReportApiVersions = true;
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.DefaultApiVersion = defaultApiVersion;
                })
                .AddVersionedApiExplorer(o =>
                {
                    o.GroupNameFormat = "'v'VVV";
                    o.SubstituteApiVersionInUrl = true;
                })
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = $"Tester Admin API {defaultApiVersion}",
                        Version = defaultApiVersion.ToString()
                    });
                });


            services.AddEntityFrameworkNpgsql()
                .AddDbContext<TesterDbContext>((sp, ob) =>
                {
                    ob.UseNpgsql(_configuration.GetConnectionString("Postgres"));
                    if (_loggerFactory != null)
                    {
                        ob.UseLoggerFactory(_loggerFactory);
                    }
                })
                .AddScoped<ResetDbContext>(x => x.GetService<TesterDbContext>())
                .AddScoped<IDataProvider, EfDataProvider>()
                .AddScoped<IRoDataProvider>(x => x.GetService<IDataProvider>())
                .AddScoped<IModelStore, TesterDbModelStore>()
                .AddScoped<IAsyncHelpers, EfAsyncHelpers>()
                .AddScoped<IDataExceptionManager, PostgresDbExceptionManager>()
                .AddScoped<IIndexProvider, PostgresIndexProvider>();

            services.AddScoped<IRoleRoService, RoleRoService>();
            services.AddScoped<IFilterHelper, FilterHelper>();
            services.AddScoped<IExpressionHelper, ExpressionHelper>();
            services.AddScoped<IOrderHelper, OrderHelper>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }

        public static void Configure([NotNull] IApplicationBuilder app, [NotNull] IWebHostEnvironment env,
            [NotNull] IHostApplicationLifetime lifetime, [NotNull] IServiceProvider serviceProvider,
            IApiVersionDescriptionProvider provider)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (env == null) throw new ArgumentNullException(nameof(env));
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                foreach (var versionDescription in provider.ApiVersionDescriptions)
                {
                    o.SwaggerEndpoint($"/swagger/{(object) versionDescription.GroupName}/swagger.json",
                        versionDescription.GroupName.ToUpperInvariant());
                }
                o.EnableDeepLinking();
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}