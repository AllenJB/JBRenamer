using System.Diagnostics;

namespace JBRenamer;

public class DebugModel
{
    public void Log(string msg)
    {
        Debug.WriteLine(msg);
    }
}