using System.Collections;
using UnityEngine;

namespace TreeStruct.Oct
{
    public class OctreeAreaRoot<T> : OctreeRoot<T> where T : class, IAreaData<T>
    {
        private OctreeAreaRoot(Vector3 startPos, float minSize, T initData = null, float maxSize = 268435456) : base(startPos, minSize, initData, maxSize) { }
        public OctreeAreaRoot(Vector3 startPos, float minSize) : base(startPos, minSize, null, 268435456) { }
        public OctreeAreaRoot(T data, float minSize, out Vector3 startPos) : base(Vector3.zero, minSize, null, 268435456)
        {
            startPos = GetAreaCenter(root.center, data.Size, data.Rotate);
            Place(root, data);
        }

        protected IEnumerable ForArea(OctreeNode<T> node, Vector3 size, Vector3 rotate)
        {
            if (node != null)
            {
                for (int x = 0; x < size.x; x++)
                    for (int y = 0; y < size.y; y++)
                        for (int z = 0; z < size.z; z++)
                        {
                            yield return Positioning(node.center + Quaternion.Euler(rotate) * new Vector3(x, y, z) * MinSize, MinSize);
                        }
            }
        }
        protected IEnumerable ForArea(T data)
        {
            if (data != null && data.MainNode != null)
            {
                for (int x = 0; x < data.Size.x; x++)
                    for (int y = 0; y < data.Size.y; y++)
                        for (int z = 0; z < data.Size.z; z++)
                        {
                            yield return Positioning(data.MainNode.center + Quaternion.Euler(data.Rotate) * new Vector3(x, y, z) * MinSize, MinSize);
                        }
            }
        }

        /// <summary> 区域数据检测 </summary>
        public bool AreaCheck(OctreeNode<T> targetNode, Vector3 size, Vector3 rotate)
        {
            if (targetNode != null)
            {
                foreach (OctreeNode<T> node in ForArea(targetNode, size, rotate))
                {
                    if (node.data != null)
                        return false;
                }
            }
            return true;
        }

        /// <summary> 放置区域数据 </summary>
        public override bool Place(OctreeNode<T> targetNode, T data)
        {
            if (targetNode != null)
            {
                data.MainNode = targetNode;
                foreach (OctreeNode<T> node in ForArea(data))
                    base.Place(node, data);
                return true;
            }
            return false;
        }

        /// <summary> 删除区域数据 </summary>
        public override bool Delect(OctreeNode<T> targetNode, out T data)
        {
            data = null;
            if (targetNode != null && targetNode.data != null)
            {
                data = targetNode.data;
                foreach (OctreeNode<T> node in ForArea(data))
                    base.Delect(node, out _);
                data.OnDestroy();
                return true;
            }
            return false;
        }

        /// <summary> 获取区域中心点: 无旋转时使用,性能更好 </summary>
        public Vector3 GetAreaCenter(Vector3 mainNodeCenter, Vector3 size)
        {
            for (int x = 1; x < size.x; x++)
                mainNodeCenter.x += MinSize / 2;
            for (int y = 1; y < size.y; y++)
                mainNodeCenter.y += MinSize / 2;
            for (int z = 1; z < size.z; z++)
                mainNodeCenter.z += MinSize / 2;
            return mainNodeCenter;
        }
        /// <summary> 获取区域中心点:使用有MainNode的data </summary>
        public Vector3 GetAreaCenter(T data)
        {
            if (data.MainNode == null)
                throw new System.Exception($"{nameof(OctreeAreaRoot<T>)}:{nameof(GetAreaCenter)}:Data.MainNode有null的风险的逻辑,不应该使用这个");
            else
                return GetAreaCenter(data.MainNode.center, data.Size, data.Rotate);
        }

        /// <summary> 获取区域中心点:应用旋转(传入y轴旋转角) </summary>
        public Vector3 GetAreaCenter(Vector3 mainNodeCenter, Vector3 size, float yRotate) => GetAreaCenter(mainNodeCenter, size, Quaternion.Euler(0, yRotate, 0));
        /// <summary> 获取区域中心点:应用旋转(传入三轴旋转角) </summary>
        public Vector3 GetAreaCenter(Vector3 mainNodeCenter, Vector3 size, Vector3 rotate) => GetAreaCenter(mainNodeCenter, size, Quaternion.Euler(rotate));
        /// <summary> 获取区域中心点:应用旋转(传入四元数) </summary>
        public Vector3 GetAreaCenter(Vector3 mainNodeCenter, Vector3 size, Quaternion rotation)
        {
            Vector3 incre = Vector3.zero;
            for (int x = 1; x < size.x; x++)
                incre.x += MinSize / 2;
            for (int y = 1; y < size.y; y++)
                incre.y += MinSize / 2;
            for (int z = 1; z < size.z; z++)
                incre.z += MinSize / 2;
            return mainNodeCenter + rotation * incre;
        }
    }
}
