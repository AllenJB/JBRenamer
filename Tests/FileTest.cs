using System.IO.Abstractions;
using JBRenamer;
using JBRenamer.Rules;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
using File = JBRenamer.File;

namespace Tests;

public class FileTest
{
    [Fact]
    public void ResolvePathChanges()
    {
        RulesModel rules = new RulesModel();
        rules.Add(new RegExpRule("^", "../"));

        IFileSystem fs = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux));
        fs.InitializeIn("/path/to/test").With(
            new FileDescription("file.ext")
        );

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        File file = files.AddSourceFile(new Uri("file:///path/to/test/file.ext"));
        
        files.RunRules(rules);
        
        Assert.Equal("/path/to/file.ext", file.Destination);
    }
}
