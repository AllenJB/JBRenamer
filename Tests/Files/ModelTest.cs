using System.IO.Abstractions;
using JBRenamer.Files;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
using File = JBRenamer.Files.File;
using FilesModel = JBRenamer.Files.Model;
using RulesModel = JBRenamer.Rules.Model;

namespace Tests.Files;

public class ModelTest
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
