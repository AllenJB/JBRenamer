using System.ComponentModel;
using System.Diagnostics;
using Qt.Bridge.Models;

namespace JBRenamer;

public class DebugModel
{
    public void Log(string msg)
    {
        Debug.WriteLine(msg);
    }
}