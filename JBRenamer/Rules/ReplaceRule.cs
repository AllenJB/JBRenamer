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
        return new Uri(sourceUri.AbsolutePath.Replace(search, replace));
    }

    public override string Describe()
    {
        return "Replace \"" + search + "\" with \"" + replace + "\"";
    }
}
