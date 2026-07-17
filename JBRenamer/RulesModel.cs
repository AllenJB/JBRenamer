using System.Diagnostics;
using JBRenamer.Rules;
using Qt.Bridge.Models;

namespace JBRenamer;

public class RulesModel :TableModel<string>
{
    private List<string> Headers { get; } =
    [
        "Order",
        "Type",
        "Description",
    ];

    private List<Rule> rules = [
        new ReplaceRule("TestSearch", "TestReplace")
    ];

    protected override int Rows => rules.Count;

    protected override int Columns => Headers.Count;

    protected override string ColumnHeader(int column) => Headers[column];

    protected override string this[int row, int col]
    {
        get
        {
            if (row < 0 || row >= rules.Count)
            {
                return null;
            }

            Rule rule = rules[row];
            switch (col)
            {
                case 0:
                    return "" + row;
                
                case 1:
                    return rule.RuleType;
                
                case 2:
                    return rule.Describe();
            }

            return null;
        }
        set => throw new InvalidOperationException();
    }

    public void Add(Rule rule)
    {
        rules.Add(rule);
        Debug.WriteLine("Total rules: " + rules.Count);
    }

    public void AddReplaceRule(string find, string replace)
    {
        Add(new ReplaceRule(find, replace));
    }
}