using NUnit.Framework;

namespace Wikitools.AzureDevOps.Tests
{
    [TestFixture]
    public class WikiPageStatsPathTests
    {
        [Test]
        public void FromFileSystemPath()
        {
            (string input, string expected)[] testCasesData =
            {
                (@"foo.md", "/foo"), 
                (@"foo\bar.md", "/foo/bar"), 
                (@"a+b.md","/a+b"),
                (@"foo\bar-qux\bar%2Dqux\quuz%3F.md", "/foo/bar qux/bar-qux/quuz?"),
                (@"1(08%2D07%2D2020)%22.md","/1(08-07-2020)\"")
            };
            Assert.Multiple(() =>
            {
                foreach (var testCaseData in testCasesData)
                {
                    Assert.That(
                        // Act
                        WikiPageStatsPath.FromFileSystemPath(testCaseData.input).Path, 
                        Is.EqualTo(testCaseData.expected));    
                }
            });
        }

    }
}