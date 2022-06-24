using System;

namespace VALVE.Formats.BSP
{
    struct Header
    {
        public readonly int ident;          // BSP file identifier
        public readonly int version;        // BSP file version
        public readonly Lump[] lumps;       // lump directory array
        public readonly int mapRevision;    // the map's revision (iteration, version) number

        public Header(byte[] data)
        {
            ident = BitConverter.ToInt32(data, 0);
            version = BitConverter.ToInt32(data, 4);
            lumps = new Lump[64];
            // Retrieve lumps
            for (int i = 0; i < 64; i++)
            {
                byte[] lump_data = new byte[16];
                for (int j = 0; j < 16; j++)
                    lump_data[j] = data[(i * 16) + j + 8]; // ident + version = 8 bytes
                lumps[i] = new Lump(lump_data);
            }
            mapRevision = BitConverter.ToInt32(data, 1028);
        }
    }
}