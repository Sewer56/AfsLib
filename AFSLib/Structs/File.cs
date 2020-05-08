using System;
using AFSLib.AfsStructs;

namespace AFSLib.Structs
{
    /// <summary>
    /// Represents an individual file to pack into an AFS archive.
    /// </summary>
    public class File
    {
        private string _name;

        /// <summary>
        /// The data of the individual file.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The time the file was added into the archive.
        /// </summary>
        public DateTime? ArchiveTime { get; set; }

        /// <summary>
        /// The name of the file, this can be maximum 32 characters including extension.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (value.Length > 32)
                    throw new ArgumentException("Maximum Filename Length (including extension) is 32 bytes for AFS archive.");

                _name = value;
            }
        }

        /// <summary>
        /// Creates a new AFS file to add onto an archive.
        /// </summary>
        /// <param name="name">The name of an archive, maximum 32 characters including extension.</param>
        /// <param name="data">The data of the file.</param>
        /// <param name="archiveTime">The time the file was added.</param>
        public File(string name, byte[] data, DateTime archiveTime)
        {
            Name = name;
            Data = data;
            ArchiveTime = archiveTime;
        }

        /// <summary>
        /// Creates a new AFS file to add onto an archive.
        /// </summary>
        /// <param name="name">The name of an archive, maximum 32 characters including extension.</param>
        /// <param name="data">The data of the file.</param>
        public File(string name, byte[] data)
        {
            Name = name;
            Data = data;
            ArchiveTime = DateTime.Now;
        }

        /// <summary>
        /// Returns a <see cref="AfsFileMetadata"/> object corresponding to the AFS file.
        /// </summary>
        public AfsFileMetadata ToMetadata()
        {
            return new AfsFileMetadata(Name, Data.Length, ArchiveTime);
        }
    }
}
