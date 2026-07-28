namespace JBRenamer;

public abstract class Rule
{
    public string RuleType { get; private set; }
    
    public abstract string Run(string sourceUri);

    public abstract string Describe();

    protected Rule(string ruleType)
    {
        RuleType = ruleType;
    }
}