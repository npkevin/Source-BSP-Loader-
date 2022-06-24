using System;
using System.IO;

/*
 * TODOs:
 * - Figure out what information is in the TTH file's tth header
 * - Figure out what information is after the VTF's [0-64 bytes] header
 *      I think it is a low res thumbnail.
 */

namespace TROIKA
{
    class TTH
    {
        string FILE_PATH;
        FileStream fs;

        int VTFOffset = 0;
        int VTFSize = 0;

        public TTH(string path)
        {
            FILE_PATH = path + ".tth";
            fs = File.OpenRead(FILE_PATH);

            // Get Offset & Size for VTF header
            byte[] buff = new byte[4];
            fs.Seek(8, SeekOrigin.Begin);
            fs.Read(buff, 0, 4);

            VTFSize = BitConverter.ToInt32(buff, 0);
            VTFOffset = (int)fs.Length - VTFSize;

            // Verify this TTH file
            //
            int TTHMagic = GetTTHMagic();
            if (TTHMagic != 0x00485454) // "TTH\0"
                throw new Exception("Invalid TTH identifier:" + TTHMagic);

            int VTFMagic = GetVTFMagic();
            if (VTFMagic != 0x00465456) // "VTF\0"
                new Exception("Invalid VTF identifier:" + VTFMagic);

            fs.Close();
        }

        public TROIKA.Formats.TTH.Header GetTTHHeader()
        {
            if (VTFOffset <= 0)
                throw new Exception("Could not get TTH header size");

            fs = File.OpenRead(FILE_PATH);
            fs.Seek(0, SeekOrigin.Begin);

            byte[] buff = new byte[VTFOffset];
            fs.Read(buff, 0, VTFOffset);
            fs.Close();

            return new TROIKA.Formats.TTH.Header(buff);
        }

        public VALVE.Formats.VTF.Header GetVTFHeader()
        {
            return new VALVE.Formats.VTF.Header(GetVTFSection());
        }

        public byte[] GetVTFSection()
        {
            if (VTFSize <= 0)
                throw new Exception("Could not get VTF header size");

            fs = File.OpenRead(FILE_PATH);
            fs.Seek(VTFOffset, SeekOrigin.Begin);

            byte[] buff = new byte[VTFSize];
            fs.Read(buff, 0, VTFSize);
            fs.Close();

            return buff;
        }

        private int GetTTHMagic()
        {
            fs.Seek(0, SeekOrigin.Begin);

            byte[] buff = new byte[4];
            fs.Read(buff, 0, 4);

            return BitConverter.ToInt32(buff, 0);
        }

        private int GetVTFMagic()
        {
            fs.Seek(VTFOffset, SeekOrigin.Begin);

            byte[] buff = new byte[4];
            fs.Read(buff, 0, 4);

            return BitConverter.ToInt32(buff, 0);
        }
    }
}
