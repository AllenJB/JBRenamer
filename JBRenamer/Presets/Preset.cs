namespace JBRenamer.Presets;

public class Preset(string name, List<Rule> rules, FiltersModel? filters)
{
    public string Name = name;
    
    public FiltersModel? Filters = filters;

    public List<Rule> Rules = rules;
}
