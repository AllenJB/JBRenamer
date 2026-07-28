namespace JBRenamer.Rules;

public class ReplaceRule : Rule
{
    private string search;

    private string replace;
    
    public ReplaceRule(string search, string replace): base("Replace")
    {
        this.search = search;
        this.replace = replace;
    }

    public override Uri Run(Uri sourceUri)
    {
        FileInfo file = new FileInfo(sourceUri.AbsolutePath);
        string path = (file.DirectoryName ?? string.Empty);
        string fileName = file.Name.Replace(search, replace);
        return new Uri(path + Path.DirectorySeparatorChar + fileName);
    }

    public override string Describe()
    {
        return "Replace \"" + search + "\" with \"" + replace + "\"";
    }
}
