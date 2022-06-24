using System;

namespace VALVE.Formats.BSP
{
    interface ILump
    {
        int bytes { get; }
    }

    // BSPs start with and array(64) of Lump structures
    struct Lump
    {
        public readonly int fileOfs;   // offset into file (bytes)
        public readonly int fileLen;   // length of lump (bytes)
        public readonly int version;   // lump format version
        public readonly int fourCC;    // uncompression size (int) or 0's for padding

        public Lump(byte[] data)
        {
            fileOfs = BitConverter.ToInt32(data, 0);
            fileLen = BitConverter.ToInt32(data, 4);
            version = BitConverter.ToInt32(data, 8);
            fourCC = BitConverter.ToInt32(data, 12);
        }
    }


    enum LUMP : uint
    {
        ENTITIES,
        PLANES,
        TEXDATA,
        VERTEXES,
        VISIBILITY,
        NODES,
        TEXINFO,
        FACES,
        LIGHTING,
        OCCLUSION,
        LEAFS,
        FACEIDS,
        EDGES,
        SURFEDGES,
        MODELS,
        WORLDLIGHTS,
        LEAFFACES,
        LEAFBRUSHES,
        BRUSHES,
        BRUSHSIDES,
        AREAS,
        AREAPORTALS,
        PORTALS = 22,
        UNUSED0 = 22,
        PROPCOLLISION = 22,
        CLUSTERS = 23,
        UNUSED1 = 23,
        PROPHULLS = 23,
        PORTALVERTS = 24,
        UNUSED2 = 24,
        PROPHULLVERTS = 24,
        CLUSTERPORTALS = 25,
        UNUSED3 = 25,
        PROPTRIS = 25,
        DISPINFO,
        ORIGINALFACES,
        PHYSDISP,
        PHYSCOLLIDE,
        VERTNORMALS,
        VERTNORMALINDICES,
        DISP_LIGHTMAP_ALPHAS,
        DISP_VERTS,
        DISP_LIGHTMAP_SAMPLE_POSITIONS,
        GAME_LUMP,
        LEAFWATERDATA,
        PRIMITIVES,
        PRIMVERTS,
        PRIMINDICES,
        PAKFILE,
        CLIPPORTALVERTS,
        CUBEMAPS,
        TEXDATA_STRING_DATA,
        TEXDATA_STRING_TABLE,
        OVERLAYS,
        LEAFMINDISTTOWATER,
        FACE_MACRO_TEXTURE_INFO,
        DISP_TRIS,
        PHYSCOLLIDESURFACE = 49,
        PROP_BLOB = 49,
        WATEROVERLAYS,
        LIGHTMAPPAGES = 51,
        LEAF_AMBIENT_INDEX_HDR = 51,
        LIGHTMAPPAGEINFOS = 52,
        LEAF_AMBIENT_INDEX = 52,
        LIGHTING_HDR,
        WORLDLIGHTS_HDR,
        LEAF_AMBIENT_LIGHTING_HDR,
        LEAF_AMBIENT_LIGHTING,
        XZIPPAKFILE,
        FACES_HDR,
        MAP_FLAGS,
        OVERLAY_FADES,
        OVERLAY_SYSTEM_LEVELS,
        PHYSLEVEL,
        DISP_MULTIBLEND
    }
}