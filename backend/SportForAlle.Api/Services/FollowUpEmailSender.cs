using Microsoft.Extensions.Options;
using SportForAlle.Api.Configuration;

namespace SportForAlle.Api.Services;

/// <summary>
/// Talks to EmailJS's REST API - nothing else. Domain logic (which loan,
/// which guardian, whether it's allowed to send) lives in
/// <see cref="LoanService.SendFollowUpEmailAsync"/>, not here.
/// </summary>
/// <remarks>
/// Called from the backend, not the browser - see
/// docs/adr/0022-emailjs-server-side.md. The <see cref="HttpClient"/> is
/// resolved from <see cref="IHttpClientFactory"/> under the name "EmailJs"
/// (registered in Program.cs) specifically so tests can substitute a fake
/// handler for that named client and never reach the real API - see
/// SportForAlle.Tests/TestSupport/AuthenticatedWebApplicationFactory.cs.
/// </remarks>
public class FollowUpEmailSender(IHttpClientFactory httpClientFactory, IOptions<EmailJsOptions> options)
{
    private const string _sendPath = "api/v1.0/email/send";

    /// <exception cref="EmailSendException">
    /// The request failed - a non-2xx response from EmailJS, or a network
    /// failure. Callers must not log a <see cref="Models.ContactAttempt"/>
    /// for an email that was not actually sent.
    /// </exception>
    public async Task SendAsync(IReadOnlyDictionary<string, string> templateParams, CancellationToken cancellationToken)
    {
        EmailJsOptions config = options.Value;
        HttpClient client = httpClientFactory.CreateClient("EmailJs");

        var body = new
        {
            service_id = config.ServiceId,
            template_id = config.TemplateId,
            user_id = config.PublicKey,
            accessToken = config.PrivateKey,
            template_params = templateParams,
        };

        HttpResponseMessage response;

        try
        {
            response = await client.PostAsJsonAsync(_sendPath, body, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new EmailSendException("Could not reach EmailJS.", exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            string detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new EmailSendException($"EmailJS returned {(int)response.StatusCode}: {detail}");
        }
    }
}

/// <summary>
/// Thrown by <see cref="FollowUpEmailSender.SendAsync"/> on any failure to
/// actually send. Never carries personal data - only EmailJS's own response
/// text, which is diagnostic (bad credentials, wrong service/template id),
/// not domain content, see CLAUDE.md "Never log personal data".
/// </summary>
public class EmailSendException : Exception
{
    public EmailSendException(string message)
        : base(message)
    {
    }

    public EmailSendException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
