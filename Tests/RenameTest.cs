
using JBRenamer;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;

namespace Tests;

public class RenameTest
{
    [Fact]
    public void TrailingSpacesTest()
    {
        RulesModel rules = new RulesModel();
        rules.AddReplaceRule("with", "w");

        var fs = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux));
        fs.InitializeIn("/path/to/test").With(
            new FileDescription("filename with trailing space.txt "),
            new DirectoryDescription("directory with trailing space ")
        );

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.AddSourceDirectory(new Uri("file:///path/to/test"));
        
        Assert.Equal(2, files.Count());

        files.RunRules(rules);
        files.RenameFiles(rules);
        
        Assert.True(fs.File.Exists("/path/to/test/filename w trailing space.txt "));
        Assert.True(fs.Directory.Exists("/path/to/test/directory w trailing space "));
        Assert.False(fs.File.Exists("/path/to/test/filename with trailing space.txt "));
        Assert.False(fs.Directory.Exists("/path/to/test/directory with trailing space "));
    }
}
