using System.Collections.ObjectModel;
using System.Diagnostics;
using JBRenamer.Presets;
using Qt.Quick;

namespace JBRenamer;

public class Program
{
    public RulesModel Rules = new();

    public FilesModel Files = new();

    public FiltersModel Filters = new();

    public ObservableCollection<Preset> Presets = new();

    public event EventHandler<ShowErrorArgs> ShowError;

    internal static void Main(string[] args)
    {
        Qml.LoadFromRootModule("Main");
        Qml.WaitForExit();
    }

    public Program()
    {
        Files.ShowError += ProxyShowError;
    }

    public void PresetAdd(string name, bool includeFilters)
    {
        Debug.WriteLine("Add new preset: " + name);
        FiltersModel? presetFilters = null;
        if (includeFilters)
        {
            presetFilters = Filters.Clone();
        }

        List<Rule> presetRules = new();
        foreach (Rule rule in Rules.Rules)
        {
            presetRules.Add(rule.Clone());
        }

        Preset preset = new Preset(name, presetRules, presetFilters);
        Presets.Add(preset);
        Debug.WriteLine("Total presets: " + Presets.Count);
    }

    public void PresetLoad(Preset preset)
    {
        Rules.Load(preset.Rules);

        if (preset.Filters != null)
        {
            Debug.WriteLine("Loading preset filters");
            Filters.Load(preset.Filters);
        }

        Debug.WriteLine("Loaded preset: " + preset.Name);
    }

    private void ProxyShowError(object? sender, ShowErrorArgs e)
    {
        ShowError?.Invoke(sender, e);
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
