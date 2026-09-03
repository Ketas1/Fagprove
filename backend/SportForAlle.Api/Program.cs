using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Helpers;

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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IClock, SystemClock>();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Presence only, never the value - it carries the database password.
Console.WriteLine(
    string.IsNullOrEmpty(connectionString)
        ? "[db] connection string NOT found"
        : "[db] connection string confirmed");

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
    app.MapOpenApi();
}

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
