using NextSolution.Server.Data;
using NextSolution.Server.Data.Entities.Identity;
using NextSolution.Server.Helpers;
using NextSolution.Server.Providers.JwtBearer;
using NextSolution.Server.Providers.Messaging.Arkesel;
using NextSolution.Server.Providers.SwaggerGen;
using NextSolution.Server.Providers.Validation;
using NextSolution.Server.Providers.ViewRender;
using NextSolution.Server.Services;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Settings.Configuration;
using Serilog.Sinks.SystemConsole.Themes;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NextSolution.Server.Providers.Messaging.MailKit;
using NextSolution.Server.Middlewares;
using NextSolution.Server.Extensions;
try
{
    // Set the default culture info to "en-GH"
    var defaultCultureInfo = new CultureInfo("en-GH");
    CultureInfo.DefaultThreadCurrentCulture = defaultCultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = defaultCultureInfo;

    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration,
        new ConfigurationReaderOptions { SectionName = "Logging:Serilog" }).Enrich.FromLogContext().CreateLogger();

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog(Log.Logger);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        var serializerOptions = options.SerializerOptions;
        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Application");
        options.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
    });

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
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<User, Role>>();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthentication();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
                    .AddJwtProvider(options =>
                    {
                        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins")?.Get<string[]>();
                        if (allowedOrigins != null && allowedOrigins.Length != 0) options.Issuer = string.Join(";", allowedOrigins);
                    })
                    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                    {
                        options.SignInScheme = IdentityConstants.ExternalScheme;
                        builder.Configuration.GetRequiredSection("Authentication:Google").Bind(options);
                    })
                    .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
                    {
                        options.SignInScheme = IdentityConstants.ExternalScheme;
                        builder.Configuration.GetRequiredSection("Authentication:Facebook").Bind(options);
                    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins")?.Get<string[]>() ?? Array.Empty<string>();

            policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithOrigins(allowedOrigins);
        });
    });


    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = false;
    })
                    .AddControllers()
                    .AddJsonOptions(options =>
                    {
                        var serializerOptions = options.JsonSerializerOptions;
                        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        serializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                        serializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                        serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    });

    // Add domain services to the container.
    builder.Services.AddScoped<IIdentityService, IdentityService>();

    // Add providers to the container.
    builder.Services.AddAutoMapper(AssemblyHelper.GetAppAssemblies());
    builder.Services.AddFluentValidationProvider();
    builder.Services.AddRazorViewRenderer();
    builder.Services.AddArkeselMessageSender(options =>
    {
        builder.Configuration.GetRequiredSection("Messaging:Arkesel").Bind(options);
    });
    builder.Services.AddMailKitMessageSender();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    await app.RunMigrationsAsync<ApplicationDbContext>();

    app.UseDbTransaction<ApplicationDbContext>();

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
        app.UseSwagger(options =>
        {
            options.SerializeAsV2 = true;
        });

        // How to use automatic variables in Swagger UI?
        // source: https://stackoverflow.com/questions/72773829/how-to-use-automatic-variables-in-swagger-ui
        app.UseSwaggerUI(swaggerUiOptions =>
        {
            var responseInterceptor = @"(res) => 
                {
                    if(res.obj.accessToken)
                    { 
                        console.log(res.obj.accessToken);
                        const token = res.obj.accessToken;
                        localStorage.setItem('token', token);
                    }; 
                    return res; 
                }";
            var requestInterceptor = @"(req) => 
                { 
                    req.headers['Authorization'] = 'Bearer ' + localStorage.getItem('token');
                    return req; 
                }";
            swaggerUiOptions.UseResponseInterceptor(Regex.Replace(responseInterceptor, @"\s+", " "));
            swaggerUiOptions.UseRequestInterceptor(Regex.Replace(requestInterceptor, @"\s+", " "));
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");

    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}