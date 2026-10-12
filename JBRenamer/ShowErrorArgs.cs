using System;

namespace JBRenamer;

public class ShowErrorArgs : EventArgs
{
    public string Message { get; set; }

    public ShowErrorArgs(string message)
    {
        this.Message = message;
    }
}
