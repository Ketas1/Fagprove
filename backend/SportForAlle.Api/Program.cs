using DotNetEnv;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

const string frontendCorsPolicy = "Frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod());
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(frontendCorsPolicy);
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
