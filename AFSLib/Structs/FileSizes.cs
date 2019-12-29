using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AFSLib.AfsStructs;
using AFSLib.Enums;
using static AFSLib.Helpers.Mathematics;

namespace AFSLib.Structs
{
    /// <summary>
    /// Represents the file sizes of various AFS file segments.
    /// </summary>
    internal unsafe struct FileSizes
    {
        public int HeaderOffset    => 0;
        public int FileEntryOffset => sizeof(AfsHeader);

        public int FileOffset      => HeaderSize;
        public int MetadataOffset  => FileOffset + FileSize;

        public int HeaderSize;
        public int FileSize;
        public int MetadataSize;
        public int TotalSize;

        /// <summary>
        /// Returns the total size of the file as well as each of the individual segments of the file.
        /// </summary>
        /// <returns></returns>
        public FileSizes(AfsArchive archive, int alignment = 2048, FileCreationMode creationMode = FileCreationMode.CreateWithMetadata) : this()
        {
            var fileListEntries = archive.Files.Count + (creationMode == FileCreationMode.CreateWithMetadata ? 1 : 0);

            HeaderSize   = RoundUp(sizeof(AfsHeader) + (sizeof(AfsFileEntry) * fileListEntries), alignment);
            FileSize     = archive.Files.Sum(x => RoundUp(x.Data.Length, alignment));
            MetadataSize = RoundUp(sizeof(AfsFileMetadata) * archive.Files.Count, alignment);
            TotalSize    = GetTotalFileSize(creationMode);
        }

        private int GetTotalFileSize(FileCreationMode creationMode = FileCreationMode.CreateWithMetadata)
        {
            switch (creationMode)
            {
                case FileCreationMode.CreateWithMetadata:
                    return HeaderSize + FileSize + MetadataSize;
                case FileCreationMode.CreateNoMetadata:
                    return HeaderSize + FileSize;
                case FileCreationMode.CreateHeaderOnly:
                    return HeaderSize;
                default:
                    throw new ArgumentOutOfRangeException(nameof(creationMode), creationMode, null);
            }
        }
    }
}
