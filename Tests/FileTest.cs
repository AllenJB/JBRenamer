using JBRenamer;
using JBRenamer.Rules;
using File = JBRenamer.File;

namespace Tests;

public class FileTest
{
    [Fact]
    public void ResolvePathChanges()
    {
        RulesModel rules = new RulesModel();
        rules.Add(new RegExpRule("^", "../"));

        FilesModel files = new FilesModel();
        File file = files.AddSourceFile(new Uri("file:///path/to/test/file.ext"));
        
        files.RunRules(rules);
        
        Assert.Equal("/path/to/file.ext", file.Destination);
    }
}
