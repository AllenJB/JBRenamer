using JBRenamer.Rules;

namespace Tests.Rules;

public class PrefixRuleTest
{
    [Fact]
    public void SimpleValue()
    {
        PrefixRule underTest = new PrefixRule("AbC");
        var testPath = (OperatingSystem.IsWindows() ? "C:/tmp/" : "/tmp/");

        var ios = new List<(string, string)>
        {
            ("Test.txt", "AbCTest.txt"),
            ("test.txt", "AbCtest.txt"),
            ("test.test.txt", "AbCtest.test.txt"),
        };

        foreach ((string input, string expectedOutput) in ios)
        {
            var actualOutput = underTest.Run(testPath + input);
            Assert.Equal(testPath + expectedOutput, actualOutput);
        }
    }

    [Fact]
    public void TestMetadataParentFolder()
    {
        PrefixRule underTest = new PrefixRule(":File_FolderName: ");
        var testPath = (OperatingSystem.IsWindows() ? "C:/tmp/" : "/tmp/");

        var ios = new List<(string, string)>
        {
            ("Test.txt", "tmp Test.txt"),
            ("test.txt", "tmp test.txt"),
            ("test.test.txt", "tmp test.test.txt"),
        };

        foreach ((string input, string expectedOutput) in ios)
        {
            var actualOutput = underTest.Run(testPath + input);
            Assert.Equal(testPath + expectedOutput, actualOutput);
        }
    }
}
