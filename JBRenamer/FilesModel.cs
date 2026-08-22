using System.ComponentModel;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Web;
using Qt.Bridge.Models;
using Qt.DotNet;
using Testably.Abstractions;

[assembly: Qt.IgnoreType(typeof(IFileSystem))]
[assembly: Qt.IgnoreType(typeof(IFileSystemInfo))]

namespace JBRenamer;

public class ShowErrorArgs : EventArgs
{
    public string Message { get; set; }

    public ShowErrorArgs(string message)
    {
        this.Message = message;
    }
}

public class FilesModel : TableModel<string>, INotifyPropertyChanged
{
    private IFileSystem Filesystem = new RealFileSystem();
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<ShowErrorArgs> ShowError;

    public List<string> Headers { get; } =
    [
        "Original Full Path",
        "Original Name",
        "New Name",
        "New Full Path",
        "Status",
        "Error Message",
    ];

    private List<File> Files = [];

    protected override int Rows => Files.Count;

    protected override int Columns => Headers.Count;

    protected override string ColumnHeader(int column) => Headers[column];

    public string ColumnName(int column) => Headers[column];

    public int ColumnIndex(string columnName) => Headers.IndexOf(columnName);

    public void SetFilesystem(IFileSystem fsImpl)
    {
        Filesystem = fsImpl;
    }

    protected override string this[int row, int col]
    {
        get
        {
            File file = Files[row];
            string columnName = (Headers[col] ?? "");
            return columnName switch
            {
                "Original Full Path" => file.Source,
                "Original Name" => file.SourceFile.Name,
                "New Name" => file.DestinationFile.Name,
                "New Full Path" => file.Destination,
                "Status" => file.Status.ToString("G"),
                "Error Message" => (file.Error ?? string.Empty),
                _ => throw new InvalidOperationException(),
            };
        }
        set => throw new InvalidOperationException();
    }

    public int Count()
    {
        return Files.Count;
    }

    public bool DestinationChanged(int index)
    {
        Debug.WriteLine("Checking destination changed for "+ index);
        return (Files[index].Destination != Files[index].Source);
    }

    public bool DestinationConflicts(int index)
    {
        return (Files[index].Status == FileStatus.Conflict);
    }

    protected string DecodeUri(string original)
    {
        string protocol = "file://";
        string uri = HttpUtility.UrlDecode(original);
        if (uri.StartsWith(protocol))
        {
            uri = uri.Substring(protocol.Length);
        }

        return uri;
    }

    private IFileSystemInfo FSInfoFromPath(string path)
    {
        if (Filesystem.File.Exists(path))
        {
            return Filesystem.FileInfo.New(path);
        } else if (Filesystem.Directory.Exists(path))
        {
            return Filesystem.DirectoryInfo.New(path);
        }

        throw new InvalidOperationException("Not a valid file or directory path: " + path);
    }

    private void AddFiles(List<File> newFiles)
    {
        int currentLastIndex = (Files.Count - 1);
        BeginInsertRows(new ModelIndex(), currentLastIndex, currentLastIndex + newFiles.Count );
        foreach (File newFile in newFiles)
        {
            Files.Add(newFile);
        }
        EndInsertRows();
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Total files: " + Files.Count);
    }

    public void AddSourceFiles(List<Uri> selectedFiles)
    {
        List<File> newFiles = [];
        try
        {
            foreach (Uri selectedFile in selectedFiles)
            {
                Debug.WriteLine("Add file URI: '" + selectedFile.LocalPath + "'");
                // FIXME Display error when file/directory not found
                // (probably fun with special chars similar to trailing spaces)
                newFiles.Add(new File(FSInfoFromPath(selectedFile.LocalPath), Filesystem));
            }
        }
        catch (Exception e)
        {
            ShowError?.Invoke(this, new ShowErrorArgs(e.Message));
        }

        AddFiles(newFiles);
    }

    public File? AddSourceFile(Uri selectedFile)
    {
        Debug.WriteLine("Add file URI: " + selectedFile.LocalPath);
        try
        {
            File newFile = new File(FSInfoFromPath(selectedFile.LocalPath), Filesystem);
            AddFiles([newFile]);
            return newFile;
        }
        catch (InvalidOperationException e)
        {
            ShowError?.Invoke(this, new ShowErrorArgs(e.Message));
        }

        return null;
    }

