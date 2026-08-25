using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JBRenamer;

public class FiltersModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public bool ItemsInFolders = false;

    public bool FoldersAsItems = true;

    public bool IncludeSubfolders = false;

    public bool IncludeHidden = false;

    public bool SkipRootFolders = true;

    public void SetItemsInFolders(bool value)
    {
        ItemsInFolders = value;
        NotifyPropertyChanged();
    }

    public void SetFoldersAsItems(bool value)
    {
        FoldersAsItems = value;
        NotifyPropertyChanged();
    }

    public void SetIncludeSubfolders(bool value)
    {
        IncludeSubfolders = value;
        NotifyPropertyChanged();
    }

    public void SetIncludeHidden(bool value)
    {
        IncludeHidden = value;
        NotifyPropertyChanged();
    }

    public void SetSkipRootFolders(bool value)
    {
        SkipRootFolders = value;
        NotifyPropertyChanged();
    }
}
