using System;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;

namespace AFSLib.AfsStructs
{
    /// <summary>
    /// Tag for an AFS file.
    /// </summary>
    public unsafe struct AfsHeader
    {
        private const int TagLength = 4;

        /// <summary>
        /// Checks if the file is an AFS archive.
        /// </summary>
        private fixed byte _tag[TagLength];

        /// <summary>
        /// Number of files in the AFS archive.
        /// </summary>
        public int NumberOfFiles;

        /// <summary>
        /// Returns true if the archive is an AFS, else false.
        /// </summary>
        public bool IsAfsArchive
        {
            get
            {
                fixed (byte* tagPtr = _tag)
                    return Marshal.PtrToStringAnsi((IntPtr) tagPtr).Equals("AFS", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns a default AFS header ready for modification.
        /// </summary>
        public static AfsHeader GetDefault()
        {
            var header = new AfsHeader();
            for (int x = 0; x < TagLength; x++) 
                header._tag[x] = 0;

            Memory.CurrentProcess.WriteRaw((IntPtr) header._tag, Encoding.ASCII.GetBytes("AFS"));
            return header;
        }

        /// <summary>
        /// Returns a default AFS header ready for modification.
        /// </summary>
        public static AfsHeader FromNumberOfFiles(int numberOfFiles)
        {
            var header = GetDefault();
            header.NumberOfFiles = numberOfFiles;
            return header;
        }
    }
}
