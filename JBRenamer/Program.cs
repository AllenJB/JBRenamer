using System.Diagnostics;
using Qt.Quick;

namespace JBRenamer;

public class Program
{
    public RulesModel Rules = new();

    public readonly FilesModel Files = new();

    public FiltersModel Filters = new();
    
    public event EventHandler<ShowErrorArgs> ShowError;

    internal static void Main(string[] args)
    {
        Qml.LoadFromRootModule("Main");
        Qml.WaitForExit();
    }

    public Program()
    {
        Files.ShowError += ProxyShowError;
    }

    private void ProxyShowError(object? sender, ShowErrorArgs e)
    {
        ShowError?.Invoke(sender, e);
    }
    
    public void OpenLink(string url)
    {
        if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", url);
        }
        else
        {
            Process.Start(url);
        }
    }
}
