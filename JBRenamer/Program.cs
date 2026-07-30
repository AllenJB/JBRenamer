using System.Diagnostics;
using Qt.Quick;

namespace JBRenamer;

public class Program
{
    internal static void Main(string[] args)
    {
        Qml.LoadFromRootModule("Main");
        Qml.WaitForExit();
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
