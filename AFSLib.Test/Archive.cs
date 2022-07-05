using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AFSLib.Enums;
using Xunit;

namespace AFSLib.Test
{
    public class Archive
    {
        [Fact]
        public void CanParseAfsFile()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            Assert.True(AfsArchive.TryFromFile(data, out var afsArchive));
            Assert.Equal(3, afsArchive.Files.Count);
        }

        [Fact]
        public void IgnoresBadFile()
        {
            var data = File.ReadAllBytes(Assets.BadHeaderFile);
            Assert.False(AfsArchive.TryFromFile(data, out _));
        }

        [Fact]
        public void FileNamesAreGood()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            Assert.True(AfsArchive.TryFromFile(data, out var afsArchive));
            Assert.Equal("GottaGoFast.png", afsArchive.Files[0].Name);
            Assert.Equal("Documentation.png", afsArchive.Files[1].Name);
            Assert.Equal("ChaoDeletThis.png", afsArchive.Files[2].Name);
        }

        [Fact]
        public void FileDatesAreGood()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            Assert.True(AfsArchive.TryFromFile(data, out var afsArchive));
            Assert.Equal(2019, afsArchive.Files[2].ArchiveTime?.Year);
            Assert.Equal(8, afsArchive.Files[2].ArchiveTime?.Month);
            Assert.Equal(26, afsArchive.Files[2].ArchiveTime?.Day);
            Assert.Equal(15, afsArchive.Files[2].ArchiveTime?.Hour);
            Assert.Equal(12, afsArchive.Files[2].ArchiveTime?.Minute);
            Assert.Equal(56, afsArchive.Files[2].ArchiveTime?.Second);
        }

        [Fact]
        public void RecreateArchive()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            AfsArchive.TryFromFile(data, out var afsArchive);

            var newData = afsArchive.ToBytes();
            Assert.True(AfsArchive.TryFromFile(newData, out var newAfsArchive));

            Assert.Equal(3, afsArchive.Files.Count);
            Assert.Equal("ChaoDeletThis.png", afsArchive.Files[2].Name);
            Assert.Equal(2019, afsArchive.Files[2].ArchiveTime?.Year);
            Assert.Equal(8, afsArchive.Files[2].ArchiveTime?.Month);
            Assert.Equal(26, afsArchive.Files[2].ArchiveTime?.Day);
            Assert.Equal(15, afsArchive.Files[2].ArchiveTime?.Hour);
            Assert.Equal(12, afsArchive.Files[2].ArchiveTime?.Minute);
            Assert.Equal(56, afsArchive.Files[2].ArchiveTime?.Second);
        }

        [Fact]
        public void RecreateArchiveHeaderOnly()
        {
            var data = File.ReadAllBytes(Assets.RealAfsFile);
            AfsArchive.TryFromFile(data, out var afsArchive);

            var newData = afsArchive.ToBytes(2048, FileCreationMode.CreateHeaderOnly);
            Assert.Equal(2048, newData.Length);
            Assert.True(AfsFileViewer.TryFromFile(newData, out var viewer));

            Assert.Equal(2048, viewer.Entries[0].Offset);
            Assert.Equal(478200, viewer.Entries[0].Length);

            Assert.Equal(481280, viewer.Entries[1].Offset);
            Assert.Equal(1414255, viewer.Entries[1].Length);

            Assert.Equal(1896448, viewer.Entries[2].Offset);
            Assert.Equal(514993, viewer.Entries[2].Length);
        }

        [Fact]
        public void SeekToAndLoadDataFromIndex()
        {
            var fileStream = new FileStream(Assets.RealAfsFile, FileMode.Open);
            int testIndex = 1;
            var result = AfsArchive.SeekToAndLoadDataFromIndex(fileStream, testIndex);
            Assert.Equal(1414255, result.Length);
            fileStream.Close();

            AfsArchive.TryFromFile(File.ReadAllBytes(Assets.RealAfsFile), out var realAfs);
            Assert.Equal(realAfs.Files[testIndex].Data, result);
        }
    }
}
