using System;
using System.Collections;
using UnityEngine;

namespace TreeStruct.Oct
{
    public enum E_Dir
    {
        // 内部
        In = 0b_000_000,
        // 上前右 111_000
        UpForwardRight = E_Offset.Up + E_Offset.Forward + E_Offset.Right,
        // 上前左 110_001
        UpForwardLeft = E_Offset.Up + E_Offset.Forward + E_Offset.Left,
        // 上后右 101_010
        UpBackRight = E_Offset.Up + E_Offset.Back + E_Offset.Right,
        // 上后左 100_011
        UpBackLeft = E_Offset.Up + E_Offset.Back + E_Offset.Left,
        // 下前右 011_100
        DownForwardRight = E_Offset.Down + E_Offset.Forward + E_Offset.Right,
        // 下前左 010_101
        DownForwardLeft = E_Offset.Down + E_Offset.Forward + E_Offset.Left,
        // 下后右 001_110
        DownBackRight = E_Offset.Down + E_Offset.Back + E_Offset.Right,
        // 下后左 000_111
        DownBackLeft = E_Offset.Down + E_Offset.Back + E_Offset.Left,
    }
    public enum E_Offset
    {
        Up = 1 << 6,
        Down = 1 << 5,
        Forward = 1 << 4,
        Back = 1 << 3,
        Right = 1 << 2,
        Left = 1 << 1,
    }

    public class OctreeNode<T> : IEnumerable, IPositioning<T> where T : class
    {
        public Vector3 center;
        public Vector3 size;
        public bool isleaf;   // 是叶节点
        public T data;

        private OctreeNode<T> ufl;
        private OctreeNode<T> ufr;
        private OctreeNode<T> ubl;
        private OctreeNode<T> ubr;
        private OctreeNode<T> dfl;
        private OctreeNode<T> dfr;
        private OctreeNode<T> dbl;
        private OctreeNode<T> dbr;

        private Vector3 mHalfSize;
        private int mDir;

        public OctreeNode(Vector3 center, Vector3 size)
        {
            this.center = center;
            this.size = size;
            this.mHalfSize = this.size / 2;
            this.isleaf = true;
            if (size.x < 1)
                Debug.Log($"{nameof(OctreeNode<T>)}:尺寸过小");
        }

        // 点是否在区域内，否返回点相对于区域的方向
        public E_Dir InArea(Vector3 targetPos)
        {
            if (targetPos.y < center.y + mHalfSize.y &&
                targetPos.y >= center.y - mHalfSize.y &&
                targetPos.z < center.z + mHalfSize.z &&
                targetPos.z >= center.z - mHalfSize.z &&
                targetPos.x < center.x + mHalfSize.x &&
                targetPos.x >= center.x - mHalfSize.x)
            {
                return E_Dir.In;
            }
            else
                return GetDir(targetPos, center);
        }

        // 当确信点在这个区域时使用
        public OctreeNode<T> CreateNode(Vector3 targetPos, float minSize, T data)
        {
            OctreeNode<T> node = Positioning(targetPos, minSize, false);
            node.data = data;
            return node;
        }

        // 当确信点在这个区域时使用
        public OctreeNode<T> Positioning(Vector3 targetPos, float minSize, bool onlyRead = false)
        {
            if (size.x <= minSize)  // 立方形, 一轴即可代表
                return this;
            else
            {
                // 只读时检测是否存在一个需要的最小单元 不触发切割
                if (onlyRead && isleaf)
                    return null;
                else
                {
                    if (isleaf)
                        Segmentaion();
                    switch (GetDir(targetPos, center))
                    {
                        case E_Dir.UpForwardRight: return ufr.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.UpForwardLeft: return ufl.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.UpBackRight: return ubr.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.UpBackLeft: return ubl.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.DownForwardRight: return dfr.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.DownForwardLeft: return dfl.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.DownBackRight: return dbr.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.DownBackLeft: return dbl.Positioning(targetPos, minSize, onlyRead);
                        case E_Dir.In:
                        default:
                            return null;
                    }
                }
            }
        }