    public void AddSourceDirectory(Uri selectedPath)
    {
        Debug.WriteLine("Add URI: " + selectedPath.LocalPath);
        List<File> newFiles = [];
        try
        {
            IDirectoryInfo srcPath = Filesystem.DirectoryInfo.New(selectedPath.LocalPath);
            if (!srcPath.Exists)
            {
                ShowError?.Invoke(this, new ShowErrorArgs("Not a valid directory: " + selectedPath.LocalPath));
                return;
            }

            foreach (IFileSystemInfo file in srcPath.GetFileSystemInfos())
            {
                newFiles.Add(new File(file, Filesystem));
            }

            AddFiles(newFiles);
        }
        catch (Exception e)
        {
            ShowError?.Invoke(this, new ShowErrorArgs(e.Message));
        }
    }

    public void Drop(string source, string items)
    {
        Debug.WriteLine("Drop source: " + source);
        Debug.WriteLine("Items: " + items);

        char[] seperators = ['\n'];
        string[] uriStrings = items.Split(seperators);
        List<Uri> uris = [];
        foreach (string uri in uriStrings)
        {
            if (!uri.StartsWith("file://"))
            {
                // TODO Notify user when drop item is rejected?
                Debug.WriteLine("Ignored URI: " + uri);
                continue;
            }

            string modifiedUri = uri.Replace(" ", "%20");
            Debug.WriteLine("New URI: '" + modifiedUri + "'");
            uris.Add(new Uri(modifiedUri));
        }

        AddSourceFiles(uris);
    }

    public void RunRules(RulesModel rulesModel)
    {
        Debug.WriteLine("Running rules");
        var i = -1;
        foreach (File file in Files)
        {
            i++;
            file.RunRules(rulesModel);

            DataChanged(i, Headers.IndexOf("New Name"));
            DataChanged(i, Headers.IndexOf("New Full Path"));
            DataChanged(i, Headers.IndexOf("Status"));
            DataChanged(i, Headers.IndexOf("Error Message"));
        }
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Running rules complete");
        
        RunConflictCheck();
    }
    
    private void RunConflictCheck()
    {
        var indexA = -1;
        foreach (File fileA in Files)
        {
            indexA++;
            
            bool hasConflict = false;
            var indexB = -1;
            foreach (File fileB in Files)
            {
                indexB++;
                
                if ((fileA != fileB) && (fileA.Destination == fileB.Destination))
                {
                    hasConflict = true;
                    fileA.Status = FileStatus.Conflict;
                    fileA.Error = "Destination conflicts with another file";
                    Debug.WriteLine($"{fileA.Source} ({indexA}) conflicts with {fileB.Source} ({indexB})");

                    DataChanged(indexA, Headers.IndexOf("Status"));
                    DataChanged(indexA, Headers.IndexOf("Error Message"));
                }
            }

            if ((!hasConflict) && (fileA.Status == FileStatus.Conflict))
            {
                Debug.WriteLine($"{fileA.Source} ({indexA}) has no conflicts");
                fileA.Status = FileStatus.Ready;
                fileA.Error = null;

                DataChanged(indexA, Headers.IndexOf("Status"));
                DataChanged(indexA, Headers.IndexOf("Error Message"));
            }
        }
    }

    public void RenameFiles(RulesModel rules)
    {
        Debug.WriteLine("Renaming files");
        var i = -1;
        foreach (File file in Files)
        {
            i++;
            if (file.Status != FileStatus.Ready)
            {
                continue;
            }

            try
            {
                if (Filesystem.File.Exists(file.Source))
                {
                    Filesystem.File.Move(file.Source, file.Destination);
                }
                else if (Filesystem.Directory.Exists(file.Source))
                {
                    Filesystem.Directory.Move(file.Source, file.Destination);
                }

                file.Status = FileStatus.Renamed;
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine("ERROR " + file.Source + " FNF: " + e.Message);
                file.Status = FileStatus.Error;
                file.Error = "Source file not found";
            }
            catch (IOException e)
            {
                Debug.WriteLine("ERROR " + file.Source + " IO: " + e.Message);
                file.Status = FileStatus.Error;
                file.Error = e.Message;
            }

            DataChanged(i, Headers.IndexOf("New Name"));
            DataChanged(i, Headers.IndexOf("New Full Path"));
            DataChanged(i, Headers.IndexOf("Status"));
            DataChanged(i, Headers.IndexOf("Error Message"));
        }
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Rename complete");
    }

    public void Clear()
    {
        Files.Clear();
        PropertyChanged?.Invoke(this, new(nameof(Files)));
    }
}
