using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// todo：看有无现成组件
/// 带选中效果的循环滚动列表_单位的滑动组件
/// </summary>
public class LoopScrollViewWithSelectedEffect_Drag : EventTrigger
{

    private LoopScrollViewWithSelectedEffect _scrollView;

    public void SetScrollView(LoopScrollViewWithSelectedEffect view)
    {
        _scrollView = view;

    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (_scrollView != null)
        {
            _scrollView.OnBeginDrag(eventData.delta);
        }

    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (_scrollView != null)
        {
            _scrollView.OnDrag(eventData.delta);
        }

    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (_scrollView != null)
        {
            _scrollView.OnEndDrag();
        }

    }

    


}
