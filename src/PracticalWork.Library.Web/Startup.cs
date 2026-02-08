using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Cache.Redis;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Data.PostgreSql;
using PracticalWork.Library.Web.Configuration;
using PracticalWork.Reports.MessageBroker.RabbitMQ;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Contracts.v2.Messaging;
using PracticalWork.Library.Web.Middleware;
using PracticalWork.Reports.Data.PostgreSql;
using RabbitMQ.Client;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace PracticalWork.Library.Web;

public class Startup
{
    private static string _basePath;
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        _basePath = string.IsNullOrWhiteSpace(Configuration["GlobalPrefix"]) ? "" : $"/{Configuration["GlobalPrefix"].Trim('/')}";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddPostgreSqlStorage(options =>
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });

        services.AddDbContext<ReportsDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("ReportsDb"))
        );
        
        services.AddRedisCache(Configuration);
        services.AddMinioFileStorage(Configuration);
        services.AddSingleton(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });
        
        services.AddHostedService<LibraryEventsConsumer>();
        
        services.AddDomain();

    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("x-api-version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    services.AddControllers();

    services.AddSwaggerGen(c =>
    {
        c.UseOneOfForPolymorphism();
        c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PracticalWork.Library.Contracts.xml"));
        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PracticalWork.Library.Controllers.xml"));
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    
    services.ConfigureOptions<ConfigureSwaggerOptions>();

        RegisterApplicationServices(services);
    }
    
    private void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<ICacheKeyGenerator, CacheKeyGenerator>();
        services.AddScoped<IMessagePublisher, RabbitMessagePublisher>();
    }

    [UsedImplicitly]
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
        ILogger logger, IServiceProvider serviceProvider)
    {
        app.UsePathBase(new PathString(_basePath));

        app.UseRouting();
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            app.UseSwagger();
            
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }
            });
            
            endpoints.MapControllers();
        });
    }
}