using System;
using System.IO;
using System.Linq;
using VALVE.Formats.VTF;

/* TODO:
*   - Get all mipmaps (currently only returning the full-res image)
*   - Make GetMaterials() return a Material[] (mipmaps)
*/

namespace VALVE
{
    class VTF
    {
        public readonly Header header;
        public readonly byte[] data;
        byte[] thumbnailData;

        // TODO: Handle Files
        public VTF(string path)
        {
            throw new NotImplementedException();
            // FileStream fs = File.OpenRead(path);
            // byte[] buff = new byte[64];
            // fs.Close();
        }

        public VTF(TROIKA.TTH tth, TROIKA.TTZ ttz)
        {
            byte[] tthSection = tth.GetVTFSection();

            header = new Header(tthSection.Take(64).ToArray());
            thumbnailData = tthSection.Skip(64).ToArray();
            data = ttz.Inflate();
        }

        public bool hasFlag(TEXTURE_FLAG flag) => (header.flags & flag) == flag;

        /*
        private int GetMipmapSize(int length, int level)
        {
            if (length > 1)
                // bitshift right by level (divide by 2 "level" times)
                return length >> level;
            return 1;
        }
        */
    }

}
