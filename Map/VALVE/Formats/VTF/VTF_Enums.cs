﻿using System;

namespace VALVE.Formats.VTF
{
    enum IMAGE_FORMAT : uint
    {
        //NONE = -1,
        RGBA8888,
        ABGR8888,
        RGB888,
        BGR888,
        RGB565,
        I8,
        IA88,
        P8,
        A8,
        RGB888_BLUESCREEN,
        BGR888_BLUESCREEN,
        ARGB8888,
        BGRA8888,
        DXT1,
        DXT3,
        DXT5,
        BGRX8888,
        BGR565,
        BGRX5551,
        BGRA4444,
        DXT1_ONEBITALPHA,
        BGRA5551,
        UV88,
        UVWQ8888,
        RGBA16161616F,
        RGBA16161616,
        UVLX8888,
    }

    [Flags]
    enum TEXTURE_FLAG : uint
    {
        POINTSAMPLE = 0x00000001,
        TRILINEAR = 0x00000002,
        CLAMPS = 0x00000004,
        CLAMPT = 0x00000008,
        ANISOTROPIC = 0x00000010,
        HINT_DXT5 = 0x00000020,
        PWL_CORRECTED = 0x00000040,
        NORMAL = 0x00000080,
        NOMIP = 0x00000100,
        NOLOD = 0x00000200,
        ALL_MIPS = 0x00000400,
        PROCEDURAL = 0x00000800,

        // These are automatically generated by vtex from the texture data.
        ONEBITALPHA = 0x00001000,
        EIGHTBITALPHA = 0x00002000,

        // Newer flags from the *.txt config file
        ENVMAP = 0x00004000,
        RENDERTARGET = 0x00008000,
        DEPTHRENDERTARGET = 0x00010000,
        NODEBUGOVERRIDE = 0x00020000,
        SINGLECOPY = 0x00040000,
        PRE_SRGB = 0x00080000,

        UNUSED_00100000 = 0x00100000,
        UNUSED_00200000 = 0x00200000,
        UNUSED_00400000 = 0x00400000,

        NODEPTHBUFFER = 0x00800000,

        UNUSED_01000000 = 0x01000000,

        CLAMPU = 0x02000000,
        VERTEXTEXTURE = 0x04000000,
        SSBUMP = 0x08000000,

        UNUSED_10000000 = 0x10000000,

        BORDER = 0x20000000,

        UNUSED_40000000 = 0x40000000,
        UNUSED_80000000 = 0x80000000,
    }
}
