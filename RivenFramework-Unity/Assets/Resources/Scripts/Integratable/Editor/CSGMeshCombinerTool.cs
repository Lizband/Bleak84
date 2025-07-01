using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Used to convert CSG level meshes into one object, so it's compatible with CorGeo's mesh slicing
/// </summary>
public static class CSGMeshCombinerTool
{
    [MenuItem("Neverway/CorGeo/Combine and bake level mesh")]
    public static void CombineLevelMeshes()
    {
        GameObject meshGroupRoot = GameObject.Find("MeshGroup");
        if (!meshGroupRoot)
        {
            Debug.LogError("No GameObject named 'MeshGroup' found in scene");
            return;
        }

        var materialMeshTransforms = new List<Transform>();

        foreach (Transform child in meshGroupRoot.transform)
        {
            if (child.name == "MaterialMesh")
                materialMeshTransforms.Add(child);
        }

        if (materialMeshTransforms.Count == 0)
        {
            Debug.LogError("No 'MaterialMesh' children found under 'MeshGroup'");
            return;
        }

        Debug.Log($"Combining {materialMeshTransforms.Count} MaterialMesh objects");

        var uniqueMaterials = new List<Material>();
        var materialToCombineInstances = new Dictionary<Material, List<CombineInstance>>();

        // 1. Group meshes by material
        foreach (Transform materialMeshTransform in materialMeshTransforms)
        {
            MeshFilter meshFilter = materialMeshTransform.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = materialMeshTransform.GetComponent<MeshRenderer>();
            if (!meshFilter || !meshRenderer || !meshFilter.sharedMesh) continue;

            Mesh mesh = meshFilter.sharedMesh;

            for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
            {
                Material material = meshRenderer.sharedMaterials[submeshIndex];
                if (!materialToCombineInstances.ContainsKey(material))
                {
                    materialToCombineInstances[material] = new List<CombineInstance>();
                    uniqueMaterials.Add(material);
                }

                var combineInstance = new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = submeshIndex,
                    transform = meshFilter.transform.localToWorldMatrix
                };
                materialToCombineInstances[material].Add(combineInstance);
            }
        }

        // 2. Combine meshes by material (one per submesh)
        List<Mesh> materialGroupedMeshes = new List<Mesh>();
        foreach (var materialEntry in materialToCombineInstances)
        {
            var groupedMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            groupedMesh.CombineMeshes(materialEntry.Value.ToArray(), true, true);
            materialGroupedMeshes.Add(groupedMesh);
        }

        // 3. Combine into one mesh with multiple submeshes
        Mesh combinedMesh = new Mesh
        {
            name = "CombinedLevelMesh",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        var combinedVertices = new List<Vector3>();
        var combinedUVs = new List<Vector2>();
        var combinedSubmeshTriangles = new List<int[]>();
        int vertexOffset = 0;

        foreach (var groupedMesh in materialGroupedMeshes)
        {
            Vector3[] vertices = groupedMesh.vertices;
            Vector2[] uvs = groupedMesh.uv;
            int[] triangles = groupedMesh.GetTriangles(0);

            for (int i = 0; i < triangles.Length; i++)
                triangles[i] += vertexOffset;

            combinedVertices.AddRange(vertices);
            combinedUVs.AddRange(uvs);
            combinedSubmeshTriangles.Add(triangles);
            vertexOffset += vertices.Length;
        }

        combinedMesh.subMeshCount = combinedSubmeshTriangles.Count;
        combinedMesh.SetVertices(combinedVertices);
        combinedMesh.SetUVs(0, combinedUVs);
        for (int i = 0; i < combinedSubmeshTriangles.Count; i++)
            combinedMesh.SetTriangles(combinedSubmeshTriangles[i], i);
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        // 4. Create new GameObject to hold final mesh
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name == "CombinedLevelMesh" && obj.scene.IsValid())
            {
                GameObject.DestroyImmediate(obj);
            }
        }

        GameObject combinedMeshObject = new GameObject("CombinedLevelMesh");
        //combinedMeshObject.transform.parent = meshGroupRoot.transform;
        combinedMeshObject.transform.localPosition = Vector3.zero;
        combinedMeshObject.transform.localRotation = Quaternion.identity;
        combinedMeshObject.transform.localScale = Vector3.one;

        MeshFilter meshFilterFinal = combinedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRendererFinal = combinedMeshObject.AddComponent<MeshRenderer>();
        MeshCollider meshColliderFinal = combinedMeshObject.AddComponent<MeshCollider>();

        // Add custom slicable component
        //combinedMeshObject.AddComponent<Mesh_Sliceable>();

        meshFilterFinal.sharedMesh = combinedMesh;
        meshRendererFinal.sharedMaterials = uniqueMaterials.ToArray();
        meshColliderFinal.sharedMesh = combinedMesh;
        meshColliderFinal.convex = false;
        
        // 5. Disable all the old level mesh data
        for (int i = 0; i < meshGroupRoot.transform.childCount; i++)
        {
            meshGroupRoot.transform.GetChild(i).gameObject.SetActive(false);
        }

        Debug.Log($"CombinedLevelMesh created with {combinedMesh.subMeshCount} submeshes");
    }
}
