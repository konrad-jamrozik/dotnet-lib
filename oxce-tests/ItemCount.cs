namespace OxceTests
{
    public record ItemCount(string Name, int Count, string BaseName)
    {
        public static string CsvHeaders()
        {
            return "ItemHeader";
        }

        public string CsvString()
        {
            return "ItemString";
        }
    }
}