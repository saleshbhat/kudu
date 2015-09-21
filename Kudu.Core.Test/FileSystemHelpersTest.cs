using Kudu.Core.Infrastructure;
using Moq;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Xunit;

namespace Kudu.Core.Test
{
    public class FileSystemHelpersTest
    {
        [Fact]
        public void EnsureDirectoryCreatesDirectoryIfNotExists()
        {
            var fileSystem = new Mock<IFileSystem>();
            var directory = new Mock<DirectoryBase>();
            fileSystem.Setup(m => m.Directory).Returns(directory.Object);
            directory.Setup(m => m.Exists("foo")).Returns(false);
            FileSystemHelpers.Instance = fileSystem.Object;

            string path = FileSystemHelpers.EnsureDirectory("foo");

            Assert.Equal("foo", path);
            directory.Verify(m => m.CreateDirectory("foo"), Times.Once());
        }

        [Fact]
        public void EnsureDirectoryDoesNotCreateDirectoryIfNotExists()
        {
            var fileSystem = new Mock<IFileSystem>();
            var directory = new Mock<DirectoryBase>();
            fileSystem.Setup(m => m.Directory).Returns(directory.Object);
            directory.Setup(m => m.Exists("foo")).Returns(true);
            FileSystemHelpers.Instance = fileSystem.Object;

            string path = FileSystemHelpers.EnsureDirectory("foo");

            Assert.Equal("foo", path);
            directory.Verify(m => m.CreateDirectory("foo"), Times.Never());
        }

