using System;
using UnityEngine;

namespace VALVE.Formats.VTF
{
    struct Header
    {
        public readonly int ident;                  // File signature ("VTF\0"). (or as little-endian integer, 0x00465456)
        public readonly uint[] version;             // version[0].version[1] (currently 7.2).
        public readonly uint size;                  // Size of the header struct  (16 byte aligned; currently 80 bytes) + size of the resources dictionary (7.3+).
        public readonly ushort width;               // Width of the largest mipmap in pixels. Must be a power of 2.
        public readonly ushort height;              // Height of the largest mipmap in pixels. Must be a power of 2.
        public readonly TEXTURE_FLAG flags;                 // VTF flags.
        public readonly ushort frames;              // Number of frames, if animated (1 for no animation).
        public readonly ushort firstFrame;          // First frame in animation (0 based). Can be -1 in environment maps older than 7.5, meaning there are 7 faces, not 6.
        // byte[] padding0;                         // reflectivity padding (16 byte alignment).
        public readonly Vector3 reflectivity;       // reflectivity vector.
        // byte[] padding1;                         // reflectivity padding (8 byte packing).
        public readonly float bumpmapScale;         // Bumpmap scale.
        public readonly IMAGE_FORMAT highResImageFormat;    // High resolution image format.
        public readonly ushort mipmapCount;                 // Number of mipmaps.
        public readonly IMAGE_FORMAT lowResImageFormat;     // Low resolution image format (always DXT1).
        public readonly ushort lowResImageWidth;            // Low resolution image width.
        public readonly ushort lowResImageHeight;           // Low resolution image height.

        /*
        // 7.2+
        ushort depth;               // Depth of the largest mipmap in pixels. Must be a power of 2. Is 1 for a 2D texture.

        // 7.3+
        ushort[] padding2;         // depth padding (4 byte alignment).
        uint numResources;          // Number of resources this vtf has. The max appears to be 32.

        ushort[] padding3;         // Necessary on certain compilers
        */

        public Header(byte[] data)
        {
            ident = BitConverter.ToInt32(data, 0);
            version = new uint[] {
                BitConverter.ToUInt32(data, 4),
                BitConverter.ToUInt32(data, 8)
            };
            size = BitConverter.ToUInt32(data, 12);

            // 16 bytes
            width = BitConverter.ToUInt16(data, 16);
            height = BitConverter.ToUInt16(data, 18);
            flags = (TEXTURE_FLAG)BitConverter.ToUInt32(data, 20);
            frames = BitConverter.ToUInt16(data, 24);
            firstFrame = BitConverter.ToUInt16(data, 26);
            // padding0 (4 bytes)

            // 32 bytes
            reflectivity = new Vector3(
                BitConverter.ToSingle(data, 32),
                BitConverter.ToSingle(data, 36),
                BitConverter.ToSingle(data, 40)
            );
            // padding1 (4 bytes)

            // 48 bytes
            bumpmapScale = BitConverter.ToSingle(data, 48);
            highResImageFormat = (IMAGE_FORMAT)BitConverter.ToUInt32(data, 52);
            mipmapCount = data[56];
            lowResImageFormat = (IMAGE_FORMAT)BitConverter.ToUInt32(data, 57);
            lowResImageWidth = data[61];
            lowResImageHeight = data[62];
            // padding3 (1 byte)
        }
    }


}
