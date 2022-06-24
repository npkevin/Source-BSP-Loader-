using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VALVE;
using VALVE.Formats.BSP;
using UnityEngine;

/* TODOs:
 *   1. Complete core* map entities
 *   2. Find a solution for 3D SkyBoxs. Currently using a custom shader for Sky Material.
          - Look into Stencil Shaders?
 *   3. Why are some .VMTs in the form "maps/MAPNAME/materials/..._-XXX_XXX_-XXX"?
 *   4. Mip-Map Support
 */

namespace NPKEVIN.Utils
{
    class BSPTools
    {
        public float SOURCE_SCALE { get; private set; }

        public BSPTools(float scale = 0.0254f)
        {
            SOURCE_SCALE = scale;
        }

        public void GenerateMap(VALVE.BSP bsp, GameObject baseGameObject)
        {
            // For organizational purposes, Add models accordingly
            baseGameObject.name = bsp.MapName;
            GameObject go_funcDIR = new GameObject("func_*");
            GameObject go_trigDIR = new GameObject("trigger_*");
            GameObject go_envDIR = new GameObject("env_*");
            go_funcDIR.transform.parent = baseGameObject.transform;
            go_trigDIR.transform.parent = baseGameObject.transform;
            go_envDIR.transform.parent = baseGameObject.transform;

            // Entities
            // https://developer.valvesoftware.com/wiki/List_of_entities
            // https://developer.valvesoftware.com/wiki/List_of_VTMB_Entities
            // TODO:
            //   - Complete compatibility for core* Entities
            //   - Figure out which Entities are considered core :)
            foreach (Entity entity in bsp.entities)
            {
                GameObject go_entity = new GameObject();
                string classname = entity["classname"];
                go_entity.name = entity["targetname"] != null ? entity["targetname"] : classname;

                if (entity.Contains("origin"))
                {
                    float[] _origin = entity["origin"]
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(str => float.Parse(str))
                            .ToArray();
                    go_entity.transform.position = new Vector3(-_origin[1], _origin[2], _origin[0]) * SOURCE_SCALE;
                }

                if (classname == "worldspawn")
                {
                    go_entity.name = "worldspawn";
                    go_entity.transform.parent = baseGameObject.transform;
                    Mesh mesh_worldspawn = CreateModelMesh(bsp, bsp.models[0], go_entity);
                    mesh_worldspawn.name = "worldspawn";
                    continue;
                }

                // Sky Camera (3d SkyBox)
                if (classname == "sky_camera")
                {
                    go_entity.transform.parent = baseGameObject.transform;
                    go_entity.AddComponent<SkyCamera>();
                    continue;
                }

                // Lights
                if (classname.StartsWith("light"))
                {
                    // TODO: Ignore lights for now.
                    // Do lights have a function? On/Off? Are they just remnants from Hammer Editor?
                    GameObject.DestroyImmediate(go_entity);
                    continue;
                }
                // Props
                else if (classname.StartsWith("prop_"))
                {
                    MDL mdl = new MDL(Application.dataPath + "/vampire/" + entity["model"]);
                    Debug.Log(entity);

                    GameObject.DestroyImmediate(go_entity);
                    continue;
                }
                // Env's
                else if (classname.StartsWith("env_"))
                {
                    go_entity.transform.parent = go_envDIR.transform;
                    switch (classname)
                    {
                        // TODO: create a custom mesh for sprites
                        // or Use a custom shader (billboarding)
                        case "env_sprite":
                            // Copy position and name
                            GameObject go_sprite = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            go_sprite.transform.position = go_entity.transform.position;
                            go_sprite.transform.parent = go_entity.transform.parent;
                            go_sprite.name = go_entity.name;
                            GameObject.Destroy(go_entity);
                            GameObject.Destroy(go_sprite.GetComponent<Collider>());
                            if (entity.Contains("scale"))
                            {
                                float scale = float.Parse(entity["scale"]);
                                go_sprite.transform.localScale = Vector3.one * scale;
                            }

                            env_sprite sprite = go_sprite.AddComponent<env_sprite>();
                            sprite.entity = entity;

                            MeshRenderer mr_sprite = sprite.GetComponent<MeshRenderer>();
                            VMT vmt = new VMT(Application.dataPath + "/vampire/" + entity["model"]);
                            mr_sprite.material = vmt.GenerateUnityMaterial();
                            break;
                        default:
                            Debug.LogError("Env Entity not Implemented\n" + entity);
                            GameObject.DestroyImmediate(go_entity);
                            continue;
                    }
                }
                // Functions
                else if (classname.StartsWith("func_"))
                {
                    go_entity.transform.parent = go_funcDIR.transform;
                    switch (classname)
                    {
                        // "cloud rotator" & "lightning rotator"
                        case "func_rotating":
                            // func_rotating rotating = go_entity.AddComponent<func_rotating>();
                            // rotating.maxSpeed = float.Parse(entity["maxspeed"]);
                            // break;
                            Debug.LogError("Entity (" + classname + ") not Implemented\n" + entity);
                            GameObject.DestroyImmediate(go_entity);
                            continue;
                        case "func_door":
                            func_door door = go_entity.AddComponent<func_door>();
                            break;
                        case "func_door_rotating":
                            func_door_rotating rdoor = go_entity.AddComponent<func_door_rotating>();
                            rdoor.targetName = entity["targetname"];
                            rdoor.linkedDoor = entity["linked_door"] != null ? entity["linked_door"] : "";
                            rdoor.openAngle = float.Parse(entity["distance"]);
                            rdoor.rotationSpeed = float.Parse(entity["speed"]);
                            break;
                        case "func_brush":
                        case "func_illusionary":
                        case "func_lod":
                        case "func_elevator":
                            break;
                        default:
                            Debug.LogError("Func Entity not Implemented\n" + entity);
                            GameObject.DestroyImmediate(go_entity);
                            continue;
                    }
                    Mesh mesh_entity = CreateModelMesh(bsp, int.Parse(entity["model"].Replace("*", "")), go_entity);
                    mesh_entity.name = go_entity.name;
                }
                // Triggers
                else if (classname.StartsWith("trigger_"))
                {
                    go_entity.transform.parent = go_trigDIR.transform;
                    switch (classname)
                    {
                        case "trigger_once":
                        case "trigger_multiple":
                        case "trigger_changelevel":
                        case "trigger_hurt":
                        case "trigger_electric_bugaloo":
                            break;
                        default:
                            // Debug.LogError("Trigger Entity not Implemented\n" + entity);
                            // GameObject.DestroyImmediate(go_entity);
                            // continue;
                            break;
                    }
                    Mesh mesh_entity = CreateModelMesh(bsp, int.Parse(entity["model"].Replace("*", "")), go_entity);
                    mesh_entity.name = go_entity.name;

                    MeshCollider meshcol_entity = go_entity.GetComponent<MeshCollider>();
                    meshcol_entity.convex = true;
                    meshcol_entity.isTrigger = true;
                }
                else
                {
                    Debug.LogError("Entity (" + classname + ") not Implemented\n" + entity);
                    GameObject.DestroyImmediate(go_entity);
                }
            }
        }

