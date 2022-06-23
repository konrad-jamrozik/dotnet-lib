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
                    "key1: 5 # comment to remove 1",
                    "key2:  ",
                    "  - foo # comment to remove 2",
                    "  - bar",
                    "entry3: 42",
                    "key 4: - qux  "
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
        public void TestYamlMappingKeyVaulePairsNoElements()
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

        [Test]
        public void TestYamlMappingKeyValuePairs()
        {
            var yamlMapping = new YamlMapping(
                new[]
                {
                    "key1: # comment 1",
                    "# comment 2",
                    "  - name: foo # comment 2",
                    "  - type: bar   ",
                    "key2: quz # comment 3"
                });
            var nestedYamlMapping = new YamlMapping(yamlMapping.Lines("key1"));

            // Act
            var actual = nestedYamlMapping.KeyValuePairs().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(actual.Count, Is.EqualTo(2));
                Assert.That(actual[0].Value, Is.EqualTo("foo"));
                Assert.That(actual[1].Value, Is.EqualTo("bar"));
            });
        }
    }
}