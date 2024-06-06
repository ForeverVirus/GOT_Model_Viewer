using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MeshReader
{
    public class MeshData
    {
        public enum ReadType
        {
            None,
            Triangles,
            Verticles,
            Normals,
            Uvs,
        }
        public Vector3[] verticles;
        public List<int> triangles;
        public Vector3[] normals;
        public List<Vector2> uvs;
        public int maxTriangleIndex = -1;
        public ReadType readType = ReadType.None;
        public int TriangleStartIndex = -1;
        public int verticlePadding = 2;
        public int normalPadding = 2;
        public bool isHide = false;
        public int triangleToVerticleSkipByte = 0;
    }

    public static MeshReader Instance = new MeshReader();

    bool CheckIsTriangle(BinaryReader br)
    {
        var triIndex = br.ReadUInt16();
        if (triIndex == 0)
        {
            var nextTriIndex = br.ReadUInt16();
            if (nextTriIndex == 1)
            {
                var thirdTriIndex = br.ReadInt16();
                if (thirdTriIndex == 2)
                {
                    return true;
                    // meshData.TriangleStartIndex = readIndex;
                    // continue;
                }
                else
                {
                    br.BaseStream.Position -= 5;
                }
            }
            else
            {
                br.BaseStream.Position -= 3;
            }
        }
        else
        {
            br.BaseStream.Position -= 1;
        }

        return false;
    }
    
    // Update is called once per frame
    public List<MeshData> ReadMesh(string path)
    {
        BinaryReader br;
        try
        {
            br = new BinaryReader(new FileStream(path,
                FileMode.Open));
        }
        catch (IOException e)
        {
            Debug.LogError("Load Error " + e.Message);
            return null;
        }

        List<MeshData> meshs = new List<MeshData>();
        MeshData meshData = null;
        try
        {
            bool isCreatingMesh = false;
            while (true)
            {
                if (meshData == null)
                {
                    var triIndex = br.ReadUInt16();
                    if (triIndex == 0 && !isCreatingMesh)
                    {
                        var triangleStartIndex = br.BaseStream.Position - 2;
                        var nextTriIndex = br.ReadUInt16();
                        if (nextTriIndex == 1)
                        {
                            var thirdTriIndex = br.ReadInt16();
                            if (thirdTriIndex == 2)
                            {
                                isCreatingMesh = true;
                                meshData = new MeshData();
                                meshData.triangles = new List<int>();
                                meshData.triangles.Add(triIndex);
                                meshData.triangles.Add(nextTriIndex);
                                meshData.triangles.Add(thirdTriIndex);
                                meshData.maxTriangleIndex = 2;
                                meshData.readType = MeshData.ReadType.Triangles;
                                meshData.TriangleStartIndex = (int)triangleStartIndex;
                                continue;
                            }
                            else
                            {
                                br.BaseStream.Position -= 5;
                            }
                        }
                        else
                        {
                            br.BaseStream.Position -= 3;
                        }

                        triangleStartIndex = -1;
                    }
                    else
                    {
                        br.BaseStream.Position -= 1;
                    }
                }
                else
                {
                    if (meshData.readType == MeshData.ReadType.Triangles)
                    {
                        var newTriandleIndex0 = br.ReadUInt16();
                        var newTriandleIndex1 = br.ReadUInt16();
                        var newTriandleIndex2 = br.ReadUInt16();
                        if (!CheckTriangleEnd(newTriandleIndex0, newTriandleIndex1, newTriandleIndex2,
                                meshData.maxTriangleIndex))
                        {
                            if (newTriandleIndex0 > meshData.maxTriangleIndex)
                            {
                                meshData.maxTriangleIndex = newTriandleIndex0;
                            }

                            if (newTriandleIndex1 > meshData.maxTriangleIndex)
                            {
                                meshData.maxTriangleIndex = newTriandleIndex1;
                            }

                            if (newTriandleIndex2 > meshData.maxTriangleIndex)
                            {
                                meshData.maxTriangleIndex = newTriandleIndex2;
                            }

                            meshData.triangles.Add(newTriandleIndex0);
                            meshData.triangles.Add(newTriandleIndex1);
                            meshData.triangles.Add(newTriandleIndex2);
                            continue;
                        }
                        else
                        {
                            meshData.readType = MeshData.ReadType.Verticles;
                            short verticleX = 0;
                            short verticleY = 0;
                            short verticleZ = 0;
                            if (newTriandleIndex0 == 0)
                            {
                                //sometimes has a 0x00 between triangles and verticles data 
                                verticleX = (short)newTriandleIndex1;
                                verticleY = (short)newTriandleIndex2;
                                verticleZ = br.ReadInt16();
                                meshData.triangleToVerticleSkipByte = 1;
                            }
                            else
                            {
                                verticleX = (short)newTriandleIndex0;
                                verticleY = (short)newTriandleIndex1;
                                verticleZ = (short)newTriandleIndex2;
                            }

                            var fX = (verticleX * Mathf.Pow(2, -8));
                            var fY = (verticleY * Mathf.Pow(2, -8));
                            var fZ = (verticleZ * Mathf.Pow(2, -8));

                            Vector3 verticle = new Vector3(fX, fY, fZ);
                            meshData.verticles = new Vector3[meshData.maxTriangleIndex + 1];
                            meshData.normals = new Vector3[meshData.maxTriangleIndex + 1];
                            meshData.uvs = new List<Vector2>();
                            meshData.verticles[0] = verticle;

                            for (int i = 0; i < meshData.verticlePadding; i++)
                            {
                                br.ReadByte();
                            }

                            continue;
                        }
                    }
                    else if (meshData.readType == MeshData.ReadType.Verticles)
                    {
                        for (int i = 1; i < meshData.verticles.Length; i++)
                        {
                            var verticleX = br.ReadInt16();
                            var verticleY = br.ReadInt16();
                            var verticleZ = br.ReadInt16();
                            var fX = (verticleX * Mathf.Pow(2, -8));
                            var fY = (verticleY * Mathf.Pow(2, -8));
                            var fZ = (verticleZ * Mathf.Pow(2, -8));
                            Vector3 verticle = new Vector3(fX, fY, fZ);
                            meshData.verticles[i] = verticle;
                            for (int j = 0; j < meshData.verticlePadding; j++)
                            {
                                br.ReadByte();
                            }
                        }

                        meshData.readType = MeshData.ReadType.Normals;
                        continue;
                    }
                    else if (meshData.readType == MeshData.ReadType.Normals)
                    {
                        for (int i = 0; i < meshData.normals.Length; i++)
                        {
                            var normalX = br.ReadInt16();
                            var normalY = br.ReadInt16();
                            var normalZ = br.ReadInt16();
                            var fX = (normalX * Mathf.Pow(2, -8));
                            var fY = (normalY * Mathf.Pow(2, -8));
                            var fZ = (normalZ * Mathf.Pow(2, -8));
                            Vector3 normal = new Vector3(fX, fY, fZ);
                            meshData.normals[i] = normal;
                            for (int j = 0; j < meshData.normalPadding; j++)
                            {
                                br.ReadByte();
                            }
                        }

                        meshData.readType = MeshData.ReadType.Uvs;
                        continue;
                    }
                    else if (meshData.readType == MeshData.ReadType.Uvs)
                    {
                        var readFirst = br.ReadUInt16();

                        if (readFirst == 0)
                        {
                            var second = br.ReadUInt16();
                            if (second == 1)
                            {
                                var third = br.ReadUInt16();
                                if (third == 2)
                                {
                                    meshData.readType = MeshData.ReadType.Triangles;

                                    meshs.Add(meshData);
                                    isCreatingMesh = false;
                                    meshData = null;
                                    
                                    br.BaseStream.Position -= 6;
                                    continue;
                                }
                                else
                                {
                                    br.BaseStream.Position -= 6;
                                }
                            }
                            else
                            {
                                br.BaseStream.Position -= 4;
                            }
                        }
                        else
                        {
                            br.BaseStream.Position -= 2;
                        }
                        
                        var uvX = Mathf.HalfToFloat(br.ReadUInt16());
                        var uvY = Mathf.HalfToFloat(br.ReadUInt16());
                        Vector2 uv = new Vector2(uvX, uvY);
                        meshData.uvs.Add(uv);
                        continue;
                    }
                }
            }
        }
        catch (IOException e)
        {
            Debug.Log("Read End ");
        }
        finally
        {
            br.Close();
            br.Dispose();
        }

        if (meshData != null)
        {
            meshs.Add(meshData);
        }
        
        
        return meshs;
    }

    bool CheckTriangleEnd(ushort index0, ushort index1, ushort index2, int maxIndex)
    {
        var max = maxIndex + 3;
        if (index0 > max || index1 > max || index2 > max)
        {
            return true;
        }

        return false;
    }

    public void SaveMesh(string path, string fileName, string orinalFileFullPath, List<MeshData> data)
    {
        var bytes = System.IO.File.ReadAllBytes(orinalFileFullPath);
        foreach (var item in data)
        {
            if (item.isHide)
            {
                var startIndex = item.TriangleStartIndex;
                var hideCount = CalculateHideCount(item);
                for (int i = startIndex; i < startIndex + hideCount; i++)
                {
                    bytes[i] = 0;
                }
            }
        }
        
        System.IO.File.WriteAllBytes(path + "\\" + fileName, bytes);
    }

    int CalculateHideCount(MeshData data)
    {
        var triangleByteCount = data.triangles.Count * 2 + data.triangleToVerticleSkipByte;
        var verticleByteCount = data.verticles.Length * 3 * 2 + data.verticles.Length * data.verticlePadding;
        var normalByteCount = data.normals.Length * 3 * 2 + data.normals.Length * data.normalPadding;
        var uvByteCount = data.uvs.Count * 2 * 2;

        return triangleByteCount + verticleByteCount + normalByteCount + uvByteCount;
        // var verticalByteCount
    }
}
