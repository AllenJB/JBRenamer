using Qt.Bridge.Models;

namespace JBRenamer;

public class File : IDisplayable
{
    public Uri source;

    public File(Uri source)
    {
        this.source = source;
    }

    public object DisplayValue => source.AbsolutePath;
}