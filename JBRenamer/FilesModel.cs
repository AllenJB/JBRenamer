using System.ComponentModel;
using System.Diagnostics;
using Qt.Bridge.Models;

namespace JBRenamer;

public class FilesModel : TableModel<string>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private List<string> Headers { get; } =
    [
        "Original Filename",
        "New Filename",
    ];

    private List<File> files =
    [
        new File(new Uri("/etc/passwd"))
    ];

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
                    return file.source.AbsolutePath;

                case 1:
                    return file.destination.AbsolutePath;
            }

            return null;
        }
        set => throw new InvalidOperationException();
    }

    public void AddSourceFiles(List<Uri> selectedFiles)
    {
        foreach (Uri selectedFile in selectedFiles)
        {
            Debug.WriteLine("Add file URI: " + selectedFile.AbsolutePath);
            files.Add(new File(selectedFile));
        }

        PropertyChanged?.Invoke(this, new(nameof(files)));
        Debug.WriteLine("Total files: " + files.Count);
    }

    public void AddSourceFile(Uri selectedFile)
    {
        Debug.WriteLine("Add file URI: " + selectedFile.AbsolutePath);
        files.Add(new File(selectedFile));
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
}