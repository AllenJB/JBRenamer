using System.IO.Abstractions;
using JBRenamer;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
using File = JBRenamer.File;

namespace Tests;

public class FilesModelTest
{
    [Fact]
    public void DetectDestinationConflictsTest()
    {
        RulesModel rules = new RulesModel();
        rules.AddReplaceRule(".with.dots", "");

        IFileSystem fs = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux));
        fs.InitializeIn("/path/to/test").With(
            new FileDescription("filename.txt"),
            new FileDescription("filename.with.dots.txt")
        );

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        File fileA = files.AddSourceFile(new Uri("file:///path/to/test/filename.txt"));
        File fileB = files.AddSourceFile(new Uri("file:///path/to/test/filename.with.dots.txt"));
        
        files.RunRules(rules);
        
        Assert.Equal(FileStatus.Conflict, fileA.Status);
        Assert.Equal(FileStatus.Conflict, fileB.Status);
    }
}
