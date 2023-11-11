using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace CurlyUtility
{
    public static class MeshUtility
    {
        /// <summary>
        /// Combines vertex and triangle data into one mesh.
        /// </summary>
        public static Mesh BasicCombineMeshes(Mesh[] meshes)
        {
            List<Vector3> verticies = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uv = new List<Vector2>();

            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh cur = meshes[i];
                int triangleOffset = verticies.Count;

                verticies.AddRange(cur.vertices);
                uv.AddRange(cur.uv);

                int[] curTris = cur.triangles;

                //Modify triangle indicies to match with new vertex array
                for (int j = 0; j < curTris.Length; j++)
                {
                    curTris[j] += triangleOffset;
                }

                triangles.AddRange(curTris);
            }

            Mesh combine = new Mesh();
            combine.vertices = verticies.ToArray();
            combine.triangles = triangles.ToArray();
            combine.uv = uv.ToArray();

            return combine;
        }

        public static void CombineMeshes(Mesh[] meshes, Material[] materials, out Mesh combinedMesh, out Material[] combinedMaterials)
        {
            combinedMesh = null;
            combinedMaterials = null;

            if (meshes.Length != materials.Length) throw new ArgumentException("Mesh array and materials array are not of the same length");


            Dictionary<Material, List<Mesh>> meshesPerMaterial = new Dictionary<Material, List<Mesh>>();

            //Sort the meshes and the materials into keyvalue pairs, that way we can combine all the meshes of the same material
            for (int i = 0; i < meshes.Length; i++)
            {
                List<Mesh> value;
                if (meshesPerMaterial.ContainsKey(materials[i]))
                {
                    //The dictionary contains the material, add corresponding mesh to the dictionary
                    value = meshesPerMaterial[materials[i]];
                    value.Add(meshes[i]);
                    meshesPerMaterial[materials[i]] = value;
                }
                else
                {
                    value = new List<Mesh>();
                    value.Add(meshes[i]);
                    meshesPerMaterial.Add(materials[i], value);
                }
            }

            Debug.Log(meshesPerMaterial.Keys.ToArray().Length);

            if (meshesPerMaterial.Keys.ToArray().Length == 1)
            {
                //There is one material
                combinedMaterials = new Material[] { materials[0] };
                combinedMesh = BasicCombineMeshes(meshes);
                return;
            }

            List<Mesh> subMeshes = new List<Mesh>();
            //Materials are now sorted
            combinedMaterials = meshesPerMaterial.Keys.ToArray();

            //Combine all the meshes of the same material
            foreach (KeyValuePair<Material, List<Mesh>> pair in meshesPerMaterial)
            {
                subMeshes.Add(BasicCombineMeshes(pair.Value.ToArray()));
            }

            //Combine all the sortedMeshes into one mesh, but keep the submeshes in tact
            List<CombineInstance> totalMesh = new List<CombineInstance>();

            foreach (Mesh m in subMeshes)
            {
                CombineInstance c = new CombineInstance();
                c.mesh = m;
                totalMesh.Add(c);
            }

            combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(totalMesh.ToArray(), false, false);
        }

        /// <summary>
        /// Scales a mesh in accord to the origin.
        /// Providing no scale value will set the maxiumum scale of the mesh to 1.
        /// The new mesh will have bounds that will extend from (-scale, -scale, -scale) to (scale, scale, scale) at it's max size;
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Mesh ScaleMesh(Mesh mesh, float scale = 1)
        {
            //Find the vertex with the largest magnitude from the origin
            float largestMagnitude = float.MinValue;
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (mesh.vertices[i].magnitude > largestMagnitude) largestMagnitude = mesh.vertices[i].magnitude;
            }
            //Use this magnitude to scale each vertex
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 newVertex = (mesh.vertices[i] / largestMagnitude) * scale;
                mesh.vertices[i] = newVertex;
            }

            return mesh;
        }
    }
}