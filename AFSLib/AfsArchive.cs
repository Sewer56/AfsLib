using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AFSLib.AfsStructs;
using AFSLib.Enums;
using AFSLib.Structs;
using Reloaded.Memory.Extensions;
using static AFSLib.Helpers.Mathematics;
using File = AFSLib.Structs.File;

namespace AFSLib
{
    public unsafe class AfsArchive
    {
        /// <summary>
        /// Contains the complete set of files for this archive.
        /// </summary>
        public List<File> Files { get; set; }

        /// <summary>
        /// Creates an AFS Archive from a list of files.
        /// </summary>
        /// <param name="files">The list of files.</param>
        public AfsArchive(List<File> files)
        {
            Files = files;
        }

        /// <summary>
        /// Creates an empty AFS archive.
        /// </summary>
        public AfsArchive()
        {
            Files = new List<File>();
        }

        /// <summary>
        /// Tries to get an AFS archive from a supplied file viewer.
        /// </summary>
        /// <param name="viewer">An existing instance of <see cref="AfsFileViewer"/></param>
        public static bool FromFileViewer(AfsFileViewer viewer, out AfsArchive archive)
        {
            if (viewer.Header->IsAfsArchive)
            {
                archive = viewer.ToArchive();
                return true;
            }

            archive = new AfsArchive();
            return false;
        }

        /// <summary>
        /// Tries to get an AFS archive from supplied data.
        /// Operation fails if file is not an AFS file.
        /// </summary>
        /// <param name="data">Array containing the AFS data.</param>
        public static bool TryFromFile(byte[] data, out AfsArchive archive)
        {
            if (AfsFileViewer.TryFromFile(data, out var fileViewer))
            {
                archive = fileViewer.ToArchive();
                return true;
            }

            archive = new AfsArchive();
            return false;
        }

        /// <summary>
        /// Retrieves the data at the index without reading other portions of the archive.
        /// 
        /// Useful if dealing with large AFS files to stream known content indexes without loading the entire AFS file.
        /// </summary>
        /// <param name="stream">Stream pointing to AFS archive</param>
        /// <param name="index">The entry index/audioId inside the AFS</param>
        public static byte[] SeekToAndLoadDataFromIndex(Stream stream, int index)
        {
            stream.Seek(sizeof(AfsHeader) + (sizeof(AfsFileEntry) * index), SeekOrigin.Begin);
            stream.Read(out AfsFileEntry entry);
            var data = new byte[entry.Length];
            stream.Seek(entry.Offset, SeekOrigin.Begin);
            stream.Read(data, 0, entry.Length);
            return data;
        }

        /// <summary>
        /// Tries to get an AFS archive from a given pointer.
        /// Operation fails if file is not an AFS file.
        /// </summary>
        /// <param name="data">Pointer to the AFS file data.</param>
        public static bool TryFromMemory(byte* data, out AfsArchive archive)
        {
            if (AfsFileViewer.TryFromMemory(data, out var fileViewer))
            {
                archive = fileViewer.ToArchive();
                return true;
            }

            archive = new AfsArchive();
            return false;
        }

        /// <summary>
        /// Creates an AFS archive writable to disk from the current <see cref="AfsArchive"/>.
        /// </summary>
        /// <param name="alignment">Alignment of the file contents in the AFS file.</param>
        /// <param name="creationMode">Settings for creating the file.</param>
        public byte[] ToBytes(int alignment = 2048, FileCreationMode creationMode = FileCreationMode.CreateWithMetadata)
        {
            var sizes = new FileSizes(this, alignment, creationMode);
            var bytes = new MemoryStream(sizes.FileSize);
            
            // Add Identification Header.
            bytes.Write(AfsHeader.FromNumberOfFiles(Files.Count));

            // Add file entries.
            int currentOffset = sizes.FileOffset;
            bytes.Write(Files.Select(x =>
            {
                var fileEntry = new AfsFileEntry(currentOffset, x.Data.Length);
                currentOffset = RoundUp(currentOffset + x.Data.Length, alignment);
                return fileEntry;
            }).ToArray().AsSpan());

            // If only header wanted, return from here.
            if (creationMode == FileCreationMode.CreateHeaderOnly)
            {
                bytes.AddPadding(alignment);
                return bytes.ToArray();
            }

            // Add Metadata if Requested
            if (creationMode == FileCreationMode.CreateWithMetadata)
                bytes.Write(new AfsFileEntry(currentOffset, sizes.MetadataSize));

            bytes.AddPadding(alignment);

            // Add files.
            foreach (var file in Files)
            {
                bytes.Write(file.Data.AsSpan());
                bytes.AddPadding(alignment);
            }

            // Write File Metadata
            if (creationMode == FileCreationMode.CreateWithMetadata)
                bytes.Write(Files.Select(x => x.ToMetadata()).ToArray().AsSpan());

            bytes.AddPadding(alignment);
            return bytes.ToArray();
        }
    }
}
