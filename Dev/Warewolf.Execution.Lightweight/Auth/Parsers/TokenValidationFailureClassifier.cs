/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.IdentityModel.Tokens;

namespace Warewolf.Execution.Lightweight.Auth.Parsers;

/// <summary>
/// WOLF-8512: classifies a failure from <see cref="EntraBearerTokenValidator.ValidateAsync"/>
/// (other than "our own invocation was cancelled" — see
/// <c>Functions.ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation</c>, which is
/// evaluated first and handles that case on its own) as transient (retry via Service Bus
/// redelivery) or terminal (a positively-decided bad token — dead-letter, no retry).
///
/// <para>
/// Transient: anything that is not a definitive, HTTP-200-backed token verdict —
/// <see cref="HttpRequestException"/>, <see cref="IOException"/>, <see cref="SocketException"/>
/// (all indicating the OIDC metadata/JWKS fetch itself failed, not that the token was
/// evaluated and rejected), <see cref="SecurityTokenSignatureKeyNotFoundException"/>, and any
/// exception (walking <see cref="Exception.InnerException"/>) whose message contains
/// <c>IDX20803</c>/<c>IDX20804</c> (IdentityModel's own codes for "could not retrieve/reload
/// metadata"). Any exception type not explicitly recognised also defaults to transient — fail
/// open to a retry, which the queue's own <c>MaxDeliveryCount</c> still bounds, rather than
/// risk misclassifying a not-yet-seen transient shape as a permanent, zero-retry dead-letter.
/// </para>
///
/// <para>
/// Terminal: a token that was actually evaluated against a resolved signing key and rejected —
/// <see cref="SecurityTokenInvalidSignatureException"/>,
/// <see cref="SecurityTokenInvalidAudienceException"/>,
/// <see cref="SecurityTokenInvalidIssuerException"/>, <see cref="SecurityTokenExpiredException"/>,
/// <see cref="SecurityTokenMalformedException"/>, and <see cref="ArgumentException"/> (raw token
/// could not even be parsed as a JWT).
/// </para>
///
/// <para>
/// Deliberately excludes <see cref="OperationCanceledException"/>/<see cref="TaskCanceledException"/>
/// from its own contract — classifying "our own invocation was cancelled" requires the
/// invocation's <see cref="CancellationToken"/>, which this pure exception-only classifier does
/// not have, and that case is already handled by
/// <c>Functions.ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation</c>, evaluated in
/// an earlier <c>catch</c> clause. A bare cancellation-shaped exception reaching this classifier
/// (i.e. one <c>IsOwnInvocationCancellation</c> did not already claim) falls through to the
/// unknown-type fail-open default below, same as any other unrecognised type.
/// </para>
/// </summary>
internal static class TokenValidationFailureClassifier
{
    /// <summary>
    /// <c>true</c> when <paramref name="ex"/> (or any exception in its
    /// <see cref="Exception.InnerException"/> chain) indicates the token could not be
    /// evaluated at all, rather than being evaluated and found invalid.
    /// </summary>
    public static bool IsTransient(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            // Checked ahead of IsTerminal deliberately: SecurityTokenSignatureKeyNotFoundException
            // derives from SecurityTokenInvalidSignatureException, but "no matching key was found
            // in the currently-cached JWKS" is a metadata-resolution problem (transient — a
            // redelivery after the next JWKS refresh can succeed), not the terminal "the token was
            // evaluated against a resolved key and the signature was wrong" verdict its base type
            // represents.
            if (current is SecurityTokenSignatureKeyNotFoundException)
            {
                return true;
            }

            if (IsTerminal(current))
            {
                return false;
            }

            if (current is HttpRequestException
                or IOException
                or System.Net.Sockets.SocketException)
            {
                return true;
            }

            if (ContainsMetadataFailureCode(current.Message))
            {
                return true;
            }
        }

        // Unknown shape — fail open to a retry rather than risk a permanent, zero-retry
        // dead-letter for a transient cause this classifier doesn't yet recognise.
        return true;
    }

    /// <summary>
    /// The §2.6 dead-letter triage sub-reason for a terminal (non-transient) token-validation
    /// failure. Only meaningful when <see cref="IsTransient"/> returned <c>false</c> for the
    /// same exception.
    /// </summary>
    public static string ResolveSubReason(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            switch (current)
            {
                case SecurityTokenInvalidSignatureException:
                    return "SignatureInvalid";
                case SecurityTokenInvalidAudienceException:
                    return "AudienceInvalid";
                case SecurityTokenInvalidIssuerException:
                    return "IssuerInvalid";
                case SecurityTokenExpiredException:
                    return "Expired";
                case SecurityTokenMalformedException:
                case ArgumentException:
                    return "Malformed";
            }
        }

        return "Unclassified";
    }

    private static bool IsTerminal(Exception ex) =>
        ex is SecurityTokenInvalidSignatureException
            or SecurityTokenInvalidAudienceException
            or SecurityTokenInvalidIssuerException
            or SecurityTokenExpiredException
            or SecurityTokenMalformedException
            or ArgumentException;

    private static bool ContainsMetadataFailureCode(string? message) =>
        !string.IsNullOrEmpty(message)
        && (message.Contains("IDX20803", StringComparison.Ordinal)
            || message.Contains("IDX20804", StringComparison.Ordinal));
}
