using System;

namespace Wikitools.AzureDevOps
{
    public class ResourceException : Exception
    {
        private const string TsgPrefixFormat = "===== ResourceException. Code: {0}. TSG:\n";
        private const string TsgPartConsult =
            "Please consult the stack trace of the base exception for details of the access.\n";
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
            "that was deleted, moved or otherwise became unavailable.";

        private const string TsgUnknown =
            "And access to external resource failed for an unknown reason.\n" +
            TsgPartConsult +
            "Note this failure did not match known patterns for (a) unauthorized access or (b) missing resources. " +
            "Possibly the patterns are incorrect, or if not, this kind of failure does not yet have corresponding TSG.";

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
                ExceptionCode.Unknown => TsgUnknown,
                _ => throw new ArgumentOutOfRangeException()
            } + $"\n{TsgBaseExceptionHeader}{base.ToString()}";
    }
}