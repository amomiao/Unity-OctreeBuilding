using UnityEngine;
namespace TreeStruct.Oct
{
    // 应用层: 为管理类提供操纵命令
    /// <summary> 实体命令:放置/移除 </summary>
    public interface ICommand<T> where T : class
    {
        /// <summary> 新建数据 </summary>
        public void Place(OctreeNode<T> operNode);
        /// <summary> 删除数据 </summary>
        public void Delect(OctreeNode<T> operNode);
    }
    /// <summary> 操纵命令:向-x旋转,向x旋转,重置 </summary>
    public interface IDataRotateCommand<T> where T : class
    {
        public void LeftRotate();
        public void RightRotate();
        public void ResetRotate();
    }

    // 数据层: 被操纵的数据需要继承的接口
    /// <summary> <see cref="OctreeAreaRoot{T}"/> 放置数据最多占1x1x1区域时使用 </summary>
    public interface IData<T> where T : class
    {
        // /// <summary> 当数据被创建时做什么 </summary> public void OnStart();
        /// <summary> 当数据被移除时做什么 </summary>
        public void OnDestroy();
    }
    /// <summary> <see cref="OctreeAreaRoot{T}"/> 放置数据占不定区域时使用 </summary>
    public interface IAreaData<T> : IData<T> where T : class
    {
        /// <summary> 物体锚定的主单元格是哪一个 </summary>
        public OctreeNode<T> MainNode { get; set; }
        /// <summary> 这个非1x1x1表示的物体,具体是什么尺寸 </summary>
        public Vector3 Size { get; }
        public Vector3 Rotate { get; }
    }

    // 逻辑层: 节点底层逻辑
    public interface IPositioning<T> where T : class
    {
        /// <summary> 树节点定位 </summary>
        public OctreeNode<T> Positioning(Vector3 targetPos, float minSize, bool onlyRead = false);
    }
}
