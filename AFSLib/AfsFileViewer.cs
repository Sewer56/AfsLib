using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using AFSLib.AfsStructs;
using AFSLib.Structs;
using Reloaded.Memory;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Sources;

namespace AFSLib
{
    /// <summary>
    /// Allows you to view the contents of AFS files via direct loading (no copying of data).
    /// </summary>
    public unsafe class AfsFileViewer : IDisposable
    {
        /// <summary>
        /// The header of the AFS file.
        /// </summary>
        public AfsHeader* Header { get; private set; }

        /// <summary>
        /// Contains all of the file entries for the given AFS file.
        /// </summary>
        public FixedArrayPtr<AfsFileEntry> Entries { get; private set; }

        /// <summary>
        /// Contains all of the metadata for the contents of the AFS file.
        /// Not all AFS archives contain metadata, this may be null.
        /// </summary>
        public FixedArrayPtr<AfsFileMetadata>? Metadata { get; private set; }

        private GCHandle? _handle;

        private AfsFileViewer() { }
        private AfsFileViewer(byte[] data) => _handle = GCHandle.Alloc(data, GCHandleType.Pinned);

        ~AfsFileViewer()
        {
            Dispose();
        }

        public void Dispose()
        {
            _handle?.Free();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Tries to get an <see cref="AfsFileViewer"/> from supplied data.
        /// Operation fails if file is not an AFS file.
        /// </summary>
        /// <param name="data">Array containing the AFS data.</param>
        public static bool TryFromFile(byte[] data, out AfsFileViewer afsFileViewer)
        {
            if (data.Length < Struct.GetSize<AfsHeader>())
                throw new Exception("Byte array too small to contain header.");

            afsFileViewer = new AfsFileViewer(data);
            return afsFileViewer.TryParseFile((byte*)afsFileViewer._handle?.AddrOfPinnedObject());
        }

        /// <summary>
        /// Tries to get an <see cref="AfsFileViewer"/> from a given pointer.
        /// Operation fails if file is not an AFS file.
        /// </summary>
        /// <param name="data">Pointer to the AFS file data.</param>
        public static bool TryFromMemory(byte* data, out AfsFileViewer afsFileViewer)
        {
            afsFileViewer = new AfsFileViewer();
            return afsFileViewer.TryParseFile(data);
        }

        /// <summary>
        /// Creates an <see cref="AfsArchive"/> from the current <see cref="AfsFileViewer"/>.
        /// </summary>
        public AfsArchive ToArchive()
        {
            var files = new List<File>(Entries.Count);

            for (var x = 0; x < Entries.Count; x++)
            {
                var entry = Entries[x];
                Memory.CurrentProcess.ReadRaw((nuint) GetAddress(entry.Offset), out byte[] data, entry.Length);
                var afsFile = new File($"{x}", data);

                if (Metadata != null)
                {
                    Metadata.Value.Get(out var metadata, x);
                    afsFile.Name = metadata.FileName;

                    if (metadata.HasTimeStamp)
                        afsFile.ArchiveTime = new DateTime(metadata.Year, metadata.Month, metadata.Day, metadata.Hour, metadata.Minute, metadata.Second);
                }

                files.Add(afsFile);
            }

            return new AfsArchive(files);
        }

        /// <summary>
        /// Returns the memory address of a specified offset into the file.
        /// </summary>
        private void* GetAddress(int offset)
        {
            return (byte*) Header + offset;
        }

        private bool TryParseFile(byte* filePointer)
        {
            Header = (AfsHeader*) filePointer;
            if (!Header->IsAfsArchive)
                return false;

            filePointer += sizeof(AfsHeader);
            Entries = new FixedArrayPtr<AfsFileEntry>((UIntPtr) filePointer, Header->NumberOfFiles);

            filePointer += sizeof(AfsFileEntry) * Header->NumberOfFiles;
            var metadataEntries = (AfsFileEntry*)filePointer;
            if (metadataEntries->Length > 0 && metadataEntries->Offset > 0)
                Metadata = new FixedArrayPtr<AfsFileMetadata>((UIntPtr)GetAddress(metadataEntries->Offset), Header->NumberOfFiles);

            return true;
        }
    }
}
