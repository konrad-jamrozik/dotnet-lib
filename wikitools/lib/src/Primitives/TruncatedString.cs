namespace Wikitools.Lib.Primitives
{
    class TruncatedString
    {
        private readonly LazyString _truncated;

        public TruncatedString(string source, int maxLength)
        {
            _truncated = new LazyString(
                () =>
                {
                    if (source.Length <= maxLength)
                    {
                        return source;
                    }
                    else
                    {
                        return source.Substring(0, maxLength) + $"... (displaying first {maxLength} characters out of {source.Length})";
                    }
                });
        }

        public override string ToString()
        {
            return _truncated.Value;
        }
    }
}
