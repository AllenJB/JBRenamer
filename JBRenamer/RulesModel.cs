using System.Diagnostics;
using JBRenamer.Rules;
using Qt.Bridge.Models;

namespace JBRenamer;

public class RulesModel : TableModel<string>
{
    private List<string> Headers { get; } =
    [
        "Order",
        "Type",
        "Description",
    ];

    public List<Rule> Rules { get; } = [];

    protected override int Rows => Rules.Count;

    protected override int Columns => Headers.Count;

    protected override string ColumnHeader(int column) => Headers[column];

    protected override string this[int row, int col]
    {
        get
        {
            Rule rule = Rules[row];
            return col switch
            {
                0 => "" + row,
                1 => rule.RuleType,
                2 => rule.Describe(),
                _ => throw new InvalidOperationException(),
            };
        }
        set => throw new InvalidOperationException();
    }

    public void Add(Rule rule)
    {
        Rules.Add(rule);
        Debug.WriteLine("Total rules: " + Rules.Count);
    }

    public void AddReplaceRule(string find, string replace)
    {
        Add(new ReplaceRule(find, replace));
    }

    public void AddRegExpRule(string findPattern, string replace)
    {
        Add(new RegExpRule(findPattern, replace));
    }
}
