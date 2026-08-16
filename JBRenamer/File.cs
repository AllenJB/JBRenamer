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
    [Qt.Ignore]
    private readonly IFileSystem Filesystem;

    public string Source { get; init; }

    [Qt.Ignore]
    public IFileSystemInfo SourceFile { get; private set; }

    public string Destination { get; private set; }
    
    [Qt.Ignore]
    public IFileSystemInfo DestinationFile { get; private set; }

    public FileStatus Status = FileStatus.Ready;

    public string? Error = null;

    [Qt.Ignore]
    public File(IFileSystemInfo sourceInfo, IFileSystem fs)
    {
        Filesystem = fs;
        Source = sourceInfo.FullName;
        Destination = sourceInfo.FullName;
        SourceFile = sourceInfo;
        DestinationFile = sourceInfo;
    }

    public object DisplayValue => Source;
    
    public void RunRules(RulesModel rulesModel)
    {
        string newUri = Source;
        foreach (Rule rule in rulesModel.Rules)
        {
            newUri = rule.Run(newUri);
        }

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
        Debug.WriteLine("New destination: " + Destination);
    }
}
