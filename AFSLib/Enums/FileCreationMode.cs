using System;
using System.Collections.Generic;
using System.Text;

namespace AFSLib.Enums
{
    public enum FileCreationMode
    {
        /// <summary>
        /// Creates the whole file.
        /// </summary>
        CreateWithMetadata,

        /// <summary>
        /// Creates the whole file, but without file metadata/information.
        /// </summary>
        CreateNoMetadata,

        /// <summary>
        /// Creates only the header for an AFS file.
        /// </summary>
        CreateHeaderOnly
    }
}
