using System.Collections.Generic;

namespace OxceTests
{
    public record BaseItems(string Name, IDictionary<string, int> Items);
}