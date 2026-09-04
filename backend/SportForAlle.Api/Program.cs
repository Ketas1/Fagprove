using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Npgsql;
using Scalar.AspNetCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Middleware;
using SportForAlle.Api.Services;

// The .env file lives at the repository root, not next to this project, so
// the frontend and backend can share one file - see docs/11-utviklingsmiljo.md.
// Loaded before CreateBuilder so its values reach configuration through the
// standard environment-variable provider ASP.NET Core already wires up.
// Absent entirely in CI and for anyone who has not created one yet, which is
// expected - it is gitignored, and configuration falls back to
// appsettings.Development.json and real environment variables instead.
string? rootEnvFile = FindEnvFile(Directory.GetCurrentDirectory());

if (rootEnvFile is not null)
{
    Env.Load(rootEnvFile);
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
        // Keeps action names identical to the C# method name, "Async" suffix
        // included - without this, ASP.NET Core strips it by convention, so
        // CreatedAtAction(nameof(GetByIdAsync), ...) fails to resolve a
        // route ("no route matches the supplied values") because the
        // registered action name is "GetById", not "GetByIdAsync".
        options.SuppressAsyncSuffixInActionNames = false)
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options =>
{
    // Describes the Bearer JWT scheme this API actually uses, so Scalar
    // renders a proper "Bearer Token" auth field instead of a raw custom
    // header - see docs/11-utviklingsmiljo.md.
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Auth0 access token. Get one from the Auth0 dashboard - your API - the Test tab."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddScoped<EquipmentCategoryService>();
builder.Services.AddScoped<GuardianService>();
builder.Services.AddScoped<BorrowerService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<LoanService>();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Host and port only, never the password - printed so a wrong value (the
// Docker-internal "db" instead of "localhost", for example) is visible
// immediately instead of only showing up as "database": "down" later.
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ [db] connection string not found");
}
else
{
    try
    {
        NpgsqlConnectionStringBuilder parsed = new(connectionString);
        Console.WriteLine($"✓ [db] connection string confirmed - host={parsed.Host} port={parsed.Port}");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"❌ [db] connection string present but could not be parsed: {exception.Message}");
    }
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// The frontend never calls this API from the browser - it proxies every
// request server-side and attaches the token itself, see
// docs/11-utviklingsmiljo.md and ADR-0015. There is deliberately no CORS
// policy: nothing but that proxy is ever a cross-origin caller.
string authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
string? audience = builder.Configuration["Auth0:Audience"];

// Neither value is a secret - printed once at startup so a misconfigured
// Auth0:Domain/Auth0:Audience is visible immediately instead of only
// showing up as an unexplained 401 later.
Console.WriteLine($"[auth] JWT bearer authority={authority} audience={audience}");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.Events = new JwtBearerEvents
        {
            // No token contents or personal data logged here - just why the
            // handler rejected or challenged a request, which is otherwise
            // invisible (a wrong Auth0:Domain, for example, silently
            // rejects every token with no clue as to why).
            OnAuthenticationFailed = context =>
            {
                Console.Error.WriteLine($"[auth] token rejected: {context.Exception}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.Error.WriteLine(
                    $"[auth] challenge issued. error={context.Error} description={context.ErrorDescription}");
                return Task.CompletedTask;
            },
        };
    });

// Every endpoint requires authentication by default - exceptions must be
// explicit ([AllowAnonymous]), not the other way around. See
// docs/06-autentisering.md, "Beskyttelse av backend".
builder.Services
    .AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Anonymous, deliberately, and only in this Development-gated block -
    // never in a real deployment. Otherwise the fallback policy above (every
    // endpoint requires authentication) would apply to /scalar and the spec
    // it reads too, and there would be no way to reach the page and paste in
    // a token if one wasn't already set - see docs/11-utviklingsmiljo.md.
    // The actual /api/* endpoints are untouched and stay fully protected -
    // that is the thing this UI exists to test.
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Sport For Alle API";
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    }).AllowAnonymous();
}

// Translates service-thrown exceptions into RFC 7807 ProblemDetails, see
// docs/05-api.md and Middleware/ProblemDetailsExceptionHandler.cs. Placed
// before authentication/authorization so it also covers anything they throw.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

/// <summary>Walks up from <paramref name="startDirectory"/> looking for a `.env` file, returning null if none exists anywhere up to the filesystem root.</summary>
static string? FindEnvFile(string startDirectory)
{
    for (DirectoryInfo? directory = new(startDirectory); directory is not null; directory = directory.Parent)
    {
        string candidate = Path.Combine(directory.FullName, ".env");

        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    return null;
}
