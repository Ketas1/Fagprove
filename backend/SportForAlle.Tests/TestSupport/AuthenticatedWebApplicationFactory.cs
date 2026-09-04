using Microsoft.AspNetCore.Authentication;
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
public class AuthenticatedWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, options => { });
        });
    }
}
