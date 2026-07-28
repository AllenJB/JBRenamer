using System.Diagnostics;
using Qt.Bridge.Models;

namespace JBRenamer;

public enum FileStatus
{
    Ready,
    Error,
    Renamed,
}

public class File : IDisplayable
{
    public string source { get; init; }

    private FileInfo sourceFile;

    public string destination { get; private set; }

    public FileStatus status = FileStatus.Ready;

    public string? error = null;

    public File(string sourcePath)
    {
        this.source = sourcePath;
        this.sourceFile = new FileInfo(this.source);
        destination = sourcePath;
    }

    public object DisplayValue => source;
    
    public void RunRules(RulesModel rulesModel)
    {
        string newUri = source;
        foreach (Rule rule in rulesModel.rules)
        {
            newUri = rule.Run(newUri);
        }

        destination = newUri;
        Debug.WriteLine("New destination: " + destination);
    }
}