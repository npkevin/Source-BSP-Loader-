using System;
using System.Linq;
using System.Collections.Generic;

using VALVE.Formats.BSP;
using NPKEVIN.Utils;

using UnityEngine;
/* 
    // [3?] Smoked too much when I made this... what was the purpose again?
    // Returns VMTs from inside BPS's PAK
    public VMTParser[] ReadPAK()
    {
        byte[] data = Reader.GetLump(LUMP.PAKFILE, 0)[0];
        PAKReader pReader = new PAKReader(data);
        return pReader.GetVMTs();
    }
*/
namespace VALVE
{
    struct BSP
    {
        public readonly string MapName;
        public readonly BSPReader Reader;

        // BSP Lumps
        public readonly Entity[] entities;
        public readonly Model[] models;
        public readonly SurfaceEdge[] surfaceEdges;
        public readonly Edge[] edges;
        public readonly Vector3[] vertices;
        public readonly TexInfo[] texInfos;
        public readonly TexData[] texDatas;
        public readonly int[] strTable;
        public readonly Dictionary<int, string> StrData; // Cache: <int Offset (from lump.fileOfs), string VMTPath>

        public BSP(string path, string fileName)
        {
            MapName = fileName;
            Reader = BSPReader.singleton;
            Reader.SetPath(path + MapName + ".bsp");

            //Cache
            StrData = new Dictionary<int, string>();

            // Get Lumps from BSP file
            models = Reader.GetLump<Model>(LUMP.MODELS);
            edges = Reader.GetLump<Edge>(LUMP.EDGES);
            surfaceEdges = Reader.GetLump<SurfaceEdge>(LUMP.SURFEDGES);
            texInfos = Reader.GetLump<TexInfo>(LUMP.TEXINFO);
            texDatas = Reader.GetLump<TexData>(LUMP.TEXDATA);

            entities = new EntityParser(Reader.GetLump(LUMP.ENTITIES)[0]).Parse().ToArray();
            strTable = Reader.GetLump(LUMP.TEXDATA_STRING_TABLE, sizeof(int))
                .Select(data => BitConverter.ToInt32(data, 0))
                .ToArray();
            vertices = Reader.GetLump(LUMP.VERTEXES, sizeof(float) * 3)
                .Select(data => new Vector3(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8)))
                .ToArray();
        }
    }
}