using System;

namespace VALVE.Formats.BSP
{
    // VTMB's modified dface_t
    // https://developer.valvesoftware.com/wiki/Source_BSP_File_Format/Game-Specific
    struct Face : ILump
    {
        public int bytes { get => 104; }

        public readonly ColorRGBExp32[] m_avgLightColor; // colorRGBExp32 => [(byte)R,G,B,  (int8)Exponent]. m_avgLightColor[8][4]

        public readonly ushort planeNum;
        public readonly byte side;          // faces opposite to the node's plane direction
        public readonly byte onNode;        // 1 if on node, 0 if in leaf
        public readonly int firstEdge;      // we must support > 64k edges
        public readonly short numEdges;
        public readonly short texInfo;
        public readonly short dispInfo;
        public readonly short surfaceFogVolumeID;
        public readonly byte[] styles;      // lighting info MAXLIGHTMAPS

        public readonly byte[] day;         // Daytime lightmapping system MAXLIGHTMAPS
        public readonly byte[] night;       // Nightime lightmapping system MAXLIGHTMAPS

        public readonly int lightOfs;       // start of [numstyles*surfsize] samples
        public readonly float area;
        public readonly int[] m_LightmapTextureMinsInLuxels; //2
        public readonly int[] m_LightmapTextureSizeInLuxels; //2
        public readonly int origFace;       // reference the original face this face was derived from
        public readonly uint smoothingGroups;

        public Face(byte[] data)
        {
            // Can be done in a loop but defining explicitly for readability/understanding
            m_avgLightColor = new ColorRGBExp32[]
            {
                new ColorRGBExp32 ( new byte[] {data[0], data[1], data[2], data[3] }),        // m_AvgLightColor[0]
                new ColorRGBExp32 ( new byte[] {data[4], data[5], data[6], data[7] }),        // m_AvgLightColor[1]
                new ColorRGBExp32 ( new byte[] {data[8], data[9], data[10], data[11] }),      // m_AvgLightColor[2]
                new ColorRGBExp32 ( new byte[] {data[12], data[13], data[14], data[15] }),    // m_AvgLightColor[3]
                new ColorRGBExp32 ( new byte[] {data[16], data[17], data[18], data[19] }),    // m_AvgLightColor[4]
                new ColorRGBExp32 ( new byte[] {data[20], data[21], data[22], data[23] }),    // m_AvgLightColor[5]
                new ColorRGBExp32 ( new byte[] {data[24], data[25], data[26], data[27] }),    // m_AvgLightColor[6]
                new ColorRGBExp32 ( new byte[] {data[28], data[29], data[30], data[31] }),    // m_AvgLightColor[7]
            };
            planeNum = BitConverter.ToUInt16(data, 32);                // uint16 planenum (2bytes)
            side = data[34];                                           // side (byte)
            onNode = data[35];                                         // onNode (byte)
            firstEdge = BitConverter.ToInt32(data, 36);                // int firstedge (4 bytes)
            numEdges = BitConverter.ToInt16(data, 40);                 // int16 numedges (2bytes)
            texInfo = BitConverter.ToInt16(data, 42);                  // int16 textinfo (2bytes)
            dispInfo = BitConverter.ToInt16(data, 44);                 // int16 dispinfo (2bytes)
            surfaceFogVolumeID = BitConverter.ToInt16(data, 46);       // int16 surfaceFogVolumeID (2bytes)
            styles = new byte[]
            {
                data[48], data[49], data[50], data[51], // styles[0-3]
                data[52], data[53], data[54], data[55], // styles[4-7]
            };
            day = new byte[]
            {
                data[56], data[57], data[58], data[59], // day[0-3]
                data[60], data[61], data[62], data[63], // day[4-7]
            };
            night = new byte[]
            {
                data[64], data[65], data[66], data[67], // night[0-3]
                data[68], data[69], data[70], data[71], // night[4-7]
            };
            lightOfs = BitConverter.ToInt32(data, 72);  // int lightofs (4bytes)
            area = BitConverter.ToSingle(data, 76);     // float area (4bytes)

            // int[2] m_LightmapTextureMinsInLuxels (8bytes)
            // int[2] m_LightmapTextureSizeInLuxels (8bytes)
            m_LightmapTextureMinsInLuxels = new int[] { BitConverter.ToInt32(data, 80), BitConverter.ToInt32(data, 84) };
            m_LightmapTextureSizeInLuxels = new int[] { BitConverter.ToInt32(data, 88), BitConverter.ToInt32(data, 92) };

            origFace = BitConverter.ToInt32(data, 96);          // int origFace (4bytes)
            smoothingGroups = BitConverter.ToUInt32(data, 100); // uint smoothingGroups (4bytes)
        }
    }
}
