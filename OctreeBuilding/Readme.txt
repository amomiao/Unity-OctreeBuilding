打开示例场景:打开"Demo"文件夹,将所有物体拖入场景即可。

关于Block:
 Block是一个ScriptableObject,可以在Project中右键新建,
 PreviewImg: 对应预制体Icon
 PlanSize: 期望这个物体的所占网格尺寸是多少,应全为整形
 Prefab: 预制体
 MeshSize: 使用"获取参数"时获得的 用以参考设计PlanSize的尺寸
 MeshToPlanScale: 使用"获取参数"时自动计算获得 用来调整模型缩放
 MeshFilters: 使用"获取网格过滤器"时获得 记录Prefab所有的MeshFilters

制作一个合法的预制体:
 1.所有模型需要挂载到一个空物体上,并且这个空物体的变化矩阵是默认的(TRS=001)
 // 总结2: 调整模型(子物体) 将中心放置在世界(0,0,0)
 2.当模型作为空物体的子物体后,称这个成为父物体的Obj为父物体,调整模型,
 	旋转缩放(RS)任意,
 	位置(P):
 		y:实例化一个父物体,将父物体的y设置为 PlanSize.y/2,调整模型(子物体)的y尽可能贴合地面, 这个y值通常近似或等于PlanSize.y/2
 		xz:在y调整完成后,继续调整模型(子物体),使模型在xz平面上居中

注意点:所有涉及到的材质需要