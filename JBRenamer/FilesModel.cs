using System.ComponentModel;
using System.Diagnostics;
using System.Web;
using Qt.Bridge.Models;
using FileStatus = JBRenamer.FileStatus;
using Filesystem = System.IO.File; 

namespace JBRenamer;

public class FilesModel : TableModel<string>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

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

    public void AddSourceFiles(List<Uri> selectedFiles)
    {
        foreach (Uri selectedFile in selectedFiles)
        {
            Debug.WriteLine("Add file URI: " + selectedFile.LocalPath);
            Files.Add(new File(selectedFile.LocalPath));
        }

        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Total files: " + Files.Count);
    }

    public File AddSourceFile(Uri selectedFile)
    {
        Debug.WriteLine("Add file URI: " + selectedFile.LocalPath);
        File file = new File(selectedFile.LocalPath);
        Files.Add(file);
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Total files: " + Files.Count);
        return file;
    }

    public void AddSourceDirectory(Uri selectedPath)
    {
        Debug.WriteLine("Add URI: " + selectedPath.LocalPath);
        DirectoryInfo srcPath = new DirectoryInfo(selectedPath.LocalPath);
        if (! srcPath.Exists)
        {
            Debug.WriteLine("Selected URI is not a directory");
            return;
        }

        foreach (FileSystemInfo file in srcPath.GetFileSystemInfos())
        {
            Files.Add(new File(file.FullName));
        }
        
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Total files: " + Files.Count);
    }

    public void Drop(string source, string items)
    {
        Debug.WriteLine("Drop source: " + source);
        Debug.WriteLine("Items: " + items);

        string[] uriStrings = items.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        List<Uri> uris = [];
        foreach (string uri in uriStrings)
        {
            if (!uri.StartsWith("file://"))
            {
                // TODO Notify user when drop item is rejected?
                Debug.WriteLine("Ignored URI: " + uri);
                continue;
            }

            uris.Add(new Uri(uri));
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
                Filesystem.Move(file.Source, file.Destination);
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
