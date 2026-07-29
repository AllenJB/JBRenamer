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
        string extension = file.Extension;
        string fileNameNoExt = file.Name.Substring(0, file.Name.Length-(extension.Length));

        fileNameNoExt = fileNameNoExt.Replace(Search, Replace);
        return path + Path.DirectorySeparatorChar + fileNameNoExt + extension;
    }

    public override string Describe()
    {
        return "Replace \"" + Search + "\" with \"" + Replace + "\"";
    }
}
