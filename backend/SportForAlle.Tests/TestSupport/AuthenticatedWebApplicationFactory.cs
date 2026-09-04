using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace SportForAlle.Tests.TestSupport;

/// <summary>
/// A <see cref="WebApplicationFactory{TEntryPoint}"/> whose client is
/// authenticated by default, using <see cref="TestAuthHandler"/> instead of
/// the real Auth0 JWT bearer scheme. For tests that need to exercise an
/// endpoint's behaviour once past authentication, without a real token.
/// </summary>
/// <param name="seedLinkedStaff">
/// Passed straight through to <see cref="TestAuthHandlerOptions.SeedLinkedStaff"/>.
/// Set false to test the "authenticated but not linked to a Staff profile"
/// path deliberately - see docs/adr/0019-staff-auth0-mapping.md.
/// </param>
/// <param name="subject">
/// The fake "sub" claim value. Defaults to a shared, well-known value that
/// other tests also seed a linked Staff row for - pass a fresh
/// <see cref="Guid"/>-based value together with <paramref name="seedLinkedStaff"/>
/// <c>false</c> to guarantee an unlinked subject that cannot collide with a
/// Staff row created by another test running against the same shared
/// database.
/// </param>
public class AuthenticatedWebApplicationFactory<TEntryPoint>(bool seedLinkedStaff = true, string? subject = null)
    : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.SchemeName, options =>
                {
                    options.SeedLinkedStaff = seedLinkedStaff;

                    if (subject is not null)
                    {
                        options.Subject = subject;
                    }
                });
        });
    }
}