        public E_Dir GetDir(Vector3 targetPos, Vector3 center)
        {
            // 可以通过位运算简化计算量 100_001:太靠上 前后正好 太靠左,
            // 可以通过 100 异或 001 得 101
            // 101 => 010 + 100_001 = 100_011
            mDir = (int)E_Dir.In;
            // 正好在中心,返回上前右
            mDir += targetPos.y >= center.y ? (int)E_Offset.Up : (int)E_Offset.Down;
            mDir += targetPos.z >= center.z ? (int)E_Offset.Forward : (int)E_Offset.Back;
            mDir += targetPos.x >= center.x ? (int)E_Offset.Right : (int)E_Offset.Left;
            switch (mDir)
            {
                case (int)E_Dir.UpForwardRight:
                    return E_Dir.UpForwardRight;
                case (int)E_Dir.UpForwardLeft:
                    return E_Dir.UpForwardLeft;
                case (int)E_Dir.UpBackRight:
                    return E_Dir.UpBackRight;
                case (int)E_Dir.UpBackLeft:
                    return E_Dir.UpBackLeft;
                case (int)E_Dir.DownForwardRight:
                    return E_Dir.DownForwardRight;
                case (int)E_Dir.DownForwardLeft:
                    return E_Dir.DownForwardLeft;
                case (int)E_Dir.DownBackRight:
                    return E_Dir.DownBackRight;
                case (int)E_Dir.DownBackLeft:
                    return E_Dir.DownBackLeft;
                default:
                    Debug.LogError($"{nameof(OctreeNode<T>)}.{nameof(InArea)}");
                    return E_Dir.In;
            }
        }

