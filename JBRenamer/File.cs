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
    public string Source { get; init; }

    private FileInfo SourceFile;

    public string Destination { get; private set; }

    public FileStatus Status = FileStatus.Ready;

    public string? Error = null;

    public File(string sourcePath)
    {
        this.Source = sourcePath;
        this.SourceFile = new FileInfo(this.Source);
        Destination = sourcePath;
    }

    public object DisplayValue => Source;
    
    public void RunRules(RulesModel rulesModel)
    {
        string newUri = Source;
        foreach (Rule rule in rulesModel.Rules)
        {
            newUri = rule.Run(newUri);
        }

        FileInfo newDest = new FileInfo(newUri);

        Destination = newDest.FullName;
        Debug.WriteLine("New destination: " + Destination);
    }
}
