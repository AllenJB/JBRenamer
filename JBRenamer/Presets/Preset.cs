using System.Xml.Serialization;
using JBRenamer.Rules;

namespace JBRenamer.Presets;

public class Preset
{
    public string Name;

    public FiltersModel? Filters;

    [XmlArrayItem(Type = typeof(ReplaceRule)),
     XmlArrayItem(Type = typeof(PrefixRule)),
     XmlArrayItem(Type = typeof(RegExpRule))]
    public List<Rule> Rules = new();

    public Preset(string name, List<Rule> rules, FiltersModel? filters)
    {
        Name = name;
        Filters = filters;
        Rules = rules;
    }

    internal Preset()
    {
        Name = "Deserialized";
    }
}