        // 将Root替换为一个体积更大的节点
        public OctreeNode<T> Expand(OctreeNode<T> origin, E_Dir expandDir)
        {
            OctreeNode<T> newRoot;
            Vector3 half = origin.size / 2;
            switch (expandDir)
            {
                case E_Dir.In:
                default:
                    newRoot = null;
                    break;
                case E_Dir.UpForwardRight:
                    // 点在新root的上前右 -> 新节点中心点向上前右推
                    newRoot = new OctreeNode<T>(origin.center + half, origin.size * 2);
                    // 旧root在新root的下后左
                    newRoot.Segmentaion(ndbl: origin);
                    break;
                case E_Dir.UpForwardLeft:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(-half.x, half.y, half.z), origin.size * 2);
                    newRoot.Segmentaion(ndbr: origin);
                    break;
                case E_Dir.UpBackRight:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(half.x, half.y, -half.z), origin.size * 2);
                    newRoot.Segmentaion(ndfl: origin);
                    break;
                case E_Dir.UpBackLeft:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(-half.x, half.y, -half.z), origin.size * 2);
                    newRoot.Segmentaion(ndfr: origin);
                    break;
                case E_Dir.DownForwardRight:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(half.x, -half.y, half.z), origin.size * 2);
                    newRoot.Segmentaion(nubl: origin);
                    break;
                case E_Dir.DownForwardLeft:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(-half.x, -half.y, half.z), origin.size * 2);
                    newRoot.Segmentaion(nubr: origin);
                    break;
                case E_Dir.DownBackRight:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(half.x, -half.y, -half.z), origin.size * 2);
                    newRoot.Segmentaion(nufl: origin);
                    break;
                case E_Dir.DownBackLeft:
                    newRoot = new OctreeNode<T>(origin.center + new Vector3(-half.x, -half.y, -half.z), origin.size * 2);
                    newRoot.Segmentaion(nufr: origin);
                    break;
            }
            return newRoot;
        }

        ///<summary> 射线检测 </summary>
        public bool IntersectRay(Ray ray, out float distance) => new Bounds(center, size).IntersectRay(ray, out distance);

        ///<summary> 射线打到了那个平面 </summary>
        public E_Offset EnterPlaneRay(Ray ray)
        {
            float minDistance = float.MaxValue;
            E_Offset returnToward = E_Offset.Up;
            MAC_PlaneRay_CheckPlane(ray, new Bounds(center + mHalfSize.y * Vector3.up, new Vector3(size.x, 0, size.z)), E_Offset.Up, ref minDistance, ref returnToward);
            MAC_PlaneRay_CheckPlane(ray, new Bounds(center + mHalfSize.y * Vector3.down, new Vector3(size.x, 0, size.z)), E_Offset.Down, ref minDistance, ref returnToward);
            MAC_PlaneRay_CheckPlane(ray, new Bounds(center + mHalfSize.x * Vector3.right, new Vector3(0, size.y, size.z)), E_Offset.Right, ref minDistance, ref returnToward);
            MAC_PlaneRay_CheckPlane(ray, new Bounds(center + mHalfSize.x * Vector3.left, new Vector3(0, size.y, size.z)), E_Offset.Left, ref minDistance, ref returnToward);
            MAC_PlaneRay_CheckPlane(ray, new Bounds(center + mHalfSize.z * Vector3.forward, new Vector3(size.x, size.y, 0)), E_Offset.Forward, ref minDistance, ref returnToward);
            MAC_PlaneRay_CheckPlane(ray, new Bounds(center + mHalfSize.z * Vector3.back, new Vector3(size.x, size.y, 0)), E_Offset.Back, ref minDistance, ref returnToward);
            return returnToward;
        }
        // 封装宏: 检测平面
        private void MAC_PlaneRay_CheckPlane(Ray ray, Bounds bounds, E_Offset toward, ref float minDistance, ref E_Offset returnToward)
        {
            if (bounds.IntersectRay(ray, out float nowDis))
                if (nowDis < minDistance)
                {
                    returnToward = toward;
                    minDistance = nowDis;
                }
        }

        public void DrawGizoms()    // 绘制小物体
        {
            if (isleaf)
            {
                if (data != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(center, size);
                    Gizmos.color = Color.white;
                }
            }
            else
            {
                foreach (OctreeNode<T> child in this)
                    child.DrawGizoms();
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (!isleaf)
            {
                yield return ufl;
                yield return ufr;
                yield return ubl;
                yield return ubr;
                yield return dfl;
                yield return dfr;
                yield return dbl;
                yield return dbr;
            }
        }

        private void Segmentaion(
            OctreeNode<T> nufl = null, OctreeNode<T> nufr = null, OctreeNode<T> nubl = null, OctreeNode<T> nubr = null,
            OctreeNode<T> ndfl = null, OctreeNode<T> ndfr = null, OctreeNode<T> ndbl = null, OctreeNode<T> ndbr = null)
        {
            isleaf = false;
            Vector3 oneFourth = size / 4; // 子节点center与父节点的xyz轴上的距离是 父节点尺寸的1/4
            ufl = nufl == null ? new OctreeNode<T>(center + new Vector3(-oneFourth.x, oneFourth.y, oneFourth.z), size / 2) : nufl;
            ufr = nufr == null ? new OctreeNode<T>(center + new Vector3(oneFourth.x, oneFourth.y, oneFourth.z), size / 2) : nufr;
            ubl = nubl == null ? new OctreeNode<T>(center + new Vector3(-oneFourth.x, oneFourth.y, -oneFourth.z), size / 2) : nubl;
            ubr = nubr == null ? new OctreeNode<T>(center + new Vector3(oneFourth.x, oneFourth.y, -oneFourth.z), size / 2) : nubr;
            dfl = ndfl == null ? new OctreeNode<T>(center + new Vector3(-oneFourth.x, -oneFourth.y, oneFourth.z), size / 2) : ndfl;
            dfr = ndfr == null ? new OctreeNode<T>(center + new Vector3(oneFourth.x, -oneFourth.y, oneFourth.z), size / 2) : ndfr;
            dbl = ndbl == null ? new OctreeNode<T>(center + new Vector3(-oneFourth.x, -oneFourth.y, -oneFourth.z), size / 2) : ndbl;
            dbr = ndbr == null ? new OctreeNode<T>(center + new Vector3(oneFourth.x, -oneFourth.y, -oneFourth.z), size / 2) : ndbr;
        }
    }
}
