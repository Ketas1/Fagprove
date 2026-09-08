using System.Net;

namespace SportForAlle.Tests.TestSupport;

/// <summary>
/// Returns a fixed status code for every request, so tests can substitute it
/// for the real "EmailJs" named <see cref="HttpClient"/> and never make a
/// real network call to EmailJS's API - see
/// <see cref="AuthenticatedWebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public class FakeHttpMessageHandler(HttpStatusCode statusCode, string content = "") : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(content) });
}
