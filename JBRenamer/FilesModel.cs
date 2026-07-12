using System.ComponentModel;
using Qt.Bridge.Models;
using Qt.Quick;

namespace JBRenamer;

public class FilesModel : TableModel<File>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private List<string> Headers { get; } =
    [
        "Original Filename",
        "New Filename",
    ];

    private List<File> files = [];

    protected override int Rows => files.Count;

    protected override int Columns => Headers.Count;

    protected override string ColumnHeader(int column) => Headers[column];

    protected override File this[int row, int col]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void AddSourceFiles(List<Uri> selectedFiles)
    {
        foreach(Uri selectedFile in selectedFiles)
        {
            files.Add(new File(selectedFile));
        }
    }

    public void AddSourceFile(Uri selectedFile)
    {
        files.Add(new File(selectedFile));
    }
}