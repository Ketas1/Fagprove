namespace SportForAlle.Api.Validation;

/// <summary>
/// Thrown when a requested resource does not exist. Mapped to <c>404</c> by
/// <see cref="SportForAlle.Api.Middleware.ProblemDetailsExceptionHandler"/>.
/// </summary>
public class NotFoundException(string message) : Exception(message)
{
}
