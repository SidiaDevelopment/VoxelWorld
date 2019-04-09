using System.Collections;
using UnityEngine;

public class MeshCombiner
{
    public static void combineMeshWithMaterials(GameObject[,,] existingVoxels, GameObject parent)
    {
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        // Get all voxel sides
        ArrayList voxels = new ArrayList();

        foreach (GameObject voxel in existingVoxels)
        {
            if (!voxel) continue;
            Block blockScript = voxel.GetComponent<Block>();
            if (!blockScript.ShouldCombine) continue;

            foreach (Transform child in voxel.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                MeshFilter meshFilter = child.GetComponent<MeshFilter>();

                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                //meshRenderer.gameObject.SetActive(false);

                if (!meshRenderer || !meshFilter.sharedMesh || meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
                {
                    continue;
                }

                // Add meshes to list
                for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
                {
                    int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                    if (materialArrayIndex == -1)
                    {
                        materials.Add(meshRenderer.sharedMaterials[s]);
                        materialArrayIndex = materials.Count - 1;
                    }
                    combineInstanceArrays.Add(new ArrayList());

                    CombineInstance combineInstance = new CombineInstance();

                    Vector3 getQuadLocalPositionFromParent = parent.transform.InverseTransformPoint(meshFilter.transform.position);
                    combineInstance.transform = Matrix4x4.TRS(getQuadLocalPositionFromParent, meshFilter.transform.rotation, new Vector3(1, 1, 1));

                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = meshFilter.sharedMesh;
                    (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
                }
            }
        }



        // Create objects for the combined mesh
        MeshFilter meshFilterCombine = parent.GetComponent<MeshFilter>();
        if (meshFilterCombine == null)
        {
            meshFilterCombine = parent.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRendererCombine = parent.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null)
        {
            meshRendererCombine = parent.AddComponent<MeshRenderer>();
        }

        // Combine meshes by material
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);
            meshes[m].RecalculateBounds();
            meshes[m].RecalculateNormals();

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine material meshes to one mesh
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);
        meshFilterCombine.sharedMesh.RecalculateBounds();
        meshFilterCombine.sharedMesh.RecalculateNormals();

        MeshCollider meshCollider = meshFilterCombine.gameObject.GetComponent<MeshCollider>();
        if (!meshCollider)
        {
            meshCollider = meshFilterCombine.gameObject.AddComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = meshFilterCombine.sharedMesh;

        // Cleanup
        foreach (Mesh oldMesh in meshes)
        {
            oldMesh.Clear();
            GameObject.DestroyImmediate(oldMesh);
        }

        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        foreach (GameObject meshFilter in voxels)
        {
            GameObject.DestroyImmediate(meshFilter);
        }
    }

    private static int Contains(ArrayList searchList, string searchName)
    {
        for (int i = 0; i < searchList.Count; i++)
        {
            if (((Material)searchList[i]).name == searchName)
            {
                return i;
            }
        }

        return -1;
    }
}
