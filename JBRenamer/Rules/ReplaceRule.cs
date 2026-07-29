namespace JBRenamer.Rules;

public class ReplaceRule : Rule
{
    private string Search;

    private string Replace;
    
    public ReplaceRule(string search, string replace): base("Replace")
    {
        this.Search = search;
        this.Replace = replace;
    }

    public override string Run(string sourceUri)
    {
        FileInfo file = new FileInfo(sourceUri);
        string path = (file.DirectoryName ?? string.Empty);
        string fileName = file.Name.Replace(Search, Replace);
        return path + Path.DirectorySeparatorChar + fileName;
    }

    public override string Describe()
    {
        return "Replace \"" + Search + "\" with \"" + Replace + "\"";
    }
}
