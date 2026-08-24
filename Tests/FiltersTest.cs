using JBRenamer;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Initializer;
using Xunit.Abstractions;
using Xunit.Sdk;
using File = JBRenamer.File;

namespace Tests;

public class DefaultUnixFileModeStrategy : IUnixFileModeStrategy
{
    private string CurrentUser = "";
    private string CurrentGroup = "";

    public DefaultUnixFileModeStrategy SimulateUser(string user)
    {
        CurrentUser = user;
        return this;
    }

    public DefaultUnixFileModeStrategy SimulateGroup(string group)
    {
        CurrentGroup = group;
        return this;
    }

    public void OnSetUnixFileMode(string fullPath,
        IFileSystemExtensibility extensibility, UnixFileMode mode)
    {
        // Record the simulated owner at the moment the file's mode was set
        extensibility.StoreMetadata(nameof(DefaultUnixFileModeStrategy),
            new UserGroup(CurrentUser, CurrentGroup));
    }

    public bool IsAccessGranted(string fullPath,
        IFileSystemExtensibility extensibility,
        UnixFileMode mode, FileAccess requestedAccess)
    {
        UserGroup owner = extensibility
                              .RetrieveMetadata<UserGroup>(nameof(DefaultUnixFileModeStrategy))
                          ?? new UserGroup(CurrentUser, CurrentGroup);

        bool readOk =
            mode.HasFlag(UnixFileMode.OtherRead) ||
            (mode.HasFlag(UnixFileMode.GroupRead) && owner.Group == CurrentGroup) ||
            (mode.HasFlag(UnixFileMode.UserRead) && owner.User == CurrentUser);

        bool writeOk =
            mode.HasFlag(UnixFileMode.OtherWrite) ||
            (mode.HasFlag(UnixFileMode.GroupWrite) && owner.Group == CurrentGroup) ||
            (mode.HasFlag(UnixFileMode.UserWrite) && owner.User == CurrentUser);

        return requestedAccess switch
        {
            FileAccess.Read => readOk,
            FileAccess.Write => writeOk,
            _ => readOk && writeOk,
        };
    }

    private record UserGroup(string User, string Group);
}

public class FiltersTest
{
    private readonly ITestOutputHelper Output;

    public FiltersTest(ITestOutputHelper output)
    {
        this.Output = output;
    }

    private MockFileSystem CreateFileSystem(string basePath)
    {
        DefaultUnixFileModeStrategy strategy = new();
        strategy.SimulateUser("user1").SimulateGroup("group1");
        MockFileSystem fs = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux))
            .WithUnixFileModeStrategy(strategy);
        fs.InitializeIn(basePath).With(
            new FileDescription("normal file.txt"),
            new DirectoryDescription("normal directory no files"),
            new FileDescription(".hidden file.txt"),
            new DirectoryDescription(".hidden directory no files"),
            new DirectoryDescription(
                "normal directory",
                new DirectoryDescription("subdirectory", new FileDescription("normal file.txt")),
                new FileDescription("subdirectory file.txt")
            )
        );

        // Note: On linux:
        // Ability to rename a file depends on the parent directory write permission (plus parent access, obviously)
        //   - You can rename a file even if it's owned by another user AND you don't have read or write permission (eg. 600)
        // Ability to rename a directory also requires execute/access permission for the directory itself (as well as the parent)
        strategy.SimulateUser("user2").SimulateGroup("group2");
        fs.File.WriteAllText(basePath + "/inaccessible file.txt", "test");
#pragma warning disable CA1416
        fs.File.SetUnixFileMode(basePath + "/inaccessible file.txt", UnixFileMode.UserRead);
