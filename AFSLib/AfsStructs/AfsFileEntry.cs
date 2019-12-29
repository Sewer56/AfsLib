namespace AFSLib.AfsStructs
{
    public struct AfsFileEntry
    {
        /// <summary>
        /// The offset of the file in bytes.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// The length of the file in bytes.
        /// </summary>
        public int Length { get; set; }

        public AfsFileEntry(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }
}
