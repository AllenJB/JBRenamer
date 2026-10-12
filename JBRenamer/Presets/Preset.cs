using System.Collections.Generic;
using System.Xml.Serialization;
using JBRenamer.Filters;
using JBRenamer.Rules.Type;
using Rule = JBRenamer.Rules.Type.Rule;

namespace JBRenamer.Presets;

public class Preset
{
    public string Name;

    public FilterSettings? Filters;

    [XmlArrayItem(Type = typeof(ReplaceRule)),
     XmlArrayItem(Type = typeof(PrefixRule)),
     XmlArrayItem(Type = typeof(RegExpRule))]
    public List<Rule> Rules = new();

    public Preset(string name, List<Rule> rules, FilterSettings? filters)
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
