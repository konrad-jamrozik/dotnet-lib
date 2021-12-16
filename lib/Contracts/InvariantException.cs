using System;

namespace Wikitools.Lib.Contracts;

public class InvariantException : Exception
{
    public InvariantException() { }
    public InvariantException(string message) : base(message) { }
    public InvariantException(string message, Exception innerException) : base(message, innerException) { }
}