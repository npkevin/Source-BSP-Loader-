using System;
using System.IO;
using System.Text;

using VALVE.Formats.BSP;


namespace NPKEVIN.Utils
{
    class BSPReader
    {
        private static readonly BSPReader instance = new BSPReader();

        public Header header { get; private set; }
        private FileStream fs;
        private string PATH = "";

        // Hide constructor
        private BSPReader() { }
        static BSPReader() { }

        public static BSPReader singleton
        {
            get { return instance; }
        }


        // Once path is "set, attempt to create header
        public bool SetPath(string path)
        {
            fs = File.OpenRead(path);

            byte[] header_data = new byte[1036];
            fs.Read(header_data, 0, header_data.Length);
            fs.Close();

            Header header_t = new Header(header_data);

            // "VBSP" = 0x50534256
            if (header_t.ident != 0x50534256)
            {
                PATH = "";
                return false;
            }

            PATH = path;
            header = header_t;
            return true;
        }

        private FileStream openFileStream()
        {
            if (PATH == "") throw new Exception("No BSP path set.");
            return File.OpenRead(PATH);
        }

        public string GetTexDataString(int offset)
        {
            fs = openFileStream();

            // Go to where string begins
            Lump lump = header.lumps[(int)LUMP.TEXDATA_STRING_DATA];
            fs.Seek(lump.fileOfs + offset, SeekOrigin.Begin);

            StringBuilder strBuilder = new StringBuilder();
            // Each string has a max of 128 characters
            for (int i = 0; i < 128; i++)
            {
                char c = (char)fs.ReadByte();
                if (c == 0) break;
                strBuilder.Append(c);
            }

            fs.Close();
            return strBuilder.ToString();
        }

        public T[] GetLump<T>(LUMP type, int offset = 0, int length = 0) where T : ILump
        {
            fs = openFileStream();
            int structSize = default(T).bytes;

            // Go to where lump starts (fileOfs)
            Lump lump = header.lumps[(int)type];
            fs.Seek(lump.fileOfs + (offset * structSize), SeekOrigin.Begin);

            // Allocate Memory for the number of structs to read
            int NUM_OF_STRUCTS = length;
            if (length == 0)
                NUM_OF_STRUCTS = lump.fileLen / structSize;

            T[] structs = new T[NUM_OF_STRUCTS];

            for (int i = 0; i < NUM_OF_STRUCTS; i++)
            {
                byte[] data = new byte[structSize];
                fs.Read(data, 0, structSize);
                structs[i] = (T)Activator.CreateInstance(typeof(T), data);
            }

            fs.Close();
            return structs;
        }

        public byte[][] GetLump(LUMP type, int size = 0)
        {
            if (size < 0)
                throw new Exception("Size must be 0 or greater");

            fs = openFileStream();

            // Go to where lump starts (fileOfs)
            Lump lump = header.lumps[(int)type];
            fs.Seek(lump.fileOfs, SeekOrigin.Begin);

            // return the whole byte[] if size is 0
            if (size == 0)
            {
                byte[] buff = new byte[lump.fileLen];
                fs.Read(buff, 0, lump.fileLen);
                return new byte[][] { buff };
            }

            // Allocate Memory for the number of data structs to read
            int LENGTH = lump.fileLen / size;
            byte[][] data = new byte[LENGTH][];

            for (int i = 0; i < LENGTH; i++)
            {
                byte[] buff = new byte[size];
                fs.Read(buff, 0, size);
                data[i] = buff;
            }

            fs.Close();
            return data;
        }
    }
}