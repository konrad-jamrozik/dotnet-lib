using System;

namespace Wikitools.AzureDevOps;

public class ResourceException : Exception
{
    private const string TsgPrefixFormat = "===== ResourceException. Code: '{0}'. TSG:\n";
    private const string TsgPartConsult =
        "Please consult the stack trace of the inner exception, provided below, " +
        "for details of the access failure.\n";
    private const string TsgPartCauses = "Common causes for such failure:\n";
    private const string TsgBaseExceptionHeader = "===== ResourceException base.ToString():\n";
    private const string TsgUnauthorized =
        "An access to an external resource has not been authorized.\n" +
        TsgPartConsult +
        TsgPartCauses +
        "- The call expected a bearer access token. " +
        "The provided token (often coming from environment variable) was missing, expired, malformed, " +
        "or didn't have relevant permissions.\n" +
        "- The authorization method was based on environment variables and/or configuration, " +
        "which were incorrect and/or missing.";
    private const string TsgNotFound =
        "An external resource was not found.\n" +
        TsgPartConsult +
        TsgPartCauses +
        "- The access is based on a configuration value, and that value points to an external resource " +
        "that was deleted, moved or otherwise became unavailable.\n" +
        "- The resource exists, but the principal accessing it does not have permission to access it, " +
        "and the resource returns 'Not Found' instead of 'Access Denied' to avoid disclosing information.";
    private const string TsgOther =
        "An access to external resource failed for a reason that has no dedicated TSG.\n" +
        TsgPartConsult +
        "Note this exception did not match any exception handling (\"catch\") " +
        "patterns that have dedicated TSGs.\n" +
        TsgPartCauses +
        "- The external resource was passed a value that is invalid and that comes from the runtime " +
        "configuration of this application. The runtime value is missing, wrong, or pointing to a resource " +
        "that is invalid because it was deleted, moved or otherwise became unavailable.\n" +
        "- There is a flaw in the internal logic of the application, exposed by this failure.\n" +
        "- The existing exception handling (\"catch\") patterns for external resource exceptions " +
        "did not take into account the thrown exception, and they need to be adjusted.\n" +
        "- It is a legitimate, not yet properly handled kind of external resource access failure, " +
        "that cannot be easily deduced based on the inner exception stack trace. " +
        "It requires investigation and addition of new exception handling (\"catch\") pattern " +
        "and the corresponding TSG.";

    private ExceptionCode Code { get; }

    public ResourceException(ExceptionCode code, Exception innerException) : base(null, innerException)
    {
        Code = code;
    }

    public override string ToString() =>
        string.Format(TsgPrefixFormat, Code) + Code switch
        {
            ExceptionCode.Unauthorized => TsgUnauthorized,
            ExceptionCode.NotFound => TsgNotFound,
            ExceptionCode.Other => TsgOther,
            _ => throw new ArgumentOutOfRangeException()
        } + $"\n{TsgBaseExceptionHeader}{base.ToString()}";
}