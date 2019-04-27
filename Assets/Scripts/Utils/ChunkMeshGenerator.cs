using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshGenerator
{
    ArrayList Materials = new ArrayList();
    ArrayList CombineInstances = new ArrayList();

    public void AddBlock(int x, int y, int z, Vector3 position)
    {
        ChunkBlock block = WorldRoot.BlockCache[x, y, z];

        if (block.CurrentBlock == BlockType.BLOCK_AIR) return;

        GameObject blockToAdd = BlockManager.main.GetBlock(block.CurrentBlock);

        foreach (Transform child in blockToAdd.transform)
        {
            if (!ShouldCombinePart(child.name, x, y, z)) continue;

            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            int materialArrayIndex = Contains(Materials, meshRenderer.sharedMaterial.name);
            if (materialArrayIndex == -1)
            {
                Materials.Add(meshRenderer.sharedMaterial);
                materialArrayIndex = Materials.Count - 1;
            }
            CombineInstances.Add(new ArrayList());

            CombineInstance combineInstance = new CombineInstance();

            combineInstance.transform = Matrix4x4.TRS(position + child.localPosition, meshFilter.transform.rotation, new Vector3(1, 1, 1));

            combineInstance.subMeshIndex = 0;
            combineInstance.mesh = meshFilter.sharedMesh;
            (CombineInstances[materialArrayIndex] as ArrayList).Add(combineInstance);
        }
    }

    public void Combine(GameObject parent)
    {
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

        Mesh[] meshes = new Mesh[Materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[Materials.Count];

        for (int m = 0; m < Materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (CombineInstances[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
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

        Material[] materialsArray = Materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;
    }

    private bool ShouldCombinePart(string name, int x, int y, int z)
    {
        ChunkBlock compareBlock;
        switch (name)
        {
            case "Top":
                compareBlock = WorldRoot.main.GetBlock(x, y + 1, z);
                break;
            case "Bottom":
                compareBlock = WorldRoot.main.GetBlock(x, y - 1, z);
                break;
            case "Right":
                compareBlock = WorldRoot.main.GetBlock(x, y, z - 1);
                break;
            case "Left":
                compareBlock = WorldRoot.main.GetBlock(x, y, z + 1);
                break;
            case "Front":
                compareBlock = WorldRoot.main.GetBlock(x - 1, y, z);
                break;
            case "Back":
                compareBlock = WorldRoot.main.GetBlock(x + 1, y, z);
                break;
            default:
                compareBlock = null;
                break;
        }

        if (compareBlock != null)
        {
            return compareBlock.NextBlock == BlockType.BLOCK_AIR;
        }

        return true;
    }

    private int Contains(ArrayList searchList, string searchName)
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
