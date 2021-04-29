using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 带选中效果的循环滚动列表_单位
/// </summary>
public class LoopScrollViewWithSelectedEffect_Item : MonoBehaviour
{
    private Transform _tr;
    private LoopScrollViewWithSelectedEffect _scrollView;

    /// <summary>
    /// 显示用单位列表中的索引
    /// </summary>
    [HideInInspector]
    public int oriListIndex;

    /// <summary>
    /// 显示用单位列表 大小
    /// </summary>
    [HideInInspector]
    public int dataLength;

    [HideInInspector]
    public int sortXValListIndex = 0;

    /// <summary>
    /// 中心偏移值，限制在0-1之间
    /// </summary>
    [HideInInspector]
    public float centerOffSet = 0;

    /// <summary>
    /// 数据索引
    /// </summary>
    [HideInInspector]
    public int dataIndex = 0;

    private LoopScrollViewWithSelectedEffect.VoidDelegate itemDefaultShowCallback;
    private LoopScrollViewWithSelectedEffect.VoidDelegate itemSelectCallback;
    private LoopScrollViewWithSelectedEffect.VoidDelegate itemUnSelectCallback;

    /// <summary>
    /// 单位滑过滑动框开头时回调
    /// </summary>
    private LoopScrollViewWithSelectedEffect.VoidDelegate itemPassBeginCallback;

    /// <summary>
    /// 单位滑过滑动框末尾时回调
    /// </summary>
    private LoopScrollViewWithSelectedEffect.VoidDelegate itemPassEndCallback;

    /// <summary>
    /// 单位点击回调
    /// </summary>
    private LoopScrollViewWithSelectedEffect.VoidDelegate itemClickCallback;


    private void Awake()
    {
        _tr = transform;

    }
   

    /// <summary>
    /// 更新单位的位置、大小、层级
    /// </summary>
    /// <param name="xVal"></param>
    /// <param name="moveDir">右移：1，左移：-1，不移：0</param>
    /// <param name="depthCurveValue"></param>
    /// <param name="itemCount"></param>
    /// <param name="yValue"></param>
    /// <param name="scaleValue"></param>
    public void UpdateScrollViewItems(float xVal, int moveDir, float depthCurveValue, int itemCount, float yValue, float scaleValue)
    {
        Vector3 targetPos = Vector3.one;
        Vector3 targetScale = Vector3.one;
        targetPos.x = xVal;
        targetPos.y = yValue;
        if (moveDir == 1 && _tr.localPosition.x > xVal)
        {
            int temp = dataIndex + (dataLength - itemCount);
            if (temp < 0)
            {
                temp += dataLength;
            }
            dataIndex = temp % dataLength;
            itemPassEndCallback(this);
        }
        else if (moveDir == -1 && _tr.localPosition.x < xVal)
        {
            dataIndex = (dataIndex + itemCount) % dataLength;
            itemPassBeginCallback(this);
        }
        // 因为变化小，所以位置直接赋值
        _tr.localPosition = targetPos;
        SetDepth(depthCurveValue, itemCount);
        targetScale.x = targetScale.y = scaleValue;
        _tr.localScale = targetScale;

    }

    public void SetScollView(LoopScrollViewWithSelectedEffect scrollView)
    {
        _scrollView = scrollView;

    }

    /// <summary>
    /// 点击单位
    /// </summary>
    public void OnClickItem()
    {
        _scrollView.SetCenterItem(this);

    }

    private void SetDepth(float depthCurveValue, float itemCount)
    {
        // depthCurveValue * itemCount即可得到层级，因为最大层级必然不超过itemCount
        int newDepth = (int)(depthCurveValue * itemCount);
        _tr.SetSiblingIndex(newDepth);

    }

    public void SetSelectState(bool isSelect)
    {
        if (isSelect)
        {
            if (itemSelectCallback != null)
            {
                itemSelectCallback(this);
            }
        }
        else
        {
            if (itemUnSelectCallback != null)
            {
                itemUnSelectCallback(this);
            }
        }

    }

    public void SetItemDefaultShowCallback(LoopScrollViewWithSelectedEffect.VoidDelegate callback)
    {
        itemDefaultShowCallback = callback;

    }

    public void DefaultShow()
    {
        if (itemDefaultShowCallback != null)
        {
            itemDefaultShowCallback(this);
        }
        // 不能只用 dataIndex 来判断，因为在 dataLength < itemCount 情况下，dataIndex 会有重复
        SetSelectState(this == _scrollView.GetCurCenterItem());

    }

    public void SetItemSelectCallback(LoopScrollViewWithSelectedEffect.VoidDelegate callback)
    {
        itemSelectCallback = callback;

    }

    public void SetItemUnSelectCallback(LoopScrollViewWithSelectedEffect.VoidDelegate callback)
    {
        itemUnSelectCallback = callback;

    }

    public void SetItemPassBeginCallback(LoopScrollViewWithSelectedEffect.VoidDelegate callback)
    {
        itemPassBeginCallback = callback;

    }

    public void SetItemPassEndCallback(LoopScrollViewWithSelectedEffect.VoidDelegate callback)
    {
        itemPassEndCallback = callback;

    }

    public void SetItemClickCallback(LoopScrollViewWithSelectedEffect.VoidDelegate callback)
    {
        itemClickCallback = callback;

    }

}
