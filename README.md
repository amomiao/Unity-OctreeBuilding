下载:OctreeBuilding_八叉树建造.unitypackage 即可 <br>
演示:bilibili.com/video/BV1MZ421n7A1/ <br> 
自由相机来自:assetstore.unity.com/packages/tools/camera/free-fly-camera-140739 <br> 
打开示例场景:打开"Demo"文件夹,将所有物体拖入场景即可。<br> 

关于Block: <br>
 Block是一个ScriptableObject,可以在Project中右键新建,<br>
 PreviewImg: 对应预制体Icon<br>
 PlanSize: 期望这个物体的所占网格尺寸是多少,应全为整形<br>
 Prefab: 预制体<br>
 MeshSize: 使用"获取参数"时获得的 用以参考设计PlanSize的尺寸<br>
 MeshToPlanScale: 使用"获取参数"时自动计算获得 用来调整模型缩放<br>
 MeshFilters: 使用"获取网格过滤器"时获得 记录Prefab所有的MeshFilters <br>

制作一个规范的预制体:<br>
 1.所有模型需要挂载到一个空物体上,并且这个空物体的变化矩阵是默认的(TRS=001) <br>
 // 总结2: 调整模型(子物体) 将中心放置在世界(0,0,0) <br>
 2.当模型作为空物体的子物体后,称这个成为父物体的Obj为父物体,调整模型, <br>
 	旋转缩放(RS)任意, <br>
 	位置(P): <br>
 		y:实例化一个父物体,将父物体的y设置为 PlanSize.y/2,调整模型(子物体)的y尽可能贴合地面, 这个y值通常近似或等于PlanSize.y/2 <br>
 		xz:在y调整完成后,继续调整模型(子物体),使模型在xz平面上居中 <br>

注意点:所有涉及到的材质需要开启Enable GPU Instancting <br>
