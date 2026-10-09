namespace JBRenamer.Rules;

public class ReplaceRule : Rule
{
    public string Search { get; init; }

    public string Replace { get; init; }

    public ReplaceRule(string search, string replace) : base("Replace")
    {
        this.Search = search;
        this.Replace = replace;
    }

    internal ReplaceRule() : base("Replace")
    {
        Search = Replace = "Deserialized";
    }

    public override string Run(string sourceUri)
    {
        FileInfo file = new FileInfo(sourceUri);
        string path = (file.DirectoryName ?? string.Empty);
        string extension = file.Extension;
        string fileNameNoExt = file.Name.Substring(0, (file.Name.Length - extension.Length));

        fileNameNoExt = fileNameNoExt.Replace(Search, Replace);
        return path + Path.DirectorySeparatorChar + fileNameNoExt + extension;
    }

    public override string Describe()
    {
        return "Replace \"" + Search + "\" with \"" + Replace + "\"";
    }
}