        [Theory]
        [InlineData(@"x:\temp\foo", @"x:\temp\Foo", true)]
        [InlineData(@"x:\temp\foo", @"x:\temp\Foo\bar", true)]
        [InlineData(@"x:\temp\bar", @"x:\temp\Foo", false)]
        [InlineData(@"x:\temp\Foo\bar", @"x:\temp\foo", false)]
        [InlineData(@"x:\temp\foo\", @"x:\temp\Foo\", true)]
        [InlineData(@"x:\temp\Foo\", @"x:\temp\foo", true)]
        [InlineData(@"x:\temp\foo", @"x:\temp\Foo\", true)]
        [InlineData(@"x:\temp\Foo", @"x:\temp\foobar", false)]
        [InlineData(@"x:\temp\foo", @"x:\temp\Foobar\", false)]
        [InlineData(@"x:\temp\foo\..", @"x:\temp\Foo", true)]
        [InlineData(@"x:\temp\..\temp\foo\..", @"x:\temp\Foo", true)]
        [InlineData(@"x:\temp\foo", @"x:\temp\Foo\..", false)]
        // slashes
        [InlineData(@"x:/temp\foo", @"x:\temp\Foo", true)]
        [InlineData(@"x:\temp/foo", @"x:\temp\Foo\bar", true)]
        [InlineData(@"x:\temp\bar", @"x:/temp\Foo", false)]
        [InlineData(@"x:\temp\Foo\bar", @"x:\temp/foo", false)]
        [InlineData(@"x:/temp\foo\", @"x:\temp\Foo\", true)]
        [InlineData(@"x:\temp/Foo\", @"x:\temp\foo", true)]
        [InlineData(@"x:\temp\foo", @"x:\temp/Foo\", true)]
        [InlineData(@"x:\temp/Foo", @"x:\temp\foobar", false)]
        [InlineData(@"x:\temp\foo", @"x:\temp\Foobar/", false)]
        [InlineData(@"x:\temp\foo/..", @"x:/temp\Foo", true)]
        [InlineData(@"x:\temp\..\temp/foo\..", @"x:/temp\Foo", true)]
        [InlineData(@"x:\temp/foo", @"x:\temp\Foo\..", false)]
        public void IsSubfolderOfTests(string parent, string child, bool expected)
        {
            Assert.Equal(expected, FileSystemHelpers.IsSubfolder(parent, child));
        }

        [Fact]
        public void IsFileSystemReadOnlyBasicTest()
        {
            // With Default TmpFolder value should always return false
            FileSystemHelpers.TmpFolder = @"%WEBROOT_PATH%\data\Temp";
            Assert.Equal(false, FileSystemHelpers.IsFileSystemReadOnly());

            // able to create and delete folder, should return false
            var fileSystem = new Mock<IFileSystem>();
            var dirBase = new Mock<DirectoryBase>();
            var dirInfoBase = new Mock<DirectoryInfoBase>();
            var dirInfoFactory = new Mock<IDirectoryInfoFactory>();

            fileSystem.Setup(f => f.Directory).Returns(dirBase.Object);
            fileSystem.Setup(f => f.DirectoryInfo).Returns(dirInfoFactory.Object);

            dirBase.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns(dirInfoBase.Object);
            dirInfoFactory.Setup(d => d.FromDirectoryName(It.IsAny<string>())).Returns(dirInfoBase.Object);

            FileSystemHelpers.Instance = fileSystem.Object;
            FileSystemHelpers.TmpFolder = @"D:\";   // value doesn`t really matter, just need to have something other than default value

            Assert.Equal(false, FileSystemHelpers.IsFileSystemReadOnly());

            // Read-Only should return true
            dirBase.Setup(d => d.CreateDirectory(It.IsAny<string>())).Throws<UnauthorizedAccessException>();
            Assert.Equal(true, FileSystemHelpers.IsFileSystemReadOnly());
        }

        [Theory]
        [InlineData(false, new string[] { "requirements.txt" }, null)]
        //[InlineData(true, new string[] { "requirements.txt", "app.py" }, null)]
        //[InlineData(true, new string[] { "requirements.txt", "runtime.txt" }, "python-3.4.1")]
        //[InlineData(false, new string[] { "requirements.txt", "runtime.txt" }, "i run all the time")]
        //[InlineData(false, new string[] { "requirements.txt", "default.asp" }, null)]
        //[InlineData(false, new string[] { "requirements.txt", "site.aspx" }, null)]
        //[InlineData(false, new string[0], null)]
        //[InlineData(false, new string[] { "index.php" }, null)]
        //[InlineData(false, new string[] { "site.aspx" }, null)]
        //[InlineData(false, new string[] { "site.aspx", "index2.aspx" }, null)]
        //[InlineData(false, new string[] { "server.js" }, null)]
        //[InlineData(false, new string[] { "app.js" }, null)]
        //[InlineData(false, new string[] { "package.json" }, null)]
        public void Qqq(bool looksLikePythonExpectedResult, string[] existingFiles, string runtimeTxtBytes)
        {
            Console.WriteLine("Testing: {0}", String.Join(", ", existingFiles));

            var directory = new Mock<DirectoryBase>();
            var file = new Mock<FileBase>();
            var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
            var dirInfoBase = new Mock<DirectoryInfoBase>();
            var fileInfo = new Mock<FileInfoBase>();
            var fileSystem = new Mock<IFileSystem>();

            dirInfoFactory.Setup(d => d.FromDirectoryName("site")).Returns(dirInfoBase.Object);
            dirInfoBase.Setup(d => d.GetFiles("*.*", SearchOption.AllDirectories)).Returns(new[] { fileInfo.Object });

            directory.Setup(d => d.GetFiles("site", "*.py")).Returns(existingFiles.Where(f => f.EndsWith(".py")).ToArray());

            foreach (var existingFile in existingFiles)
            {
                file.Setup(f => f.Exists("site\\" + existingFile)).Returns(true);
            }

            fileSystem.Setup(f => f.Directory).Returns(directory.Object);
            fileSystem.Setup(f => f.File).Returns(file.Object);
            fileSystem.Setup(f => f.DirectoryInfo).Returns(dirInfoFactory.Object);
            FileSystemHelpers.Instance = fileSystem.Object;

            DirectoryInfoBase jobBinariesDirectory = FileSystemHelpers.DirectoryInfoFromDirectoryName("site");
            FileInfoBase[] files = jobBinariesDirectory.GetFiles("*.*", SearchOption.AllDirectories);

            Assert.Equal(1, files.Length);
        }
    }
}
