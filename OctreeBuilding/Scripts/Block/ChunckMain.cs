using System.Collections;
using System.Collections.Generic;
using TreeStruct.Oct;
using UnityEngine;

public class ChunckMain : MonoBehaviour, ICommand<ChunckMain.Block>, IDataRotateCommand<ChunckMain.Block>//, IAddEventListeners
{
    private enum InputEvent
    {
        Place,
        Delect,
        LeftRotate,
        RightRotate,
        ResetRotate
    }

    public class Block : IAreaData<Block>
    {
        GameObject block;
        public Block() { }
        public Block(GameObject block, Vector3 size, Vector3 rotate)
        {
            this.block = block;
            Size = size;
            Rotate = rotate;
        }
        public GameObject BK { get => block; set => block = value; }
        public OctreeNode<Block> MainNode { get; set; }
        public Vector3 Size { get; }
        public Vector3 Rotate { get; }

        public void OnDestroy()
        {
            GameObject.Destroy(block);
        }
    }

    public static ChunckMain _instance;
    private void Awake() => _instance = this;

    public SOBlock soBlock;
    public float checkLength = 100;
    public Transform selectedAreaTip;
    public Transform createdAreaTip;
    private OctreeAreaRoot<Block> tree;
    private new Camera camera;

    private bool mDrawLine = false; // 开启可视化射线
    private LineRenderer mLineRender;
    private Vector3 mMousePos;
    private OctreeNode<Block> mSelectedNode;
    private OctreeNode<Block> mCreatedNode;
    private float mNowYRotate = 0;
    private bool mIsPlaceable = false;
    private bool mTempReverse = false;

    #region interface
    // ICommand
    public void Place(OctreeNode<Block> operNode)
    {
        if (operNode != null)
        {
            GameObject block = soBlock.InstantiateBlock(
                tree.GetAreaCenter(operNode.center, soBlock.planSize, mNowYRotate),
                mNowYRotate);
            tree.Place(
                operNode,
                new Block(block, soBlock.planSize, new Vector3(0, mNowYRotate, 0)));
        }
    }
    public void Delect(OctreeNode<Block> operNode)
    {
        if (operNode != null)
        {
            tree.Delect(operNode, out Block data);
        }
    }
    // IDataRotateCommand
    public void LeftRotate() => mNowYRotate = (mNowYRotate - 90) % 360;
    public void RightRotate() => mNowYRotate = (mNowYRotate + 90) % 360;
    public void ResetRotate() => mNowYRotate = 0;
    // IAddEventListeners
    //public void AddEventListeners()
    //{
    //    InputMgr.GetInstance().ChangeMouseInfo(InputEvent.Place, 0, InputInfo.E_InputType.Down);
    //    InputMgr.GetInstance().ChangeMouseInfo(InputEvent.Delect, 1, InputInfo.E_InputType.Down);
    //    InputMgr.GetInstance().ChangeKeyboardInfo(InputEvent.LeftRotate, KeyCode.Q, InputInfo.E_InputType.Down);
    //    InputMgr.GetInstance().ChangeKeyboardInfo(InputEvent.RightRotate, KeyCode.E, InputInfo.E_InputType.Down);
    //    InputMgr.GetInstance().ChangeKeyboardInfo(InputEvent.ResetRotate, KeyCode.R, InputInfo.E_InputType.Down);
    //    EventCenter.GetInstance().AddEventListener(InputEvent.Place, PlaceToBlock);
    //    EventCenter.GetInstance().AddEventListener(InputEvent.Delect, DelectToBlock);
    //    EventCenter.GetInstance().AddEventListener(InputEvent.LeftRotate, LeftRotate);
    //    EventCenter.GetInstance().AddEventListener(InputEvent.RightRotate, RightRotate);
    //    EventCenter.GetInstance().AddEventListener(InputEvent.ResetRotate, ResetRotate);
    //}
    #endregion interface
    private void InputListeners()   // 输入事件监听
    {
        if (Input.GetMouseButtonDown(0)) PlaceToBlock();
        if (Input.GetMouseButtonDown(1)) DelectToBlock();
        if (Input.GetKeyDown(KeyCode.Q)) LeftRotate();
        if (Input.GetKeyDown(KeyCode.E)) RightRotate();
        if (Input.GetKeyDown(KeyCode.R)) ResetRotate();
    }

