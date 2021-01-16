using System.Text.Json;

namespace Wikitools.Lib.Json
{
    public static class JsonExtensions
    {
        public static string ToIndentedJsonString(this object obj)
        {
            return JsonSerializer.Serialize(obj,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
        }
    }
}