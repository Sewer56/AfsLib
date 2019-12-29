using System;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;

namespace AFSLib.AfsStructs
{
    public unsafe struct AfsFileMetadata
    {
        private const int FileNameLength = 32;

        /// <summary>
        /// The name of the file.
        /// </summary>
        private fixed byte _fileName[FileNameLength];

        /// <summary>
        /// Gets or sets the name of the file, up to 32 (<see cref="FileNameLength"/>) characters.
        /// </summary>
        public string FileName
        {
            get
            {
                fixed (byte* fileNamePtr = _fileName)
                    return Marshal.PtrToStringAnsi((IntPtr)fileNamePtr);
            }
            set
            {
                fixed (byte* fileNamePtr = _fileName)
                    Memory.CurrentProcess.WriteRaw((IntPtr) fileNamePtr, Encoding.ASCII.GetBytes(value));
            }
        }

        /// <summary>
        /// The year this file was archived.
        /// </summary>
        public ushort Year;

        /// <summary>
        /// The month this file was archived.
        /// </summary>
        public ushort Month;

        /// <summary>
        /// The day this file was archived.
        /// </summary>
        public ushort Day;

        /// <summary>
        /// The hour this file was archived.
        /// </summary>
        public ushort Hour;

        /// <summary>
        /// The minute this file was archived.
        /// </summary>
        public ushort Minute;

        /// <summary>
        /// The second this file was archived.
        /// </summary>
        public ushort Second;

        /// <summary>
        /// The length of the file, in bytes.
        /// </summary>
        public int Length;
    }
}
