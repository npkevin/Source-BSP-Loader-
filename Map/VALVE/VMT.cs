using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TROIKA;
using VALVE.Formats.VTF;
using NPKEVIN.Utils;
using UnityEngine;

namespace VALVE
{
    // Valve Material, holds data related to a texture
    class VMT
    {
        VMTParser Parser;
        string Path;
        VTF VTF;

        public readonly string BaseTexture;

        public VMT(string vmtPath)
        {
            Parser = new VMTParser(vmtPath);
            Path = vmtPath.Replace(".vmt", "");

            BaseTexture = (string)Parser.SearchKey("*/$baseTexture");
            if (BaseTexture != null)
                BaseTexture = BaseTexture.Replace('\\', '/').TrimStart('/');
        }

        public Material GenerateUnityMaterial(bool drawTools = false)
        {
            string troikaPath = getTroikaPath();
            if (troikaPath != null && VTF == null)
                VTF = new VTF(new TTH(troikaPath), new TTZ(troikaPath));
            else
            {
                // TODO: VTF constructor by path is not implemented yet
                return new Material(Shader.Find("Standard"));
                // VTF = new VTF(Path);
            }


            string name = (string)Parser.SearchKey("*/$baseTexture");
            if (name == null)
                throw new ArgumentNullException("$baseTexture not found");

            name = name.Replace('\\', '/');

            if (name.Contains("toolsskybox"))
                return new Material(Shader.Find("VALVE/SkyBox3D"));

            if (!drawTools &&
            (
                name.Contains("nodraw") ||
                name.Contains("invis") ||
                name.Contains("trigger") ||
                name.Contains("tools")
            ))
                return (Material)Resources.Load("materials/invis", typeof(Material));

            // Debug.Log(name + '\n' + header.flags);

            byte[] highResImage;
            TextureFormat format;

            // https://en.wikipedia.org/wiki/S3_Texture_Compression
            // https://developer.valvesoftware.com/wiki/Valve_Texture_Format
            switch (VTF.header.highResImageFormat)
            {
                case IMAGE_FORMAT.DXT1:
                    highResImage = VTF.data.Skip(VTF.data.Length - VTF.header.height * VTF.header.width / 16 * 8).ToArray();
                    format = TextureFormat.DXT1;
                    break;
                case IMAGE_FORMAT.DXT5:
                    highResImage = VTF.data.Skip(VTF.data.Length - VTF.header.height * VTF.header.width / 16 * 16).ToArray();
                    format = TextureFormat.DXT5;
                    break;
                case IMAGE_FORMAT.BGRA8888:
                    highResImage = VTF.data.Skip(VTF.data.Length - VTF.header.height * VTF.header.width * 32).ToArray();
                    format = TextureFormat.BGRA32;
                    break;
                case IMAGE_FORMAT.BGR888:
                    List<byte> BGR888_bytes = VTF.data.Skip(VTF.data.Length - VTF.header.height * VTF.header.width * 24).ToList();
                    List<byte> BGRA8888_bytes = new List<byte>();
                    for (int i = 0; i < BGR888_bytes.Count; i += 3)
                    {
                        BGRA8888_bytes.AddRange(BGR888_bytes.GetRange(i, 3));
                        BGRA8888_bytes.Add((byte)255);
                    }
                    highResImage = BGRA8888_bytes.ToArray();
                    format = TextureFormat.BGRA32;
                    break;
                default:
                    throw new NotImplementedException(Parser.PATH + '\n' + VTF.header.highResImageFormat + " Format not supported.");
            }

            Texture2D texture = new Texture2D(VTF.header.width, VTF.header.height, format, false);
            texture.LoadRawTextureData(highResImage);
            texture.Apply();

            Shader shader = Shader.Find("VALVE/LightmappedGeneric");
            Material material = new Material(shader);
            material.name = name;
            material.mainTexture = texture;

            bool isEnvMapped = Parser.SearchKey("*/$envmap") != null;
            bool isSelfIllumed = Parser.SearchKey("*/$selfillum") != null;

            // (hasFlag(TEXTURE_FLAG.EIGHTBITALPHA) || hasFlag(TEXTURE_FLAG.ONEBITALPHA))
            // Alpha is Transparency ($env_map is a trick for masked illumination (alpha channel). Ignore it)
            if ((VTF.hasFlag(TEXTURE_FLAG.EIGHTBITALPHA) || VTF.hasFlag(TEXTURE_FLAG.ONEBITALPHA)) &&
                !isEnvMapped && !isSelfIllumed)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
            return material;
        }

        // Searches curent path for TROIKA files (.tth, .ttz)
        public string getTroikaPath()
        {
            string baseTexture = (string)Parser.SearchKey("*/$baseTexture");
            if (baseTexture != null)
            {
                string troikaPath = Application.dataPath + "/vampire/materials/" + baseTexture;
                if (File.Exists(troikaPath + ".tth") && File.Exists(troikaPath + ".ttz"))
                    return troikaPath;
                return null;
            }
            return null;
        }

        public object this[string key]
        {
            get => Parser.SearchKey(key);
        }

        // public bool Contains(string key) => this["*/$baseTexture"] == null ? false : true;
    }
}
