using System.IO;
using System.Text.RegularExpressions;

namespace JBRenamer.Rules.Type;

public class RegExpRule : Rule
{
    public string Search { get; init;  }

    public string Replace { get; init; }

    // TODO Validate regexp
    public RegExpRule(string searchPattern, string replace) : base("RegExp")
    {
        Search = searchPattern;
        Replace = replace;
    }

    internal RegExpRule() : base("RegExp")
    {
        Search = Replace = "Deserialized";
    }

    public override string Run(string sourceUri)
    {
        FileInfo file = new FileInfo(sourceUri);
        string path = (file.DirectoryName ?? string.Empty);
        string extension = file.Extension;
        string fileNameNoExt = file.Name.Substring(0, file.Name.Length - (extension.Length));

        fileNameNoExt = Regex.Replace(fileNameNoExt, Search, Replace);
        return path + Path.DirectorySeparatorChar + fileNameNoExt + extension;
    }

    public override string Describe()
    {
        return "Replace expression \"" + Search + "\" with \"" + Replace + "\"";
    }
}
