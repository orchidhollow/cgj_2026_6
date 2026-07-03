using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 缓存池中的数据对象
/// </summary>
public class PoolData
{
    //用来存储数据对象
    private Stack<GameObject> dataStack = new Stack<GameObject>();
    //用来存储正在使用的数据对象
    private Queue<GameObject> usedDataQueue = new Queue<GameObject>();

    //对象池正在使用的数据对象数量的最大值 从挂载的物体身上设置并获取
    public int maxNum;
    //抽屉根对象 用来进行布局管理
    private GameObject rootObj;
    //获取容器中是否有对象
    public int Count => dataStack.Count;
    //获取正在使用的数据对象数量
    public int usedCount => usedDataQueue.Count;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="poolObj">用于作为根的对象</param>
    /// <param name="rootName">根对象的名字</param>
    /// <param name="firstObj">第一个获取的对象</param>
    public PoolData(GameObject poolObj,string rootName,GameObject firstObj)
    {
        //开启布局功能时 才会动态创建父子关系
        if(PoolMgr.isOpenLayout)
        {
            //创建抽屉父对象
            rootObj = new GameObject(rootName + "_Pool");
            //和柜子对象建立父子关系
            rootObj.transform.SetParent(poolObj.transform);
        }
        //从gameObject身上挂载的脚本上获取限制的最大值
        PoolObjMaxNum poolObjMaxNum = firstObj.GetComponent<PoolObjMaxNum>();
        if(poolObjMaxNum == null)
        {
            Debug.LogError("请为对象挂载PoolObjMaxNum脚本，并设置限制最大值！");
            return;
        }
        maxNum = poolObjMaxNum.maxNum;
        if(maxNum == 0)
        {
            Debug.LogError("请为对象挂载的PoolObjMaxNum脚本上的maxNum设置限制最大值！");
            return;
        }
        if(maxNum == 1)
        {
            Debug.LogError("请不要将限制最大值设为1！");
            return;
        }
    }

    /// <summary>
    /// 从缓存池中取出对象
    /// </summary>
    /// <returns>想要的对象数据</returns>
    public GameObject Pop()
    {
        //取出对象
        GameObject obj = dataStack.Pop();
        //激活对象
        obj.SetActive(true);
        //将对象存入正在使用的队列
        usedDataQueue.Enqueue(obj);

        if (PoolMgr.isOpenLayout)
        {
            //断开父子关系
            obj.transform.SetParent(null);
        }

        return obj;
    }

    /// <summary>
    /// 将物体存放到缓存池中
    /// </summary>
    /// <param name="obj">要存放的物体</param>
    public void Push(GameObject obj)
    {
        //让对象失活
        obj.SetActive(false);
        //让对象离开正在使用的队列
        usedDataQueue.Dequeue();

        if (PoolMgr.isOpenLayout)
        {
            //放入对应抽屉的根物体 建立父子关系
            obj.transform.SetParent(rootObj.transform);
        }
        //让栈记录对应的对象数据
        dataStack.Push(obj);
    }

    public void usedQueuePush(GameObject obj)
    {
        usedDataQueue.Enqueue(obj);
    }

    public GameObject usedQueuePop()
    {
        GameObject obj = usedDataQueue.Dequeue();
        return obj;
    }
}

/// <summary>
/// 缓存池（对象池）模块 管理器
/// </summary>
public class PoolMgr : BaseManager<PoolMgr>
{
    //柜子容器
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    //抽屉根对象 用来进行布局管理
    private GameObject poolObj;
    //是否开启布局功能
    public static bool isOpenLayout = true;

    private PoolMgr() { }

    /// <summary>
    /// 从缓存池中取东西的方法
    /// </summary>
    /// <param name="name">抽屉容器的名字</param>
    /// <param name="maxNum">对象上限数量 默认为10</param>
    /// <returns>取出的对象</returns>
    public GameObject GetObj(string name)
    {
        GameObject obj;

        //存物体前判断是否有根物体
        //若为空 则创建
        if (PoolMgr.isOpenLayout && poolObj == null )
        {
            poolObj = new GameObject("Pool");
        }
        //不存在对应的抽屉容器 创建抽屉
        if (!poolDic.ContainsKey(name))
        {
            //通过资源加载去实例化一个gameobject
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
            obj.name = name;
            //创建一个新的抽屉
            poolDic.Add(name, new PoolData(poolObj, name ,obj));
            //将obj放入正在使用中的数据对象队列
            poolDic[name].usedQueuePush(obj);
        }
        //存在对应的抽屉容器
        else
        {
            //若使用中的对象数量超过上限
            if (poolDic[name].usedCount >= poolDic[name].maxNum)
            {
                //从usedDataQueue中取出对象再次加入usedDataQueue中
                obj = poolDic[name].usedQueuePop();
                poolDic[name].usedQueuePush(obj);
            }
            //若使用中的对象数量没超过上限
            else
            {
                //抽屉中有对象
                if (poolDic[name].Count > 0)
                {
                    obj = poolDic[name].Pop();
                }
                //如果没有对象 去创造
                else
                {
                    //通过资源加载去实例化一个gameobject
                    obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
                    obj.name = name;
                    poolDic[name].usedQueuePush(obj);   
                }
            }
        }
        return obj;
    }

    /// <summary>
    /// 往缓存池中放东西的方法
    /// </summary>
    /// <param name="name">放入抽屉的名字</param>
    /// <param name="obj">放入的对象</param>
    public void PushObj(GameObject obj)
    {
        //往抽屉中存入对象
        poolDic[obj.name].Push(obj);
    }
    /// <summary>
    /// 用于清楚整个柜子的数据
    /// 使用场景主要是切场景时
    /// </summary>
    public void ClearPool()
    {
        poolDic.Clear();
        //切换场景时 根物体也要被移除
        poolObj = null;
    }
}
