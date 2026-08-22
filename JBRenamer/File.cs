using System.Diagnostics;
using System.IO.Abstractions;
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
    private readonly IFileSystem Filesystem;

    public string Source { get; init; }

    public IFileSystemInfo SourceFile { get; private set; }

    public string Destination { get; private set; }
    
    public IFileSystemInfo DestinationFile { get; private set; }

    public FileStatus Status = FileStatus.Ready;

    public string? Error = null;

    public File(IFileSystemInfo sourceInfo, IFileSystem fs)
    {
        Filesystem = fs;
        Source = sourceInfo.FullName;
        SourceFile = sourceInfo;
        SetDestination(Source);
    }

    public object DisplayValue => Source;

    private void SetDestination(string newUri)
    {
        IFileSystemInfo newDest;
        if (SourceFile is IFileInfo)
        {
            newDest = Filesystem.FileInfo.New(newUri);
        } else if (SourceFile is IDirectoryInfo)
        {
            newDest = Filesystem.DirectoryInfo.New(newUri);
        }
        else
        {
            throw new UnreachableException();
        }

        Destination = newDest.FullName;
        DestinationFile = newDest;
        Status = FileStatus.Ready;
    }
    
    public void RunRules(RulesModel rulesModel)
    {
        if (Status == FileStatus.Renamed)
        {
            return;
        }

        string newUri = Source;
        foreach (Rule rule in rulesModel.Rules)
        {
            newUri = rule.Run(newUri);
        }

        SetDestination(newUri);
        Debug.WriteLine("New destination: " + Destination);
    }
}
