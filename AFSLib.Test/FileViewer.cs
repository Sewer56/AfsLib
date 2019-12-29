using System;
using System.IO;
using Xunit;

namespace AFSLib.Test
{
    public unsafe class FileViewer
    {
        [Fact]
        public void CanParseAfsFile()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            Assert.True(AfsFileViewer.TryFromFile(data, out var afsFileViewer));
            Assert.True(afsFileViewer.Header->IsAfsArchive);
            Assert.Equal(3, afsFileViewer.Header->NumberOfFiles);
        }

        [Fact]
        public void IgnoresBadFile()
        {
            var data = File.ReadAllBytes(Assets.BadHeaderFile);
            Assert.False(AfsFileViewer.TryFromFile(data, out var afsFileViewer));
            Assert.False(afsFileViewer.Header->IsAfsArchive);
        }

        [Fact]
        public void FileNamesAreGood()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            Assert.True(AfsFileViewer.TryFromFile(data, out var afsFileViewer));
            Assert.NotNull(afsFileViewer.Metadata);
            Assert.Equal("GottaGoFast.png", afsFileViewer.Metadata.Value[0].FileName);
            Assert.Equal("Documentation.png", afsFileViewer.Metadata.Value[1].FileName);
            Assert.Equal("ChaoDeletThis.png", afsFileViewer.Metadata.Value[2].FileName);
        }

        [Fact]
        public void FileDatesAreGood()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            Assert.True(AfsFileViewer.TryFromFile(data, out var afsFileViewer));
            Assert.NotNull(afsFileViewer.Metadata);
            Assert.Equal(2019, afsFileViewer.Metadata.Value[2].Year);
            Assert.Equal(8, afsFileViewer.Metadata.Value[2].Month);
            Assert.Equal(26, afsFileViewer.Metadata.Value[2].Day);
            Assert.Equal(15, afsFileViewer.Metadata.Value[2].Hour);
            Assert.Equal(12, afsFileViewer.Metadata.Value[2].Minute);
            Assert.Equal(56, afsFileViewer.Metadata.Value[2].Second);
        }
    }
}
