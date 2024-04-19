using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChunckMain;

[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObject/Block", order = 0)]
public class SOBlock : ScriptableObject
{
    public Sprite previewImg;
    public Vector3 planSize = Vector3.one;
    public GameObject prefab;
    public Vector3 meshSize;
    public Vector3 meshToPlanScale;
    public MeshFilter[] meshFilters;

    [ContextMenu("获取参数")]
    private void GetParam()
    {
        if (prefab != null)
        {
            Bounds bounds = new Bounds();
            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
            if (meshFilters != null)
            {
                foreach (MeshFilter mf in meshFilters)
                {
                    foreach (Vector3 point in mf.sharedMesh.vertices)
                    {
                        bounds.Encapsulate(point);
                    }
                }
                meshSize = bounds.size;
                meshToPlanScale = new Vector3(planSize.x / meshSize.x, planSize.y / meshSize.y, planSize.z / meshSize.z);
            }
        }
    }

    [ContextMenu("实例化测试")]
    public GameObject InstantiateBlock() => InstantiateBlock(Vector3.zero, Vector3.zero, null);
    public GameObject InstantiateBlock(Vector3 centerPos, float yRotate, Transform parent = null) => InstantiateBlock(centerPos, new Vector3(0, yRotate, 0), parent);
    public GameObject InstantiateBlock(Vector3 centerPos, Vector3 rotate, Transform parent = null)
    {
        GameObject obj = null;
        if (prefab != null)
        {
            obj = GameObject.Instantiate(prefab);
            obj.transform.position = centerPos;
            obj.transform.localScale = meshToPlanScale;
            obj.transform.rotation = Quaternion.Euler(rotate);
            obj.transform.SetParent(parent);
        }
        return obj;
    }

    [ContextMenu("获取网格过滤器")]
    private void GetMeshFilters()
    {
        if (prefab != null)
        {
            meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
#if UNITY_EDITOR
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (!meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial.enableInstancing)
                {
                    Debug.Log($"{prefab.name}的材质未开启:GPU Instancing,已给予开启。");
                    meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial.enableInstancing = true;
                }
            }
#endif
        }
    }

    /// <summary> 渲染网格 </summary>
    public void InstancingMesh(Vector3 center, Vector3 rotate, Material material = null)
    {
        Matrix4x4 mat;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            mat = Matrix4x4.TRS(
                    // 自中心偏移 向量
                    center + Quaternion.Euler(rotate) * meshFilters[i].transform.localPosition,
                    // 加旋自带的旋转角
                    Quaternion.Euler(rotate) * meshFilters[i].transform.localRotation,
                    meshToPlanScale
                    );
            Graphics.DrawMesh(meshFilters[i].sharedMesh,
                mat,
                material == null ? meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial : material,
                0); // 即Default层
            Debug.Log($"Sources: center:{center}" +
                $"\nPosition:{mat.GetPosition()} Rotate:{mat.rotation.eulerAngles} Scale:{mat.lossyScale}");
        }
    }
}
