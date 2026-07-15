namespace JBRenamer;

public abstract class Rule
{
    public string RuleType { get; private set; }
    
    public abstract Uri Run(Uri sourceUri);

    public abstract string Describe();

    protected Rule(string ruleType)
    {
        RuleType = ruleType;
    }
}