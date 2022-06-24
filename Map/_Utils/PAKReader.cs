using System;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace NPKEVIN.Utils
{
    class PAKReader
    {
        //Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public List<VMTParser> VMTs = new List<VMTParser>();
        ZipFile zip;
        MemoryStream pakStream;

        public PAKReader(byte[] data)
        {
            pakStream = new MemoryStream(data);
            zip = new ZipFile(pakStream);
        }

        public VMTParser[] GetVMTs()
        {
            foreach (ZipEntry ze in zip)
            {
                if (ze.IsDirectory) continue;

                string[] path = ze.Name.Split('/');
                string fileName = path[path.Length - 1];
                string[] fileName_split = fileName.Split('.');
                string fileExt = fileName_split[fileName_split.Length - 1];

                // Found a Texture, add to dictionary
                if (fileExt.ToLower() == "vmt")
                {
                    string filePath = "";
                    for (int i = 0; i < path.Length - 1; i++)
                        filePath += path[i] + '/';

                    string fileName_noExt = "";
                    for (int i = 0; i < fileName_split.Length - 1; i++)
                        fileName_noExt += fileName_split[i];


                    // Check if this VMT also has TTH or TTZ inside PAK, if so Add VMT to array
                    if (!ZipHasTroikaTexture(filePath + fileName_noExt))
                    {
                        // Zip's file headers: 30 + filename.length
                        pakStream.Seek(ze.Offset + 30 + ze.Name.Length, SeekOrigin.Begin);

                        byte[] buff = new byte[ze.Size];
                        pakStream.Read(buff, 0, (int)ze.Size);

                        VMTs.Add(new VMTParser(buff));
                        //Debug.Log(ze.Offset + " (0x" + ze.Offset.ToString("X") + "):         Size: " + ze.Size + "\n" + ze.Name);
                    }
                }
            }
            return VMTs.ToArray();
        }

        private bool ZipHasTroikaTexture(string filePath)
        {
            try
            {
                if (zip.GetEntry(filePath + ".ttz").IsFile && zip.GetEntry(filePath + ".tth").IsFile)
                    return true;
            }
            catch (Exception e)
            {
                // isFile when GetEntry() == null
                if (e.GetType() == typeof(NullReferenceException))
                    return false;
                Debug.LogError(e);
            }
            return false;
        }
    }
}
