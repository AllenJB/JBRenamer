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

    private List<string> Headers { get; } =
    [
        "Original Filename",
        "New Filename",
        "Status",
        "Error Message",
    ];

    private List<File> files = [];

    protected override int Rows => files.Count;

    protected override int Columns => Headers.Count;

    protected override string ColumnHeader(int column) => Headers[column];

    protected override string this[int row, int col]
    {
        get
        {
            if (row < 0 || row >= files.Count)
            {
                return null;
            }

            File file = files[row];
            switch (col)
            {
                case 0:
                    return file.source;

                case 1:
                    return file.destination;
                
                case 2:
                    return file.status.ToString("G");
                
                case 3:
                    return (file.error ?? string.Empty);
            }

            return null;
        }
        set => throw new InvalidOperationException();
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
            files.Add(new File(selectedFile.LocalPath));
        }

        PropertyChanged?.Invoke(this, new(nameof(files)));
        Debug.WriteLine("Total files: " + files.Count);
    }

    public void AddSourceFile(Uri selectedFile)
    {
        Debug.WriteLine("Add file URI: " + selectedFile.LocalPath);
        files.Add(new File(selectedFile.LocalPath));
        PropertyChanged?.Invoke(this, new(nameof(files)));
        Debug.WriteLine("Total files: " + files.Count);
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

        foreach (FileInfo file in srcPath.GetFiles())
        {
            files.Add(new File(file.FullName));
        }
        
        PropertyChanged?.Invoke(this, new(nameof(files)));
        Debug.WriteLine("Total files: " + files.Count);
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
        foreach (File file in files)
        {
            i++;
            file.RunRules(rulesModel);
            DataChanged(i, 1);
        }
        PropertyChanged?.Invoke(this, new(nameof(files)));
        Debug.WriteLine("Running rules complete");
    }

    public void RenameFiles(RulesModel rules)
    {
        Debug.WriteLine("Renaming files");
        var i = -1;
        foreach (File file in files)
        {
            i++;
            if (file.status != FileStatus.Ready)
            {
                continue;
            }

            try
            {
                Filesystem.Move(file.source, file.destination);
                file.status = FileStatus.Renamed;
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine("ERROR " + file.source + " FNF: " + e.Message);
                file.status = FileStatus.Error;
                file.error = "Source file not found";
            }
            catch (IOException e)
            {
                Debug.WriteLine("ERROR " + file.source + " IO: " + e.Message);
                file.status = FileStatus.Error;
                file.error = e.Message;
            }
            DataChanged(i, 1);
            DataChanged(i, 2);
            DataChanged(i, 3);
        }
        PropertyChanged?.Invoke(this, new(nameof(files)));
        Debug.WriteLine("Rename complete");
    }
}
