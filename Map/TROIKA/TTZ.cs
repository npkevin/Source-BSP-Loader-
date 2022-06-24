using System;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace TROIKA
{
    class TTZ
    {
        string FILE_PATH;
        FileStream fs;

        // Caching for optimization (inflate() gets called too much)
        static Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();

        public TTZ (string path)
        {
            FILE_PATH = path + ".ttz";
        }

        static public void ClearCache()
        {
            cache.Clear();
        } 

        public byte[] Inflate()
        {
            // If we've already inflated this ttz, return it.
            if (cache.ContainsKey(FILE_PATH))
                return cache[FILE_PATH];

            fs = File.OpenRead(FILE_PATH);

            // We're not expecting TTZ sizes to be larger than 2.14 Gb.
            // Read(,, int32), whereas fs.Length is a int64?
            if ((int)fs.Length != fs.Length)
            {
                fs.Close();
                throw new Exception("TTZ file is too large! ( > 2Gb )");
            }

            InflaterInputStream inflater = new InflaterInputStream(fs);
            List<byte> data = new List<byte>();
            byte[] buff = new byte[16];

            while (inflater.CanRead)
            {
                inflater.Read(buff, 0, 16);

                // 0 == \EOF
                if (inflater.Available == 0) break;

                data.AddRange(buff);
            }

            inflater.Close();
            fs.Close();

            // Add inflated data to the cache
            cache.Add(FILE_PATH, data.ToArray());
            return data.ToArray();
        }
    }
}