    // Life Function
    void Start()
    {
        Vector3 pos;
        GameObject origin = soBlock.InstantiateBlock();
        tree = new OctreeAreaRoot<Block>(new Block(origin, soBlock.planSize, Vector3.zero), 1, out pos);
        origin.transform.position = pos;
        camera = Camera.main;
        if (mDrawLine)
        {
            mLineRender = GetComponent<LineRenderer>();
            mLineRender.enabled = true;
        }
        //AddEventListeners();
    }
    private void Update()
    {
        mMousePos = camera.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * checkLength);
        if (mDrawLine)
        {
            mLineRender.SetPosition(0, camera.transform.position);
            mLineRender.SetPosition(1, mMousePos);
        }
        mSelectedNode = tree.IntersectRay(new Ray(camera.transform.position, (mMousePos - camera.transform.position).normalized), checkLength);
        if (mSelectedNode != null)
        {
            DrawSelectedFrame(mSelectedNode);
            mCreatedNode = tree.IntersectRayDirNode(new Ray(camera.transform.position, (mMousePos - camera.transform.position).normalized), checkLength);
            DrawCreatedAreaTip(mCreatedNode);
        }
        else
        {
            selectedAreaTip.gameObject.SetActive(false);
            createdAreaTip.gameObject.SetActive(false);
            mCreatedNode = null;
        }
        InputListeners();
    }

    #region 绘制选中预览
    private void DrawSelectedFrame(OctreeNode<Block> node)
    {
        if (node != null)
        {
            selectedAreaTip.gameObject.SetActive(true);
            if (node.data == null && node.data.MainNode != null)
            {
                selectedAreaTip.position = mSelectedNode.center;
                selectedAreaTip.localScale = Vector3.one;
            }
            else
            {
                selectedAreaTip.position = tree.GetAreaCenter(node.data.MainNode.center, node.data.Size, node.data.Rotate);
                selectedAreaTip.localScale = node.data.Size;
            }
        }
    }
    #endregion 绘制选中预览

    #region 绘制创建预览
    /// <summary> 绘制创建预览 Draw Create Preview </summary>
    private void DrawCreatedAreaTip(OctreeNode<Block> node)
    {
        if (node != null)
        {
            // 重置临时旋转
            if (mTempReverse)
            {
                // 回转两次
                ResetTempRotate();
                mTempReverse = false;
            }
            // 是否合法
            if (tree.AreaCheck(node, soBlock.planSize, new Vector3(0, mNowYRotate, 0)))
            {
                PlaceableDo();
            }
            else
            {
                // 旋转角反转试试
                if (!mTempReverse)
                {
                    // 回转两次
                    SetTempRotate();
                    if (tree.AreaCheck(node, soBlock.planSize, new Vector3(0, mNowYRotate, 0)))
                    {
                        PlaceableDo();
                        mTempReverse = true;
                    }
                    else
                    {
                        ResetTempRotate();
                        UnplaceableDo();
                    }
                }
                else
                {
                    UnplaceableDo();
                }
            }
            // 框
            createdAreaTip.gameObject.SetActive(true);
            createdAreaTip.position = tree.GetAreaCenter(mCreatedNode.center, soBlock.planSize, mNowYRotate);
            createdAreaTip.localScale = soBlock.planSize;
            createdAreaTip.rotation = Quaternion.Euler(0, mNowYRotate, 0);
            // 预览
            soBlock.InstancingMesh(
                createdAreaTip.position,
                new Vector3(0, mNowYRotate, 0),
                createdAreaTip.GetComponent<MeshRenderer>().sharedMaterial);
        }
        else
        {
            mIsPlaceable = false;
        }
    }
    /// <summary> 可放置时 </summary>
    private void PlaceableDo()
    {
        mIsPlaceable = true;
        createdAreaTip.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color32(0x00, 0xFF, 0x4A, 56));
    }
    /// <summary> 不可放置时 </summary>
    private void UnplaceableDo()
    {
        mIsPlaceable = false;
        createdAreaTip.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color32(0xFF, 0x00, 0x0E, 56));
    }
    /// <summary> 设置临时旋转 </summary>
    private void ResetTempRotate() => mNowYRotate = (mNowYRotate - 180) % 360;
    /// <summary> 重置临时旋转 </summary>
    private void SetTempRotate() => mNowYRotate = (mNowYRotate + 180) % 360;
    #endregion 绘制创建预览

    // KeyEevet
    private void PlaceToBlock()
    {
        if (mIsPlaceable)
            Place(mCreatedNode);
    }
    private void DelectToBlock() => Delect(mSelectedNode);

    // DrawGizmos
    private void OnDrawGizmos()
    {
        // 绘制树
        Gizmos.color = Color.green;
        tree?.DrawGizoms();
        // 绘制选中节点
        if (mSelectedNode != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(mSelectedNode.center, mSelectedNode.size * 1.1f);
        }
        Gizmos.color = Color.white;
    }
}
