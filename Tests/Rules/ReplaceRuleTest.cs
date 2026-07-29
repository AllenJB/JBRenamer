using JBRenamer.Rules;

namespace Tests.Rules;

public class ReplaceRuleTest
{
    [Fact]
    public void SimpleReplace()
    {
        var underTest = new ReplaceRule("a", "b");
        var testPath = (OperatingSystem.IsWindows() ? "C:/tmp/" : "/tmp/");

        var ios = new List<(string, string)>
        {
            ("Test.txt", "Test.txt"),
            ("Tast.txt", "Tbst.txt"),
            ("A.txt", "A.txt"),
            ("a.txt", "b.txt"),
            ("AA.txt", "AA.txt"),
            ("aa.txt", "bb.txt"),
            ("test.a", "test.b"),
        };

        foreach ((string input, string expectedOutput) in ios)
        {
            var actualOutput = underTest.Run(testPath + input);
            Assert.Equal(testPath + expectedOutput, actualOutput);
        }
    }
}
