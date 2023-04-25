namespace DocMapper;

public class Tests
{
    public const string RepoClonePathHomeDirSuffixAzureRestApiSpecs = "/repos/azure-rest-api-specs";
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MapsDocs()
    {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        Console.Out.WriteLine("homeDir " + homeDir);
        
        string repoClonePathAzureRestApiSpecs =
            Path.Join(homeDir, RepoClonePathHomeDirSuffixAzureRestApiSpecs).Replace("\\", "/");
        
        Console.Out.WriteLine(repoClonePathAzureRestApiSpecs);

        Assume.That(
            Directory.Exists(repoClonePathAzureRestApiSpecs),
            Is.True,
            $"Directory.Exists(repoClonePathAzureRestApiSpecs={repoClonePathAzureRestApiSpecs})");
        
        Assert.Pass();
    }
}