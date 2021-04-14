using System;

namespace Wikitools.AzureDevOps
{
    public class ResourceException : Exception
    {
        public ExceptionCode Code { get; }

        public ResourceException(ExceptionCode code, Exception innerException) : base(null, innerException)
        {
            Code = code;
        }
    }
}