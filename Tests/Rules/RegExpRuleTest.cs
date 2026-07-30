using JBRenamer.Rules;

namespace Tests.Rules;

public class RegExpRuleTest
{
    [Fact]
    public void SimpleReplace()
    {
        var underTest = new RegExpRule("\\.t", "b");
        var testPath = (OperatingSystem.IsWindows() ? "C:/tmp/" : "/tmp/");

        var ios = new List<(string, string)>
        {
            ("Test.txt", "Test.txt"),
            ("test.txt", "test.txt"),
            ("test.test.txt", "testbest.txt"),
        };

        foreach ((string input, string expectedOutput) in ios)
        {
            var actualOutput = underTest.Run(testPath + input);
            Assert.Equal(testPath + expectedOutput, actualOutput);
        }
    }
}
