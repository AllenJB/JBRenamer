using System.Text.RegularExpressions;

namespace JBRenamer.Rules;

public class PrefixRule : Rule
{
    private string Value;
    
    public PrefixRule(string value) : base("Prefix")
    {
        Value = value;
    }

    public override string Run(string sourceUri)
    {
        FileInfo file = new FileInfo(sourceUri);
        string path = (file.DirectoryName ?? string.Empty);
        string extension = file.Extension;
        string fileNameNoExt = file.Name.Substring(0, file.Name.Length - (extension.Length));

        // TODO Replace with proper metadata system
        string prefix = Value.Replace(":File_FolderName:", (file.Directory?.Name ?? ""));

        fileNameNoExt = prefix + fileNameNoExt;
        return path + Path.DirectorySeparatorChar + fileNameNoExt + extension;
    }

    public override string Describe()
    {
        return "Prefix with \"" + Value + "\"";
    }
}
