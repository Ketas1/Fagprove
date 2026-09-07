namespace SportForAlle.Api.Configuration;

/// <summary>
/// Configures <see cref="Services.FollowUpEmailSender"/>. Bound from the
/// "EmailJs" section - see appsettings.json and .env.example. All four
/// values must be supplied for real (no sensible default exists for a
/// third-party account's credentials); see docs/adr/0022-emailjs-server-side.md
/// for why the backend calls EmailJS directly instead of the browser SDK.
/// </summary>
public class EmailJsOptions
{
    public const string SectionName = "EmailJs";

    public string ServiceId { get; set; } = string.Empty;

    public string TemplateId { get; set; } = string.Empty;

    /// <summary>EmailJS's "Public Key" (their <c>user_id</c> field).</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// EmailJS's "Private Key" (their <c>accessToken</c> field) - required
    /// because this is a server-side call, not a browser one. Must also be
    /// enabled under EmailJS Account &gt; Security, which is a one-time
    /// manual step in their dashboard, not something this code can do.
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;
}
