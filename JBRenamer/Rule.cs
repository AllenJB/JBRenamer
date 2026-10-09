namespace JBRenamer;

public abstract class Rule
{
    public string RuleType { get; init; }
    
    public abstract string Run(string sourceUri);

    public abstract string Describe();

    protected Rule(string ruleType)
    {
        RuleType = ruleType;
    }

    internal Rule()
    {
       RuleType = "Deserialized";
    }

    public Rule Clone()
    {
        return (Rule) MemberwiseClone();
    }
}