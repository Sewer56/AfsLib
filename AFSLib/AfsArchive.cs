using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using AFSLib.AfsStructs;
using AFSLib.Enums;
using AFSLib.Helpers;
using AFSLib.Structs;
using Reloaded.Memory;
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
            var reader = new Reloaded.Memory.Streams.BufferedStreamReader(stream, sizeof(AfsFileEntry));
            var seekAmount = sizeof(AfsHeader);
            if (index != 0)
                seekAmount += sizeof(AfsFileEntry) * index;
            reader.Seek(seekAmount, SeekOrigin.Begin);
            var entry = reader.Read<AfsFileEntry>();
            byte[] result = reader.ReadBytes(entry.Offset, entry.Length);
            reader.Dispose();
            return result;
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
            var bytes = new ExtendedMemoryStream(sizes.FileSize);
            
            // Add Identification Header.
            bytes.Append(AfsHeader.FromNumberOfFiles(Files.Count));

            // Add file entries.
            int currentOffset = sizes.FileOffset;
            bytes.Append(Files.Select(x =>
            {
                var fileEntry = new AfsFileEntry(currentOffset, x.Data.Length);
                currentOffset = RoundUp(currentOffset + x.Data.Length, alignment);
                return fileEntry;
            }).ToArray());

            // If only header wanted, return from here.
            if (creationMode == FileCreationMode.CreateHeaderOnly)
            {
                bytes.AddPadding(alignment);
                return bytes.ToArray();
            }

            // Add Metadata if Requested
            if (creationMode == FileCreationMode.CreateWithMetadata)
                bytes.Append(new AfsFileEntry(currentOffset, sizes.MetadataSize));

            bytes.AddPadding(alignment);

            // Add files.
            foreach (var file in Files)
            {
                bytes.Append(file.Data);
                bytes.AddPadding(alignment);
            }

            // Write File Metadata
            if (creationMode == FileCreationMode.CreateWithMetadata)
                bytes.Append(Files.Select(x => x.ToMetadata()).ToArray());

            bytes.AddPadding(alignment);
            return bytes.ToArray();
        }
    }
}
