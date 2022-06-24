using System;
using UnityEngine;

namespace VALVE.Formats.BSP
{
    struct TexData : ILump
    {
        public int bytes { get => 32; }

        public readonly Vector3 reflectivity;    // RGB reflectivity
        public readonly int nameStringTableID;  // index into TexdataStringTable
        public readonly int width;
        public readonly int height;             // source image
        public readonly int viewWidth;
        public readonly int viewHeight;

        public TexData(byte[] data)
        {
            reflectivity = new Vector3(
                BitConverter.ToSingle(data, 0),
                BitConverter.ToSingle(data, 4),
                BitConverter.ToSingle(data, 8)
            );
            nameStringTableID = BitConverter.ToInt32(data, 12);
            width = BitConverter.ToInt32(data, 16);
            height = BitConverter.ToInt32(data, 20);
            viewWidth = BitConverter.ToInt32(data, 24);
            viewHeight = BitConverter.ToInt32(data, 28);
        }
    };
}
