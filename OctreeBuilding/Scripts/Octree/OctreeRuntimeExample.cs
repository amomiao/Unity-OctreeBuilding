using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeStruct.Oct.Example
{
    //public class OctreeRuntimeExample : MonoBehaviour
    //{
    //    public class Block
    //    {
    //        GameObject cube;
    //        public Block() { }
    //        public Block(GameObject cube) { this.cube = cube; }

    //        public GameObject Cube { get => cube; set => cube = value; }
    //    }

    //    public static OctreeRuntimeExample _instance;
    //    private void Awake() => _instance = this;

    //    private OctreeRoot<Block> tree;
    //    private new Camera camera;

    //    private LineRenderer mLineRender;
    //    private Vector3 mMousePos;
    //    private OctreeNode<Block> mNode;
    //    private float mCheckLength = 15;

    //    void Start()
    //    {
    //        tree = new OctreeRoot<Block>(Vector3.zero, 1, new Block(GameObject.CreatePrimitive(PrimitiveType.Cube)));
    //        camera = Camera.main;
    //        mLineRender = GetComponent<LineRenderer>();
    //        mLineRender.enabled = true;
    //    }

    //    public OctreeNode<Block> CreateNode(Vector3 pos)
    //    {
    //        OctreeNode<Block> node = tree.TryCreateNode(pos, null);
    //        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        cube.transform.position = node.center;
    //        node.data = new Block();
    //        return node;
    //    }

    //    private void Update()
    //    {
    //        mMousePos = camera.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * mCheckLength);
    //        mLineRender.SetPosition(0, camera.transform.position);
    //        mLineRender.SetPosition(1, mMousePos);
    //        //mNode = tree.TryRayGet(camera.transform.position, (mMousePos - camera.transform.position).normalized, mCheckLength);
    //        mNode = tree.IntersectRay(new Ray(camera.transform.position, (mMousePos - camera.transform.position).normalized), mCheckLength);
    //        //if (mNode != null) Debug.Log(mNode.center);
    //        PlaceToBlock();
    //        DelectToBlock();
    //    }

    //    private void PlaceToBlock()
    //    {
    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            OctreeNode<Block> node;
    //            tree.Place(new Ray(camera.transform.position, (mMousePos - camera.transform.position).normalized), mCheckLength, null, out node);
    //            if (node != null)
    //            {
    //                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //                cube.transform.position = node.center;
    //                node.data = new Block(cube);
    //            }
    //        }
    //    }
    //    private void DelectToBlock()
    //    {
    //        if (Input.GetMouseButtonDown(1))
    //        {
    //            tree.Delect(new Ray(camera.transform.position, (mMousePos - camera.transform.position).normalized), mCheckLength, out Block data, out _);
    //            GameObject.Destroy(data.Cube);
    //        }
    //    }


    //    private void OnDrawGizmos()
    //    {
    //        // 绘制树
    //        Gizmos.color = Color.green;
    //        tree?.DrawGizoms();
    //        // 绘制选中节点
    //        if (mNode != null)
    //        {
    //            Gizmos.color = Color.blue;
    //            Gizmos.DrawWireCube(mNode.center, mNode.size * 1.1f);
    //        }
    //        Gizmos.color = Color.white;
    //    }
    //}
}
