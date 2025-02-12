using App.Db;
using App.Repositories;
using App.Models;
using App.Constants;
using App.Routes;
using App.Pkg;
using Prometheus;
using System.Diagnostics;
using App.Utils;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Coravel;
using Microsoft.Extensions.FileProviders;
using App.Routes.Users;
using App.Routes.Workspaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddLogging(options =>
{
    options.AddConsole();  // Adds console logging
    options.AddDebug();    // Adds debug output (optional)
    options.SetMinimumLevel(LogLevel.Information);  // Set minimum log level to Information
});

builder.Services.ConfigureHttpJsonOptions(
    options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    }
    );
var config = builder.Configuration;
builder.Services.AddSingleton<IAppSettings>(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);
var dbConnection = config.GetValue<string>("SeriousApp:DB:ConnectionString");
var secretKey = config.GetValue<string>("SeriousApp:JWT:Secret");
var expiresInMinutes = config.GetValue<int>("SeriousApp:JWT:ExpiresInMinutes");
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddSingleton<IAppSettings, AppSettings>();
builder.Services.AddScoped<IGenerator, Generator>();
builder.Services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(dbConnection ?? string.Empty));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApiUrlRepository, ApiUrlRepository>();
builder.Services.AddScoped<IApiUrlPermissionRepository, ApiUrlPermissionRepository>();
builder.Services.AddScoped<IUserRoleAssigneeRepository, UserRoleAssigneeRepository>();
builder.Services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
builder.Services.AddScoped<IPgLockRepository, PgLockRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IUserRolePermissionRepository, UserRolePermissionRepository>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceParticipantRoleRepository, WorkspaceParticipantRoleRepository>();
builder.Services.AddScoped<IWorkspaceParticipantRepository, WorkspaceParticipantRepository>();
builder.Services.AddScoped<IJWTPkg>(_ => new JWTPkg(secretKey ?? throw new ArgumentNullException(nameof(secretKey)), expiresInMinutes));
builder.Services.AddScheduler();
builder.Services.AddTransient<App.Worker.Scheduler.ApiUrlPermissionCleaner>();
builder.Services.AddTransient<App.Worker.Scheduler.ApiUrlCleaner>();

var app = builder.Build();
// using (var scope = app.Services.CreateScope())
// {
//     using var cts = new CancellationTokenSource();
//     // Simulate a cancellation after 5 seconds
//     cts.CancelAfter(5000);
//     var lockRepo = scope.ServiceProvider.GetRequiredService<IPgLockRepository>();
//     lockRepo.ReleaseAllLockAsync(cancellationToken: cts.Token).GetAwaiter().GetResult();
// }
// app.Services.UseScheduler(scheduler =>
// {
//     var appSettings = app.Services.GetRequiredService<IAppSettings>();

//     if (appSettings.SeriousApp.Worker.ApiUrlPermissionCleaner.IsEnabled)
//     {
//         scheduler
//         .Schedule<App.Worker.Scheduler.ApiUrlPermissionCleaner>()
//         .Cron(appSettings.SeriousApp.Worker.ApiUrlPermissionCleaner.CronJob)
//         .RunOnceAtStart()
//         ;
//     }

//     if (appSettings.SeriousApp.Worker.ApiUrlCleaner.IsEnabled)
//     {
//         scheduler
//         .Schedule<App.Worker.Scheduler.ApiUrlCleaner>()
//         .Cron(appSettings.SeriousApp.Worker.ApiUrlCleaner.CronJob)
//         .RunOnceAtStart()
//         ;
//     }

// });


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:8000/scalar
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "/assets"
});
app.MapFallbackToFile("index.html");
app.MapGroup("/api/v1/users")
    .MapUsersApi()
    .WithTags("Users");

app.MapGroup("/api/v1/users/auth")
    .MapUsersAuthApi()
    .WithTags("UsersAuth");

app.MapGroup("/api/v1/workspaces")
    .MapWorkspacesApi()
    .WithTags("Workspaces");

app.MapGroup("/api/v1/api-urls")
    .MapApiUrls()
    .WithTags("ApiUrls");

app.MapGroup("/api/v1/user-permissions")
    .MapUserPermissionsApi()
    .WithTags("UserPermissions");

app.MapGroup("/api/v1/locks")
    .MapLocks()
    .WithTags("Locks");

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // Inject logger
    var logId = Guid.NewGuid().ToString();

    // Get the current date and time in UTC format
    var timestamp = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

    // Assign a unique RequestId to the context (used for logging scope)
    context.Items[LogConstants.LOG] = logId;

    // Log the request details with timestamp
    logger.LogInformation("Request started at {Timestamp}, RequestId: {RequestId}, Method: {Method}, Path: {Path}, Query: {Query}",
        timestamp, logId, context.Request.Method, context.Request.Path, context.Request.QueryString);

    try
    {
        await next(); // Process the request
    }
    catch (Exception ex)
    {
        var res = new ResponseError
        {
            Error = new ResponseErrorData
            {
                Message = "Internal Server Error",  // General fallback message
                Code = ErrorConstants.InternalServerError,
                Details = ex.Message,  // Provide detailed error message
                Log = logId
            }
        };
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Handle known exceptions
        if (ex is NotFoundException notFoundException)
        {
            res.Error.Message = "Not found";
            res.Error.Code = notFoundException.Code ?? "NOT_FOUND";
            res.Error.Details = ex.Message ?? "";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        else if (ex is BadHttpRequestException badHttpRequestException)
        {
            res.Error.Message = ex.Message ?? "Bad request";
            res.Error.Code = ErrorConstants.BAD_REQUEST;
            res.Error.Details = badHttpRequestException.ToString() ?? "";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else if (ex is BadRequestException badRequestException)
        {
            res.Error.Message = "Bad request";
            res.Error.Code = ErrorConstants.BAD_REQUEST;
            res.Error.Details = badRequestException.Errors;
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else if (ex is UnauthorizedException unauthorizedException)
        {
            res.Error.Message = "Unauthorized";
            res.Error.Code = unauthorizedException.Code;
            res.Error.Details = unauthorizedException.Message;
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else if (ex is ConflictException conflictException)
        {
            res.Error.Message = "Conflict";
            res.Error.Code = conflictException.Code;
            res.Error.Details = conflictException.Message;
            context.Response.StatusCode = StatusCodes.Status409Conflict;
        }
        // Log the error with timestamp
        logger.LogError(ex, "Error occurred at {Timestamp} during request with RequestId: {RequestId}, StatusCode: {StatusCode}",
            timestamp, logId, context.Response.StatusCode);

        // Set the response format
        context.Response.ContentType = "application/json";

        // Return the structured error response
        await context.Response.WriteAsJsonAsync(res);
    }
});

var metricsServer = new MetricServer(port: 1234); // http://localhost:1234/metrics
metricsServer.Start();

// Start CPU and memory metrics monitoring in a background task
var cpuGauge = Metrics.CreateGauge("cpu_usage_milliseconds", "CPU usage in milliseconds");
var memoryGauge = Metrics.CreateGauge("memory_usage_bytes", "Memory usage in bytes");

_ = Task.Run(async () =>
{
    while (true)
    {
        var process = Process.GetCurrentProcess();
        cpuGauge.Set(process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount);
        memoryGauge.Set(process.WorkingSet64);

        await Task.Delay(1000); // Update every second
    }
});

app.Run();