using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Xml.Serialization;
using JBRenamer.Filters;
using JBRenamer.Presets;
using JBRenamer.Rules.Type;
using Qt.Quick;
using Testably.Abstractions;
using FilesModel = JBRenamer.Files.Model;
using RulesModel = JBRenamer.Rules.Model;

namespace JBRenamer;

public class Program
{
    private IFileSystem Filesystem = new RealFileSystem();

    private string? ConfigDir = null;

    public RulesModel Rules = new();

    public FilesModel Files = new();

    public FilterSettings Filters = new();

    public ObservableCollection<Preset> Presets = new();

    public event EventHandler<ShowErrorArgs> ShowError;

    internal static void Main(string[] args)
    {
        Qt.Application.Name = "JBRenamer";
        Qt.Application.DisplayName = "JBRenamer";
        Qt.Application.OrganizationName = "AllenJB";
        Qt.Application.OrganizationDomain = "allenjb.uk";
        
        Qml.LoadFromRootModule("Main");
        Qml.WaitForExit();
    }

    public Program()
    {
        SetupConfigDir();
        Files.ShowError += ProxyShowError;
        PresetsRestore();
    }

    private void SetupConfigDir()
    {
        string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            Path.DirectorySeparatorChar + "jbrenamer" + Path.DirectorySeparatorChar;

        if (Filesystem.Directory.Exists(configPath))
        {
            ConfigDir = configPath;
            return;
        }

        try
        {
            Filesystem.Directory.CreateDirectory(configPath);
        }
        catch
        {
            Debug.WriteLine("Config directory could not be created.");
            return;
        }

        ConfigDir = configPath;
    }

    private void PresetSave(Preset preset)
    {
        if (ConfigDir == null)
        {
            return;
        }

        string presetsPath = ConfigDir + "presets" + Path.DirectorySeparatorChar;
        if (! Filesystem.Directory.Exists(presetsPath))
        {
            Filesystem.Directory.CreateDirectory(presetsPath);
        }

        string filename = preset.Name;
        FileSystemStream file = Filesystem.FileStream.New(presetsPath + filename, FileMode.Create);
        XmlSerializer serializer = new(typeof(Preset));
        serializer.Serialize(file, preset);
        file.Close();
    }

    private void PresetsRestore()
    {
        if (ConfigDir == null)
        {
            Debug.WriteLine("PresetsRestore: ConfigDir is null");
            return;
        }

        string presetsPath = ConfigDir + "presets" + Path.DirectorySeparatorChar;
        if (! Filesystem.Directory.Exists(presetsPath))
        {
            Debug.WriteLine("PresetsRestore: Path does not exist: " + presetsPath);
            return;
        }

        XmlSerializer serializer = new(typeof(Preset));
        foreach (string filePath in Filesystem.Directory.EnumerateFiles(presetsPath))
        {
            FileSystemStream file = Filesystem.FileStream.New(filePath, FileMode.Open);
            Preset? preset = (Preset?)serializer.Deserialize(file);
            if (preset != null)
            {
                Presets.Add(preset);
                Debug.WriteLine("Preset loaded: " + preset.Name);
            }
        }
    }

    public void PresetAdd(string name, bool includeFilters)
    {
        Debug.WriteLine("Add new preset: " + name);
        FilterSettings? presetFilters = null;
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

        PresetSave(preset);
    }

    public string PresetValidateName(string name)
    {
        var disallowed = new[] { Path.DirectorySeparatorChar, '*', '.', '?' };
        if (disallowed.Any(c => name.Contains(c)))
        {
            return "Preset name cannot contain the following characters: " + String.Join(" ", disallowed);
        }
        
        foreach (Preset preset in Presets)
        {
            if (preset.Name == name)
            {
                return "Another preset with the same name already exists";
            }
        }

        return "";
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
