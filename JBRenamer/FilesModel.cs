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

    [Qt.Ignore] public List<File> Files = [];

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
        Debug.WriteLine("Checking destination changed for " + index);
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
        }
        else if (Filesystem.Directory.Exists(path))
        {
            return Filesystem.DirectoryInfo.New(path);
        }

        throw new InvalidOperationException("Not a valid file or directory path: " + path);
    }

    private void AddFSInfoRecursive(IFileSystemInfo newEntry, FiltersModel fileFilters, bool isRootLevel = false)
    {
        if (! newEntry.Exists)
        {
            return;
        }

        bool isHidden = ((newEntry.Attributes & FileAttributes.Hidden) != 0);
        if (isHidden && (! fileFilters.IncludeHidden))
        {
            return;
        }

        if (newEntry is IFileInfo newFile)
        {
            Files.Add(new File(newFile, Filesystem));
        }
        else if (newEntry is IDirectoryInfo newDir)
        {
            if (! fileFilters.ItemsInFolders)
            {
                if (fileFilters.FoldersAsItems)
                {
                    Files.Add(new File(newDir, Filesystem));
                }

                return;
            }

            try
            {
                foreach (IFileSystemInfo subitem in newDir.GetFileSystemInfos("*",
                             new EnumerationOptions { IgnoreInaccessible = true }))
                {
                    if ((subitem is IDirectoryInfo) && (! fileFilters.IncludeSubfolders))
                    {
                        continue;
                    }

                    AddFSInfoRecursive(subitem, fileFilters);
                }

                if (fileFilters.FoldersAsItems)
                {
                    Debug.WriteLine("isRootLevel = " + (isRootLevel ? "Y" : "N") + " :: SkipRootFolders = " +
                                    (fileFilters.SkipRootFolders ? "Y" : "N" + " :: path: " + newDir.FullName));
                    if (! (isRootLevel && fileFilters.SkipRootFolders))
                    {
                        Files.Add(new File(newDir, Filesystem));
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.WriteLine("Ignored path: " + newDir.FullName + " :: " + e.Message);
            }
        }
    }

    private void AddFileInfos(List<IFileSystemInfo> newFiles, FiltersModel? fileFilters = null)
    {
        int currentLastIndex = (Files.Count - 1);
        BeginInsertRows(new ModelIndex(), currentLastIndex, currentLastIndex + newFiles.Count);
        foreach (IFileSystemInfo newFile in newFiles)
        {
            if (fileFilters == null)
            {
                Files.Add(new File(newFile, Filesystem));
                continue;
            }

            AddFSInfoRecursive(newFile, fileFilters, true);
        }

        EndInsertRows();
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Total files: " + Files.Count);
    }

    public void AddURIs(List<Uri> selectedFiles, FiltersModel? fileFilters = null)
    {
        List<IFileSystemInfo> newFiles = [];
        List<string> invalidURIs = [];
        foreach (Uri selectedFile in selectedFiles)
        {
            try
            {
                Debug.WriteLine("Add file URI: '" + selectedFile.LocalPath + "'");
                // FIXME Display error when file/directory not found
                // (probably fun with special chars similar to trailing spaces)
                newFiles.Add(FSInfoFromPath(selectedFile.LocalPath));
            }
            catch (Exception e)
            {
                invalidURIs.Add(selectedFile.LocalPath);
            }
        }

        if (invalidURIs.Count > 0)
        {
            var errorMsg = "One or more entries could not be added:\n" + String.Join("\n", invalidURIs);
            ShowError?.Invoke(this, new ShowErrorArgs(errorMsg));
        }

        AddFileInfos(newFiles, fileFilters);
    }

    public File? AddSourceFile(Uri selectedFile)
    {
        Debug.WriteLine("Add file URI: " + selectedFile.LocalPath);
        try
        {
            File newFile = new File(FSInfoFromPath(selectedFile.LocalPath), Filesystem);

            int currentLastIndex = (Files.Count - 1);
            BeginInsertRows(new ModelIndex(), currentLastIndex, currentLastIndex + 1);
            Files.Add(newFile);
            EndInsertRows();
            PropertyChanged?.Invoke(this, new(nameof(Files)));
            Debug.WriteLine("Total files: " + Files.Count);

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
        List<IFileSystemInfo> newFiles = [];
        try
        {
            IDirectoryInfo srcPath = Filesystem.DirectoryInfo.New(selectedPath.LocalPath);
            if (! srcPath.Exists)
            {
                ShowError?.Invoke(this, new ShowErrorArgs("Not a valid directory: " + selectedPath.LocalPath));
                return;
            }

            foreach (IFileSystemInfo file in srcPath.GetFileSystemInfos())
            {
                newFiles.Add(file);
            }

            AddFileInfos(newFiles);
        }
        catch (Exception e)
        {
            ShowError?.Invoke(this, new ShowErrorArgs(e.Message));
        }
    }

    public void Drop(string items, FiltersModel fileFilters)
    {
        Debug.WriteLine("Items: " + items);
        // TODO Update to obey file filters

        char[] seperators = ['\n'];
        string[] uriStrings = items.Split(seperators);
        List<Uri> uris = [];
        foreach (string uri in uriStrings)
        {
            if (! uri.StartsWith("file://"))
            {
                // TODO Notify user when drop item is rejected?
                Debug.WriteLine("Ignored URI: " + uri);
                continue;
            }

            string modifiedUri = uri.Replace(" ", "%20");
            Debug.WriteLine("New URI: '" + modifiedUri + "'");
            uris.Add(new Uri(modifiedUri));
        }

        AddURIs(uris, fileFilters);
    }

    /*
    [Qt.Ignore]
    protected List<(int, int)> RowRangesFromSelection(int[] rowsArr)
    {
        List<int> rows = new List<int>(rowsArr);
        // Order by descending so indexes don't get shifted as multiple selections are removed
        rows.OrderDescending();

        List<(int, int)> rowRanges = [];
        int rangeStart = -1;
        int rangeEnd = -1;
        foreach (int row in rows)
        {
            if (rangeEnd == -1)
            {
                rangeStart = row;
                rangeEnd = row;
                continue;
            }

            if (row < rangeEnd - 1)
            {
                rowRanges.Add((rangeStart, rangeEnd));
                rangeStart = row;
                rangeEnd = row;
                continue;
            }
        }

        rowRanges.Add((rangeStart, rangeEnd));

        return rowRanges;
    }

    public void RemoveSelection(int[] rows)
    {
        List<(int, int)> rowRanges = RowRangesFromSelection(rows);

        foreach ((int rangeStart, int rangeEnd) in rowRanges)
        {
            BeginRemoveRows(ModelIndex.Empty, rangeStart, rangeEnd);
            for (int i = rangeEnd; i >= rangeStart; i--)
            {
                Files.RemoveAt(i);
            }

            EndRemoveRows();
        }
    }
    */

    public void RemoveRow(int row)
    {
        BeginRemoveRows(new ModelIndex(), row, row);
        Files.RemoveAt(row);
        EndRemoveRows();
        RowChanged(row);
        PropertyChanged?.Invoke(this, new(nameof(Files)));
        Debug.WriteLine("Total files: " + Files.Count);
    }

    private void RowChanged(int row)
    {
        for (int i = 0; i < Headers.Count; i++)
        {
            DataChanged(row, i);
        }
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

            if ((! hasConflict) && (fileA.Status == FileStatus.Conflict))
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
