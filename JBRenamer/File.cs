using System.Diagnostics;
using Qt.Bridge.Models;

namespace JBRenamer;

public class File : IDisplayable
{
    public Uri source { get; init; }

    public Uri destination { get; private set; }

    public File(Uri source)
    {
        this.source = source;
        destination = new Uri(source.AbsolutePath);
    }

    public object DisplayValue => source.AbsolutePath;
    
    public void RunRules(RulesModel rulesModel)
    {
        Uri newUri = new Uri(source.AbsolutePath);
        foreach (Rule rule in rulesModel.rules)
        {
            newUri = rule.Run(newUri);
        }

        destination = newUri;
        Debug.WriteLine("New destination: " + destination);
    }
}