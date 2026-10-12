using System.IO;

namespace JBRenamer.Rules.Type;

public class PrefixRule : Rule
{
    public string Value { get; init; }

    public PrefixRule(string value) : base("Prefix")
    {
        Value = value;
    }

    internal PrefixRule() : base("Prefix")
    {
        Value = "Deserialized";
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
