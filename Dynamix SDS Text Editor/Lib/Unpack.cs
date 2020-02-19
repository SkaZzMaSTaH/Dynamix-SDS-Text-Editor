using System.IO;

namespace Lib
{
    public static class Unpack
    {
        public static byte[] DecompressLZW(uint sz, byte[] data)
        {
            LZW LZW = new LZW();
            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            return LZW.Decompress(sz, bin);
        }

        public static FileFormat.Chunks.SDS ChunkSDS(string fileName)
        {
            FileFormat.Chunks.SDS chunkSDS = null;

            BinaryReader bin = new BinaryReader(new MemoryStream(File.ReadAllBytes(fileName)));

            char[] id = new char[4];
            uint chunkSize;
            byte[] data = new byte[] { };

            using (bin)
            {
                id = bin.ReadChars(4);
                chunkSize = bin.ReadUInt32();
                data = bin.ReadBytes((int)chunkSize);
            }

            chunkSDS = new FileFormat.Chunks.SDS(id, data);

            return chunkSDS;
        }
    }
}
