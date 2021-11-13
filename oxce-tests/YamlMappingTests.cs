using System.Linq;
using NUnit.Framework;

namespace OxceTests
{
    [TestFixture]
    public class YamlMappingTests
    {
        [Test]
        public void TestYamlMapping()
        {
            var yamlMapping = new YamlMapping(
                new[]
                {
                    "key1: 5",
                    "key2:  ",
                    "  - foo",
                    "  - bar",
                    "entry3: 42",
                    "key 4: - qux"
                });

            Assert.Multiple(() =>
            {
                // Acts
                Assert.That(yamlMapping.Lines("key1"),   Is.EquivalentTo(new[] { "5" }));
                Assert.That(yamlMapping.Lines("key2"),   Is.EquivalentTo(new[] { "- foo", "- bar" }));
                Assert.That(yamlMapping.Lines("entry3"), Is.EquivalentTo(new[] { "42" }));
                Assert.That(yamlMapping.Lines("key 4"),  Is.EquivalentTo(new[] { "- qux" }));

            });
        }

        [Test]
        public void TestYamlMappingKeyValuePairs()
        {
            var yamlMapping = new YamlMapping(
                new[]
                {
                    "key1:",
                    "  {}",
                });
            var nestedYamlMapping = new YamlMapping(yamlMapping.Lines("key1"));

            Assert.That(nestedYamlMapping.KeyValuePairs().Count(), Is.Zero);
        }
    }
}