using System;
using UnityEngine;

namespace VALVE.Formats.BSP
{
    struct Model : ILump
    {
        public int bytes { get => 48; }

        public readonly Vector3 mins;
        public readonly Vector3 maxs;   // bounding box
        public readonly Vector3 origin; // for sounds or lights
        public readonly int headNode;  // index into node array
        public readonly int firstFace; // index into face array
        public readonly int numFaces;

        public Model(byte[] data)
        {
            mins = new Vector3(
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4),
                BitConverter.ToSingle(data, 8)
            );
            maxs = new Vector3(
                BitConverter.ToSingle(data, 12),
                BitConverter.ToSingle(data, 16),
                BitConverter.ToSingle(data, 20)
            );
            origin = new Vector3(
                BitConverter.ToSingle(data, 24),
                BitConverter.ToSingle(data, 28),
                BitConverter.ToSingle(data, 32)
            );
            headNode = BitConverter.ToInt32(data, 36);
            firstFace = BitConverter.ToInt32(data, 40);
            numFaces = BitConverter.ToInt32(data, 44);
        }
    };
}