        public Mesh CreateModelMesh(VALVE.BSP bsp, int model, GameObject go)
        {
            return CreateModelMesh(bsp, bsp.models[model], go);
        }

        public Mesh CreateModelMesh(VALVE.BSP bsp, Model model, GameObject go)
        {
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();

            Face[] faces = GetModelFaces(bsp, model);
            Dictionary<string, Tuple<int[], Material>> subMeshes = new Dictionary<string, Tuple<int[], Material>>();

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();

            // Each face is sorted into a SubMesh depending on it's vmtFilePath
            foreach (Face face in faces)
            {
                {/*
                Issue seems to have disapeared after rewriting vmtparser
                or maybe it was becuase textures were replaced?
                TODO:
                Why does sm_hub_2 have a firstEdge > sEdges.Length ?
                Also some faces have < 3 Edges
                NOTE: getting very large numEdges here. bug in code?
                */
                }
                if (face.firstEdge > bsp.surfaceEdges.Length || face.numEdges < 3)
                {
                    Debug.LogError(
                        "This face has issues." +
                        "\nface.firstEdge: " + face.firstEdge +
                        "\nsfEdge.Length: " + bsp.surfaceEdges.Length +
                        "\nface.numEdges: " + face.numEdges
                    );
                    continue;
                }

                // Add Vertices & UVs to parentMesh
                Vector3[] faceVerts = GetFaceVerts(bsp, face);
                int[] faceTris = GetFaceTris(face, verts.Count);

                string vmtPath = Application.dataPath + "/vampire/materials/" + GetVmtPath(bsp, face).ToLower();
                // Add this face's triangles to "SubMeshes"
                if (!subMeshes.ContainsKey(vmtPath))
                {
                    VALVE.VMT vmt = new VALVE.VMT(vmtPath);
                    Material material = vmt.GenerateUnityMaterial();
                    subMeshes.Add(vmtPath, new Tuple<int[], Material>(faceTris, material));
                }
                // Concat triangle indices of this face to the "SubMesh"
                else
                {
                    subMeshes[vmtPath] = new Tuple<int[], Material>(
                        subMeshes[vmtPath].Item1.Concat(faceTris).ToArray(),
                        subMeshes[vmtPath].Item2
                    );
                }
                verts.AddRange(faceVerts);
                UVs.AddRange(GetFaceUVs(bsp, face, faceVerts));
            }
            Mesh parentMesh = new Mesh();

            // Set mesh properties
            parentMesh.subMeshCount = subMeshes.Count;
            parentMesh.vertices = verts.ToArray();
            parentMesh.uv = UVs.ToArray();

            // Set Matrials and Indices for each SubMesh
            int matCount = 0;

            mr.materials = new Material[] { };
            foreach (var subMesh in subMeshes)
            {
                mr.materials = mr.materials.Concat(new Material[] { subMesh.Value.Item2 }).ToArray();
                parentMesh.SetTriangles(subMesh.Value.Item1, matCount);
                matCount++;
            }

            parentMesh.RecalculateNormals();
            mf.sharedMesh = parentMesh;
            mc.sharedMesh = parentMesh;
            return parentMesh;
        }

