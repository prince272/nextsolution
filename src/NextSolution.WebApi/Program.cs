using Microsoft.EntityFrameworkCore;
using NextSolution.Core;
using NextSolution.Infrastructure.Data;
using NextSolution.WebApi.Shared;
using Serilog;
using Serilog.Settings.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;
using NextSolution.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.EmailSender.MailKit;
using NextSolution.Infrastructure.ViewRenderer.Razor;
using NextSolution.Infrastructure.SmsSender;

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog(Log.Logger);

    // Add database services.
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));
    });

    builder.Services.AddRepositories();

    // Add identity services.
    builder.Services.AddIdentity<User, Role>(options =>
    {
        // Password settings. (Will be using fluent validation)
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 0;
        options.Password.RequiredUniqueChars = 0;

        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings.
        options.User.AllowedUserNameCharacters = string.Empty;
        options.User.RequireUniqueEmail = false;

        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Generate Short Code for Email Confirmation using Asp.Net Identity core 2.1
        // source: https://stackoverflow.com/questions/53616142/generate-short-code-for-email-confirmation-using-asp-net-identity-core-2-1
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;

        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
        options.ClaimsIdentity.SecurityStampClaimType = ClaimTypes.SerialNumber;
    })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddUserSession(builder.Configuration.GetRequiredSection("Authentication:Bearer"));

    builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    })
        .AddJwtBearer();

    builder.Services.AddAuthorization();

    builder.Services.AddMailKitEmailSender(builder.Configuration.GetRequiredSection("Mailing:MailKit"));
    builder.Services.AddFakeSmsSender();
    builder.Services.AddRazorViewRenderer();

    // Add application services.
    builder.Services.AddApplication();

    // Configure serialization services.
    builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

    // Add documentation services.
    builder.Services.AddDocumentation();

    // Build application.
    var app = builder.Build();

    app.UseStatusCodePagesWithReExecute("/errors/{0}");
    app.UseExceptionHandler(new ExceptionHandlerOptions()
    {
        AllowStatusCode404Response = true,
        ExceptionHandler = null,
        ExceptionHandlingPath = "/errors/500"
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    // app.UseCors();

    app.UseAuthorization();

    app.MapEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");

    throw;
}
finally
{
    Log.CloseAndFlush();
}