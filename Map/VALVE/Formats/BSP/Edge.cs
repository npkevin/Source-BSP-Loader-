using System;

namespace VALVE.Formats.BSP
{
    struct Edge : ILump
    {
        public int bytes { get => 4; }

        // v[] holds indices for
        public readonly ushort[] vectors;

        public Edge(byte[] data)
        {
            vectors = new ushort[2];
            vectors[0] = BitConverter.ToUInt16(data, 0);
            vectors[1] = BitConverter.ToUInt16(data, 2);
        }
    }
}
