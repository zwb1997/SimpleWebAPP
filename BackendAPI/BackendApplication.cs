using System.Reflection;
using System.Threading.RateLimiting;
using BackendAPI.Schedule;
using Microsoft.OpenApi.Models;

namespace BackendAPI;

using AspNetCoreRateLimit;
using BackendAPI.Services;
using BackendAPI.Services.impl;
using BackendAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using NLog.Config;
using NLog.Web;
using System.Net;

public class BackendApplication
{
    public static void Main(string[] args)
    {
        ISetupBuilder nLogBuilder = NLog.LogManager.Setup().LoadConfigurationFromAppSettings();
        nLogBuilder.LoadConfigurationFromAppSettings();
        var builder = WebApplication.CreateBuilder(args);
        WebApplicationConfiguration(builder);
        var app = builder.Build();
        AppConfiguration(app);
        app.Run();
    }

    private static void AppConfiguration(WebApplication app)
    {
        // Configure CORS if necessary
        app.UseCors(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Use rate limiting middleware
        app.UseRateLimiter();

        //app.UseHttpsRedirection();
        app.UseRouting();
        
        app.UseAuthorization();
        //app.UseMiddleware<ByPassAuthMiddleware>();

        app.MapControllers(); // Ensure this line is included to map controller routes
    }

    private static void WebApplicationConfiguration(WebApplicationBuilder builder)
    {
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Host.UseNLog(); // NLog: Setup NLog for Dependency injection
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, 16610);       // Listen on IPv4
        });
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                options.JsonSerializerOptions.IgnoreReadOnlyFields = true;
                options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            });

        // Configure the DbContext with DI
        builder.Services.AddDbContext<AppDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c=>{
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Case Follow API", Version = "v1" });
            
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddControllers()
            .AddNewtonsoftJson(options => { options.SerializerSettings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }); });

        // Add rate limiting services
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 80, // Number of requests allowed in the window
                        Window = TimeSpan.FromMinutes(1), // Time window
                        // Window = TimeSpan.FromSeconds(10), // for dev
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 20 // Queue limit
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
            };
        });
        
        // Add framework services.
        builder.Services.AddControllers();

        // Add rate limiting middleware
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // Add custom rate limiting policies
        // builder.Services.AddSingleton<IRateLimitPolicyStore, RateLimitPolicyStore>();

        builder.Services.AddScoped<ICaseService, CaseService>();
        builder.Services.AddScoped<IFollowedCaseService, FollowedCaseService>();
        builder.Services.AddSingleton<IHostedService, SyncedFromMainService>();
    }
}
