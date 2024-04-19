using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeStruct.Oct
{
    public class OctreeRoot<T> : IEnumerable, IPositioning<T> where T : class
    {
        public OctreeNode<T> root;

        protected float mMinSize;
        protected float mMaxSize = int.MaxValue >> 3;
        protected E_Dir mDir;

        public float MinSize => mMinSize;

        public OctreeRoot(Vector3 startPos, float minSize, T initData = null, float maxSize = int.MaxValue >> 3)
        {
            mMinSize = minSize;
            mMaxSize = maxSize;
            // 创建一个最小节点作为新建树的根结点
            root = new OctreeNode<T>(startPos, Vector3.one * minSize);
            if (initData != null)   // 根据需要添加初始数据
            {
                Place(root, initData);
            }
        }

        /// <summary> 尝试创建一个最小子节点 </summary>
        public OctreeNode<T> TryCreateNode(Vector3 targetPos, T data)
        {
            OctreeNode<T> node = Positioning(targetPos, MinSize);
            if (node != null)
                node.data = data;
            return node;
        }

        /// <summary> 放置单格(1x1x1)数据 </summary>
        public virtual bool Place(OctreeNode<T> targetNode, T data)
        {
            if (targetNode != null)
            {
                targetNode.data = data;
                return true;
            }
            return false;
        }

        /// <summary> 删除单格(1x1x1)数据 </summary>
        public virtual bool Delect(OctreeNode<T> targetNode, out T data)
        {
            data = null;
            if (targetNode != null)
            {
                data = targetNode.data;
                targetNode.data = null;
                return true;
            }
            return false;
        }

        /// <summary> 寻找有意义的最小节点 </summary>
        /// <returns> 返回null证明没有这个最小节点 </returns>
        public OctreeNode<T> TryGetNode(Vector3 targetPos)
          => root.InArea(targetPos) == E_Dir.In ? root.Positioning(targetPos, MinSize, true) : null;

        // 射线寻找有意义的最小节点 返回null证明没有这个最小节点
        //[Obsolete("这种方式每次检测会创建大量叶子节点,更适合满树", true)]
        //public OctreeNode<T> TryRayGet(Vector3 startPos, Vector3 dir, float length) => throw new Exception("在射线上,每MinSize上取样检测,并检测3x3的区域");

        /// <summary> 射线寻找有意义的最小数据节点: 目标需要挂载数据 </summary>
        /// <returns> 返回null证明没有这个最小节点 </returns>
        public OctreeNode<T> IntersectRay(Ray ray, float maxLength) => IntersectRay(ray, maxLength, (node) => node.data != null);

        /// <summary> 射线寻找有意义的最小逻辑节点: 无需挂载数据 </summary>
        /// <returns> 返回null证明没有这个最小节点 </returns>
        protected OctreeNode<T> IntersectRay(Ray ray, float maxLength, Func<OctreeNode<T>, bool> filter = null)
        {
            float minDis = maxLength;
            OctreeNode<T> rn = null;
            float nowDis;
            foreach (OctreeNode<T> node in this)
            {
                if (node.IntersectRay(ray, out nowDis))
                {
                    // 一般情况 <= 即可
                    if (nowDis < minDis || Mathf.Approximately(nowDis, minDis))
                    {
                        if (filter == null || filter.Invoke(node))
                        {
                            rn = node;
                            minDis = nowDis;
                        }
                    }
                }
            }
            return rn;
        }

        /// <summary> 射线寻找有意义的最小逻辑节点,返回 射入面邻接的节点 </summary>
        public OctreeNode<T> IntersectRayDirNode(Ray ray, float maxLength)
        {
            OctreeNode<T> rayNode = IntersectRay(ray, maxLength);
            if (rayNode != null)
            {
                return rayNode.EnterPlaneRay(ray) switch
                {
                    E_Offset.Down => this.Positioning(rayNode.center + rayNode.size.y * Vector3.down, MinSize),
                    E_Offset.Forward => this.Positioning(rayNode.center + rayNode.size.z * Vector3.forward, MinSize),
                    E_Offset.Back => this.Positioning(rayNode.center + rayNode.size.z * Vector3.back, MinSize),
                    E_Offset.Right => this.Positioning(rayNode.center + rayNode.size.x * Vector3.right, MinSize),
                    E_Offset.Left => this.Positioning(rayNode.center + rayNode.size.x * Vector3.left, MinSize),
                    // up + default
                    _ => this.Positioning(rayNode.center + rayNode.size.y * Vector3.up, MinSize),
                };
            }
            return null;
        }

        // GUI绘制
        public void DrawGizoms()
        {
            Gizmos.DrawWireCube(root.center, root.size);
            root.DrawGizoms();
        }

        // Interface
        /// <summary> 遍历获取所有最小叶子节点(优先度依照OctreeNode的GetEnumerator) </summary>
        public IEnumerator GetEnumerator()
        {
            Queue<OctreeNode<T>> nodeQueue = new Queue<OctreeNode<T>>();
            OctreeNode<T> node;
            nodeQueue.Enqueue(root);
            while (nodeQueue.Count > 0)
            {
                node = nodeQueue.Dequeue();
                // 非叶子节点: 继续向下
                if (!node.isleaf)
                {
                    foreach (OctreeNode<T> child in node)
                        nodeQueue.Enqueue(child);
                } // 叶子节点: 仅计入最小叶节点
                else if (node.size.x <= MinSize)
                    yield return node;
            }
        }

        public OctreeNode<T> Positioning(Vector3 targetPos, float minSize, bool onlyRead = false)
        {
            mDir = root.InArea(targetPos);
            if (mDir == E_Dir.In)
                return root.Positioning(targetPos, MinSize, onlyRead);
            else if (root.size.x < mMaxSize)
            {
                root = root.Expand(root, mDir);
                return this.Positioning(targetPos, MinSize, onlyRead);
            }
            else
                throw new System.Exception($"{nameof(OctreeRoot<T>)}:root尺寸到达极限!");
        }
    }
}