#pragma warning restore CA1416
        fs.Directory.CreateDirectory(basePath + "/inaccessible directory", UnixFileMode.UserRead);

        strategy.SimulateUser("user1").SimulateGroup("group1");
        return fs;
    }
    
    private string DropListForTopLevel(string basePath)
    {
        string[] topEntries =
        [
            "inaccessible directory",
            ".hidden directory no files",
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory",
            "normal directory no files",
        ];

        List<string> uris = [];
        foreach (string entry in topEntries)
        {
            uris.Add("file://" + basePath + "/" + entry);
        }

        return String.Join("\n", uris);
    }

    private void AssertFileList(string[] expectedContains, FilesModel files, string basePath)
    {
        List<string> uris = [];
        foreach (File file in files.Files)
        {
            uris.Add(file.Source.Replace(basePath + "/", ""));
        }

        try
        {
            foreach (string expected in expectedContains)
            {
                Assert.Contains(expected, uris);
            }
            Assert.Equal(expectedContains.Length, files.Count());
        }
        catch (XunitException)
        {
            Output.WriteLine(string.Join("\n", uris));
            throw;
        }
    }

    [Fact]
    public void TestBasePathWithoutNoHidden()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = false,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.Drop("file://" + basePath, fileFilters);

        string[] expectedContains =
        [
            "normal file.txt",
            "normal directory no files",
            "normal directory",
            "normal directory/subdirectory",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
            "inaccessible file.txt",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestBasePathWithoutItemsInFolders()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = false,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.Drop("file://" + basePath, fileFilters);

        AssertFileList([basePath], files, basePath);
    }
    
    [Fact]
    public void TestBasePathWithoutFoldersAsItems()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = false,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.Drop("file://" + basePath, fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
            "inaccessible file.txt",
        ];
        AssertFileList(expectedContains, files, basePath);
    }
    
    [Fact]
    public void TestBasePathWithoutIncludeSubfolders()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = false,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.Drop("file://" + basePath, fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestBasePathWithoutSkipRootFolders()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = false
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.Drop("file://" + basePath, fileFilters);

        string[] expectedContains =
        [
            basePath,
            ".hidden directory no files",
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
            "normal directory/subdirectory",
            "normal directory",
            "normal directory no files",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact] public void TestBasePathWithAllEnabled()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);
        files.Drop("file://" + basePath, fileFilters);

        string[] expectedContains =
        [
            ".hidden directory no files",
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
            "normal directory/subdirectory",
            "normal directory",
            "normal directory no files",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestTopLevelWithAllEnabled()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);

        files.Drop(DropListForTopLevel(basePath), fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
            "normal directory/subdirectory",
        ];
        AssertFileList(expectedContains, files, basePath);
    }
    
    [Fact]
    public void TestTopLevelWithoutItemsInFolders()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = false,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);

        files.Drop(DropListForTopLevel(basePath), fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "inaccessible directory",
            ".hidden directory no files",
            "normal directory",
            "normal directory no files",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestTopLevelWithoutHidden()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = false,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);

        files.Drop(DropListForTopLevel(basePath), fileFilters);

        string[] expectedContains =
        [
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestTopLevelWithoutFoldersAsItems()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = false,
            IncludeSubfolders = true,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);

        files.Drop(DropListForTopLevel(basePath), fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestTopLevelWithoutIncludeSubfolders()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = false,
            SkipRootFolders = true
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);

        files.Drop(DropListForTopLevel(basePath), fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory file.txt",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

    [Fact]
    public void TestTopLevelWithoutSkipRootFolders()
    {
        string basePath = "/path/to/test";
        MockFileSystem fs = CreateFileSystem(basePath);

        FiltersModel fileFilters = new FiltersModel
        {
            ItemsInFolders = true,
            IncludeHidden = true,
            FoldersAsItems = true,
            IncludeSubfolders = true,
            SkipRootFolders = false
        };

        FilesModel files = new FilesModel();
        files.SetFilesystem(fs);

        files.Drop(DropListForTopLevel(basePath), fileFilters);

        string[] expectedContains =
        [
            ".hidden file.txt",
            "normal file.txt",
            "inaccessible file.txt",
            "normal directory/subdirectory/normal file.txt",
            "normal directory/subdirectory file.txt",
            "normal directory/subdirectory",
            "normal directory",
            "normal directory no files",
            ".hidden directory no files",
        ];
        AssertFileList(expectedContains, files, basePath);
    }

}
