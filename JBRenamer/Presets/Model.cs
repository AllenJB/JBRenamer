using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using JBRenamer.Filters;
using JBRenamer.Rules.Type;
using Qt.Bridge.Models;

namespace JBRenamer.Presets;

public class Model(Program program) : ListModel<string>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private List<Preset> Presets = new();

    public override int ItemCount()
    {
        return Presets.Count;
    }

    public override string Data(int index)
    {
        return Presets[index].Name;
    }

    public void Add(string name, bool includeFilters)
    {
        Debug.WriteLine("Add new preset: " + name);
        FilterSettings? filters = null;
        if (includeFilters)
        {
            filters = program.Filters.Clone();
        }

        List<Rule> rules = new();
        foreach (Rule rule in program.Rules.Rules)
        {
            rules.Add(rule.Clone());
        }

        Preset preset = new Preset(name, rules, filters);
        Presets.Add(preset);
        PropertyChanged?.Invoke(this, new(nameof(Presets)));
        Debug.WriteLine("Total presets: " + Presets.Count);
    }
}
