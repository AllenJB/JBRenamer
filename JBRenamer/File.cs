using System.Diagnostics;
using Qt.Bridge.Models;

namespace JBRenamer;

public enum FileStatus
{
    Ready,
    Conflict,
    Error,
    Renamed,
}

public class File : IDisplayable
{
    public string Source { get; init; }

    public FileSystemInfo SourceFile { get; private set; }

    public string Destination { get; private set; }
    
    public FileSystemInfo DestinationFile { get; private set; }

    public FileStatus Status = FileStatus.Ready;

    public string? Error = null;

    public File(string sourcePath)
    {
        Source = sourcePath;
        SourceFile = new FileInfo(Source);
        Destination = sourcePath;
        DestinationFile = new FileInfo(Destination);
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
        DestinationFile = new FileInfo(Destination);
        Debug.WriteLine("New destination: " + Destination);
    }
}