        private Face[] GetModelFaces(VALVE.BSP bsp, Model model)
        {
            return bsp.Reader.GetLump<Face>(
                LUMP.FACES,
                model.firstFace,
                model.numFaces
           );
        }

        private string GetVmtPath(VALVE.BSP bsp, Face face)
        {
            TexInfo texInfo = bsp.texInfos[face.texInfo];
            TexData texData = bsp.texDatas[texInfo.texData];
            int tableIndex = texData.nameStringTableID;
            int offset = bsp.strTable[tableIndex];

            // Uses caching to avoid excessive IO calls
            if (bsp.StrData.ContainsKey(offset))
                return bsp.StrData[offset];

            string texPath = bsp.Reader.GetTexDataString(offset);

            // TODO: Find a solution for VMT's with this format
            //  - Currently "cleaning" the path to a fallback texture?
            // 
            // NOTE:
            // This most likely points to inside the BSP(ie. "maps/MAPNAME/*/*_-XXX_XXX_-XXX")
            // VMTs inside the BSP have the shader: "patch" { include VMT_NAME, replace { ... } }

            // Clean texPath
            texPath = texPath.ToLower();
            texPath = texPath.Replace('\\', '/');
            texPath = texPath.TrimStart('/');
            if (texPath.Split('/')[0].Contains("maps"))
            {
                string textPath_old = texPath;
                texPath = texPath.Substring("maps/".Length + bsp.MapName.Length);
                texPath = Regex.Replace(texPath, @"(_-?\d+){3}$", "");
                // Debug.LogError("Renaming: " + textPath_old + "\n" + texPath);
            }

            texPath += ".vmt";
            bsp.StrData.Add(offset, texPath); // cache!
            return texPath;
        }

        // Credits: https://github.com/DeadZoneLuna
        private Vector2[] GetFaceUVs(VALVE.BSP bsp, Face face, Vector3[] vertices)
        {
            Vector2[] UVs = new Vector2[vertices.Length];

            TexInfo texInfo = bsp.texInfos[face.texInfo];
            TexData texData = bsp.texDatas[texInfo.texData];

            Vector3 tS = new Vector3(-texInfo.textureVecs[0].y, texInfo.textureVecs[0].z, texInfo.textureVecs[0].x);
            Vector3 tT = new Vector3(-texInfo.textureVecs[1].y, texInfo.textureVecs[1].z, texInfo.textureVecs[1].x);

            for (int i = 0; i < vertices.Length; i++)
            {
                float textureUVS = (Vector3.Dot(vertices[i], tS) / SOURCE_SCALE + texInfo.textureVecs[0].w) / (texData.viewWidth);
                float textureUVT = (Vector3.Dot(vertices[i], tT) / SOURCE_SCALE + texInfo.textureVecs[1].w) / (texData.viewHeight);

                UVs[i] = new Vector2(textureUVS, textureUVT);
            }
            return UVs;
        }

        private Vector3[] GetFaceVerts(VALVE.BSP bsp, Face face)
        {
            Vector3[] faceVertices = new Vector3[face.numEdges];
            for (int i = face.firstEdge; i < face.firstEdge + face.numEdges; i++)
            {
                Edge edge = bsp.edges[bsp.surfaceEdges[i].index];
                // FWD or REV doesn't really matter. As long as we aren't repeating Vertices
                Vector3 Vertex = bsp.surfaceEdges[i].direction == SURFACE_EDGE_DIRECTION.REV ? bsp.vertices[edge.vectors[0]] : bsp.vertices[edge.vectors[1]];

                // Corrected Scaling and Rotation
                faceVertices[i - face.firstEdge] = new Vector3(-Vertex.y, Vertex.z, Vertex.x) * SOURCE_SCALE;
            }
            return faceVertices;
        }

        private int[] GetFaceTris(Face face, int offset)
        {
            // Vertices are already in clockwise order
            int[] indices = new int[(face.numEdges - 2) * 3];
            for (int i = 0; i < face.numEdges - 2; i++)
            {
                indices[3 * i] = offset;
                indices[3 * i + 1] = offset + 1 + i;
                indices[3 * i + 2] = offset + 2 + i;
            }
            return indices;
        }
    }
}