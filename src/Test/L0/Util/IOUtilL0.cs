using Microsoft.VisualStudio.Services.Agent.Util;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests.Util
{
    public sealed class IOUtilL0
    {
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeletesDirectoriesRecursively()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange: Create a directory with a grandchild directory.
                string directory = Path.Combine(IOUtil.GetBinPath(), Path.GetRandomFileName());
                try
                {
                    Directory.CreateDirectory(Path.Combine(directory, "some child directory", "some grandchild directory"));

                    // Act.
                    IOUtil.DeleteDirectory(directory, CancellationToken.None);

                    // Assert.
                    Assert.False(Directory.Exists(directory));
                }
                finally
                {
                    // Cleanup.
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeletesFilesRecursively()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange: Create a directory with a grandchild file.
                string directory = Path.Combine(IOUtil.GetBinPath(), Path.GetRandomFileName());
                try
                {
                    string file = Path.Combine(directory, "some subdirectory", "some file");
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                    File.WriteAllText(path: file, contents: "some contents");

                    // Act.
                    IOUtil.DeleteDirectory(directory, CancellationToken.None);

                    // Assert.
                    Assert.False(Directory.Exists(directory));
                }
                finally
                {
                    // Cleanup.
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeletesReadOnlyDirectories()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange: Create a directory with a read-only subdirectory.
                string directory = Path.Combine(IOUtil.GetBinPath(), Path.GetRandomFileName());
                string subdirectory = Path.Combine(directory, "some subdirectory");
                try
                {
                    var subdirectoryInfo = new DirectoryInfo(subdirectory);
                    subdirectoryInfo.Create();
                    subdirectoryInfo.Attributes = subdirectoryInfo.Attributes | FileAttributes.ReadOnly;

                    // Act.
                    IOUtil.DeleteDirectory(directory, CancellationToken.None);

                    // Assert.
                    Assert.False(Directory.Exists(directory));
                }
                finally
                {
                    // Cleanup.
                    var subdirectoryInfo = new DirectoryInfo(subdirectory);
                    if (subdirectoryInfo.Exists)
                    {
                        subdirectoryInfo.Attributes = subdirectoryInfo.Attributes & ~FileAttributes.ReadOnly;
                    }

                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeletesReadOnlyRootDirectory()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange: Create a read-only directory.
                string directory = Path.Combine(IOUtil.GetBinPath(), Path.GetRandomFileName());
                try
                {
                    var directoryInfo = new DirectoryInfo(directory);
                    directoryInfo.Create();
                    directoryInfo.Attributes = directoryInfo.Attributes | FileAttributes.ReadOnly;

                    // Act.
                    IOUtil.DeleteDirectory(directory, CancellationToken.None);

                    // Assert.
                    Assert.False(Directory.Exists(directory));
                }
                finally
                {
                    // Cleanup.
                    var directoryInfo = new DirectoryInfo(directory);
                    if (directoryInfo.Exists)
                    {
                        directoryInfo.Attributes = directoryInfo.Attributes & ~FileAttributes.ReadOnly;
                        directoryInfo.Delete();
                    }
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void DeletesReadOnlyFiles()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                // Arrange: Create a directory with a read-only file.
                string directory = Path.Combine(IOUtil.GetBinPath(), Path.GetRandomFileName());
                string file = Path.Combine(directory, "some file");
                try
                {
                    Directory.CreateDirectory(directory);
                    File.WriteAllText(path: file, contents: "some contents");
                    File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.ReadOnly);

                    // Act.
                    IOUtil.DeleteDirectory(directory, CancellationToken.None);

                    // Assert.
                    Assert.False(Directory.Exists(directory));
                }
                finally
                {
                    // Cleanup.
                    if (File.Exists(file))
                    {
                        File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
                    }

                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void GetRelativPath()
        {
            using (TestHostContext hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace();

                string relativePath;
#if OS_WINDOWS
                /// MakeRelative(@"d:\src\project\foo.cpp", @"d:\src") -> @"project\foo.cpp"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:\src\project\foo.cpp", @"d:\src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project\foo.cpp", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:\", @"d:\specs") -> @"d:\"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:\", @"d:\specs");
                // Assert.
                Assert.True(string.Equals(relativePath, @"d:\", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:\src\project\foo.cpp", @"d:\src\proj") -> @"d:\src\project\foo.cpp"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:\src\project\foo.cpp", @"d:\src\proj");
                // Assert.
                Assert.True(string.Equals(relativePath, @"d:\src\project\foo.cpp", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:\src\project\foo", @"d:\src") -> @"project\foo"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:\src\project\foo", @"d:\src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project\foo", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:\src\project\foo.cpp", @"d:\src\project\foo.cpp") -> @""
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:\src\project", @"d:\src\project");
                // Assert.
                Assert.True(string.Equals(relativePath, string.Empty, StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:/src/project/foo.cpp", @"d:/src") -> @"project/foo.cpp"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:/src/project/foo.cpp", @"d:/src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project\foo.cpp", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:/src/project/foo.cpp", @"d:\src") -> @"d:/src/project/foo.cpp"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:/src/project/foo.cpp", @"d:/src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project\foo.cpp", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d:/src/project/foo", @"d:/src") -> @"project/foo"
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:/src/project/foo", @"d:/src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project\foo", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"d\src\project", @"d:/src/project") -> @""
                // Act.
                relativePath = IOUtil.MakeRelative(@"d:\src\project", @"d:/src/project");
                // Assert.
                Assert.True(string.Equals(relativePath, string.Empty, StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");
#else
                /// MakeRelative(@"/user/src/project/foo.cpp", @"/user/src") -> @"project/foo.cpp"
                // Act.
                relativePath = IOUtil.MakeRelative(@"/user/src/project/foo.cpp", @"/user/src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project/foo.cpp", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"/user", @"/user/specs") -> @"/user"
                // Act.
                relativePath = IOUtil.MakeRelative(@"/user", @"/user/specs");
                // Assert.
                Assert.True(string.Equals(relativePath, @"/user", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"/user/src/project/foo.cpp", @"/user/src/proj") -> @"/user/src/project/foo.cpp"
                // Act.
                relativePath = IOUtil.MakeRelative(@"/user/src/project/foo.cpp", @"/user/src/proj");
                // Assert.
                Assert.True(string.Equals(relativePath, @"/user/src/project/foo.cpp", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"/user/src/project/foo", @"/user/src") -> @"project/foo"
                // Act.
                relativePath = IOUtil.MakeRelative(@"/user/src/project/foo", @"/user/src");
                // Assert.
                Assert.True(string.Equals(relativePath, @"project/foo", StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");

                /// MakeRelative(@"/user/src/project", @"/user/src/project") -> @""
                // Act.
                relativePath = IOUtil.MakeRelative(@"/user/src/project", @"/user/src/project");
                // Assert.
                Assert.True(string.Equals(relativePath, string.Empty, StringComparison.OrdinalIgnoreCase), $"RelativePath does not expected: {relativePath}");
#endif
            }
        }
    }
}
