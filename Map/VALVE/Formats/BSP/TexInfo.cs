using System;
using UnityEngine;

namespace VALVE.Formats.BSP
{
    struct TexInfo : ILump
    {
        public int bytes { get => 72; }

        public readonly Vector4[] textureVecs;   // [st][xyzw]
        public readonly Vector4[] lightmapVecs;  // [st][xyzw]
        public readonly TexInfoFlag flags;               // miptex flags	overrides
        public readonly int texData;             // Pointer to texture name, size, etc.

        public TexInfo(byte[] data)
        {
            textureVecs = new Vector4[] {
                new Vector4(
                    BitConverter.ToSingle(data, 0),
                    BitConverter.ToSingle(data, 4),
                    BitConverter.ToSingle(data, 8),
                    BitConverter.ToSingle(data, 12)
                ),
                new Vector4(
                    BitConverter.ToSingle(data, 16),
                    BitConverter.ToSingle(data, 20),
                    BitConverter.ToSingle(data, 24),
                    BitConverter.ToSingle(data, 28)
                ),
            };
            lightmapVecs = new Vector4[] {
                new Vector4(
                    BitConverter.ToSingle(data, 32),
                    BitConverter.ToSingle(data, 36),
                    BitConverter.ToSingle(data, 40),
                    BitConverter.ToSingle(data, 44)
                ),
                new Vector4(
                    BitConverter.ToSingle(data, 48),
                    BitConverter.ToSingle(data, 52),
                    BitConverter.ToSingle(data, 58),
                    BitConverter.ToSingle(data, 60)
                ),
            };
            flags = (TexInfoFlag)BitConverter.ToInt32(data, 64);
            texData = BitConverter.ToInt32(data, 68);
        }
    }

    [Flags]
    enum TexInfoFlag
    {
        LIGHT = 1 << 0,         //value will hold the light strength
        SKY2D = 1 << 1,         //don't draw, indicates we should skylight + draw 2d sky but not draw the 3D skybox
        SKY = 1 << 2,           //don't draw, but add to skybox
        WARP = 1 << 3,          //turbulent water warp
        TRANS = 1 << 4,         //texture is translucent
        NOPORTAL = 1 << 5,      //the surface can not have a portal placed on it
        TRIGGER = 1 << 6,       //FIXME: This is an xbox hack to work around elimination of trigger surfaces, which breaks occluders
        NODRAW = 1 << 7,        //don't bother referencing the texture
        HINT = 1 << 8,          //make a primary bsp splitter
        SKIP = 1 << 9,          //completely ignore, allowing non-closed brushes
        NOLIGHT = 1 << 10,      //Don't calculate light
        BUMPLIGHT = 1 << 11,    //calculate three lightmaps for the surface for bumpmapping
        NOSHADOWS = 1 << 12,    //Don't receive shadows
        NODECALS = 1 << 13,     //Don't receive decals
        NOCHOP = 1 << 14,       //Don't subdivide patches on this surface
        HITBOX = 1 << 15,       //surface is part of a hitbox
    }
}
