using JBRenamer;
using File = JBRenamer.File;

namespace Tests;

public class FilesModelTest
{
    [Fact]
    public void DetectDestinationConflictsTest()
    {
        RulesModel rules = new RulesModel();
        rules.AddReplaceRule(".with.dots", "");

        FilesModel files = new FilesModel();
        File fileA = files.AddSourceFile(new Uri("file:///path/to/test/filename.txt"));
        File fileB = files.AddSourceFile(new Uri("file:///path/to/test/filename.with.dots.txt"));
        
        files.RunRules(rules);
        
        Assert.Equal(FileStatus.Conflict, fileA.Status);
        Assert.Equal(FileStatus.Conflict, fileB.Status);
    }
}
