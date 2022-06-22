using System.Linq;
using NUnit.Framework;

namespace OxceTests
{
    [TestFixture]
    public class YamlBlockSequenceTests
    {
        [Test]
        public void TestYamlBlockSequence()
        {
            var yamlBlockSequence = new YamlBlockSequence(
                new[]
                {
                    "- foo: 1",
                    "- bar: qux",
                    "  categories:",
                    "# comment 1",
                    "    - name: cat1",
                    "          # comment 2",
                    "    - name: cat2",
                    "-",
                    "  42",
                });

            // Act
            var nodesLines = yamlBlockSequence.NodesLines();

            var actual = nodesLines.ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, actual.Length);
                Assert.That(actual[0], Is.EquivalentTo(new[]
                {
                    "foo: 1" 
                }));
                Assert.That(actual[1], Is.EquivalentTo(new[]
                {
                    "bar: qux",
                    "categories:",
                    "  - name: cat1",
                    "  - name: cat2"
                }));
                Assert.That(actual[2], Is.EquivalentTo(new[]
                {
                    "42"
                }));
            });
        }
    }
}