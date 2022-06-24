using System.IO;

namespace VALVE
{
    class MDL
    {
        public string PATH = null;
        StreamReader inputStream;

        public MDL(string path)
        {
            PATH = path;
            inputStream = new StreamReader(File.OpenRead(PATH));
        }
    }
}