using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 带选中效果的循环滚动列表
/// 使用 AnimationCurve 实现动效
/// 使用 %取余 实现数据循环
/// </summary>
public class LoopScrollViewWithSelectedEffect : MonoBehaviour
{
    /// <summary>
    /// 单位滑动最大宽度
    /// </summary>
    private float _totalHorizontalWidth = 500f;

    private Transform _tr;
    private int _dataLength = 0;
    /// <summary>
    /// 数据长度放大倍数
    /// </summary>
    private int _dataLengthMul = 2;

    /// <summary>
    /// 当前插值用时
    /// </summary>
    private float _currentDuration = 0.0f;

    private int _oriListCenterIndex = 0;

    /// <summary>
    /// 激活Tween插值
    /// </summary>
    private bool _enableLerpTween = false;

    /// <summary>
    /// 常用来限制某个值的范围在0-1之间
    /// </summary>
    private float _factor = 0.2f;

    /// <summary>
    /// 是否可更改视框中心的元素
    /// </summary>
    private bool _canChangeCenterItem = true;

    private float _originHorizontalValue = 0f;

    /// <summary>
    /// 当前水平拖动值，牌组往左走--，往右走++
    /// </summary>
    private float _curHorizontalValue = 0.5f;

    private LoopScrollViewWithSelectedEffect_Item _curCenterItem;
    private LoopScrollViewWithSelectedEffect_Item _preCenterItem;

    private VoidDelegate itemDefaultShowCallback;
    private VoidDelegate itemSelectCallback;
    private VoidDelegate itemUnSelectCallback;

    /// <summary>
    /// 单位滑过滑动框开头时回调
    /// </summary>
    private VoidDelegate itemPassBeginCallback;

    /// <summary>
    /// 单位滑过滑动框末尾时回调
    /// </summary>
    private VoidDelegate itemPassEndCallback;

    /// <summary>
    /// 单位点击回调
    /// </summary>
    private VoidDelegate itemClickCallback;


    /// <summary>
    /// 原始（即按初始生成顺序）列表
    /// </summary>
    private List<LoopScrollViewWithSelectedEffect_Item> _oriItemList = new List<LoopScrollViewWithSelectedEffect_Item>();

    [HideInInspector]
    /// <summary>
    /// 按位置x值从小到大排的列表
    /// </summary>
    public List<LoopScrollViewWithSelectedEffect_Item> xValSortItemList = new List<LoopScrollViewWithSelectedEffect_Item>();

    /// <summary>
    /// 单位模板
    /// </summary>
    public GameObject itemTemplate;

    /// <summary>
    /// 视框中显示几个单位
    /// </summary>
    public int showItemCountInView = 3;

    public float itemSpacingWidth = 10f;

    /// <summary>
    /// 初始选中数据的索引
    /// </summary>
    private int _startCenterDataIndex = 0;

    /// <summary>
    /// 固定的位置y值
    /// </summary>
    public float yFixedPosVal = 0f;

    /// <summary>
    /// 初始中心偏移值
    /// </summary>
    public float startCenterXOffset = -0.001f;

    /// <summary>
    /// 插值用时，即最靠中间且是未选中状态的单位变成选中状态时用时
    /// </summary>
    public float lerpDuration = 0.2f;

    /// <summary>
    /// 拖动范围影响因子，值越大拖动越快
    /// </summary>
    public float scrollFactor = 0.001f;


    /// <summary>
    /// 位置x的曲线：x值（一般x范围从0到1）可以看成单位从最左滑到最右边的过程
    /// 如果要明显环形效果，则应该设计成离中心越远，移动值变化越少的曲线
    /// </summary>
    public AnimationCurve positionCurve;

    /// <summary>
    /// 大小曲线：x值（一般x范围从0到1）可以看成单位从最左滑到最右边的过程
    /// 如果要明显环形效果（即大小变化为小——>大——>小），则应该设计成看起来是三角形的曲线
    /// </summary>
    public AnimationCurve scaleCurve;

    /// <summary>
    /// 深度曲线：x值（一般x范围从0到1）可以看成单位从最左滑到最右边的过程
    /// 如果要明显环形效果（即中间卡牌层级最高，后边的牌一层比一层低），则应该设计成看起来是三角形的曲线
    /// </summary>
    public AnimationCurve depthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    [HideInInspector]
    public delegate void VoidDelegate(LoopScrollViewWithSelectedEffect_Item itemSelf);




    private void Awake()
    {
        _tr = transform;

    }

    public void Init()
    {
        _canChangeCenterItem = true;
        int useItemCount = showItemCountInView * _dataLengthMul;
        // 乘1万再除以1万的方式，就是为了保留4位小数
        _factor = (Mathf.RoundToInt((1f / useItemCount) * 10000f)) * 0.0001f;
        _totalHorizontalWidth = useItemCount * itemSpacingWidth;
        _oriListCenterIndex = useItemCount / 2;
        if (useItemCount % 2 == 0)
        {
            _oriListCenterIndex = useItemCount / 2 - 1;
        }
        itemTemplate.SetActive(true);
        for (int i = 0; i < useItemCount; i++)
        {
            GameObject go = Instantiate(itemTemplate);
            go.name = i.ToString();
            go.transform.SetParent(_tr);
            go.transform.localPosition = new Vector3(_factor * i * _totalHorizontalWidth - 0.5f * _totalHorizontalWidth, yFixedPosVal, 0);
            go.transform.localScale = Vector3.one;
            LoopScrollViewWithSelectedEffect_Drag drag = go.GetComponent<LoopScrollViewWithSelectedEffect_Drag>();
            if (drag == null)
            {
                drag = go.AddComponent<LoopScrollViewWithSelectedEffect_Drag>();
            }
            drag.SetScrollView(this);
            LoopScrollViewWithSelectedEffect_Item item = go.GetComponent<LoopScrollViewWithSelectedEffect_Item>();
            if (item == null)
            {
                item = go.AddComponent<LoopScrollViewWithSelectedEffect_Item>();
            }
            item.oriListIndex = i;
            item.dataLength = _dataLength;
            item.SetScollView(this);
            item.SetItemDefaultShowCallback(itemDefaultShowCallback);
            item.SetItemSelectCallback(itemSelectCallback);
            item.SetItemUnSelectCallback(itemUnSelectCallback);
            item.SetItemPassBeginCallback(itemPassBeginCallback);
            item.SetItemPassEndCallback(itemPassEndCallback);
            _oriItemList.Add(item);
        }
        itemTemplate.SetActive(false);
        #region 修正数据
        int index = 0;
        int centerItemIndex = _startCenterDataIndex % useItemCount;
        int dataValOffset = _startCenterDataIndex - centerItemIndex;
        // 必须倒序循环，因为要按“中心卡牌为0，左边为负，右边为正”的位置摆牌（正序循环则会相反）
        for (int i = useItemCount - 1; i >= 0; i--)
        {
            var item = _oriItemList[i];
            // 乘上factor就是为了让centerOffSet是个小数
            item.centerOffSet = _factor * (_oriListCenterIndex - index);
            item.SetSelectState(false);
            index++;
        }
        xValSortItemList = new List<LoopScrollViewWithSelectedEffect_Item>(_oriItemList);
        _curCenterItem = _oriItemList[centerItemIndex];
        _totalHorizontalWidth = useItemCount * itemSpacingWidth;
        // 为了让中心单位出现在视图中心，就得用0.5减（和后面GetXPosValue函数有关）
        // 为什么不直接用0.5f的原因：如果用0.5f会算出有些牌SiblingIndex重复问题，但在滑动牌组时，应是每一张牌各占一层。
        float targetVal = 0.5f + startCenterXOffset - _curCenterItem.centerOffSet;
        LerpTweenToTarget(0, targetVal, 0, false);
        SortItemListByXVal();
        int indexOffset = _curCenterItem.oriListIndex - _curCenterItem.sortXValListIndex;
        for (var i = 0; i < xValSortItemList.Count; i++)
        {
            var item = xValSortItemList[i];
            int dataIndex = (i + indexOffset + dataValOffset) % _dataLength;
            if (dataIndex < 0)
            {
                dataIndex += _dataLength;
            }
            item.dataIndex = dataIndex;
            item.DefaultShow();
        }
        #endregion
        for (int i = 0; i < xValSortItemList.Count; i++)
        {
            var item = xValSortItemList[i];
            item.DefaultShow();
        }
    }

    /// <summary>
    /// 是否开启插值动画来滑到目标
    /// </summary>
    /// <param name="originValue">滑动起点值</param>
    /// <param name="targetValue">滑动目标值</param>
    /// <param name="moveDir">牌右移：1，左移：-1，不移：0</param>
    /// <param name="needTween">是否开启插值动画</param>
    private void LerpTweenToTarget(float originValue, float targetValue, int moveDir, bool needTween = false, bool needTweenOver = true)
    {
        if (!needTween)
        {
            SortItemListByXVal();
            _originHorizontalValue = originValue;
            _curHorizontalValue = targetValue;
            UpdateScrollView(targetValue, moveDir);
            if (needTweenOver)
            {
                OnTweenOver();
            }
        }
        else
        {
            _originHorizontalValue = originValue;
            _curHorizontalValue = targetValue;
            _currentDuration = 0.0f;
        }
        _enableLerpTween = needTween;

    }

    /// <summary>
    /// 滑动更新
    /// </summary>
    /// <param name="targetValue"></param>
    /// <param name="moveDir">牌右移：1，左移：-1，不移：0</param>
    public void UpdateScrollView(float targetValue, int moveDir)
    {
        for (int i = 0; i < _oriItemList.Count; i++)
        {
            LoopScrollViewWithSelectedEffect_Item item = _oriItemList[i];
            float xValue = GetXPosValue(targetValue, item.centerOffSet);
            float scaleValue = GetScaleValue(targetValue, item.centerOffSet);
            float depthValue = GetDepthValue(targetValue, item.centerOffSet);
            item.UpdateScrollViewItems(xValue, moveDir, depthValue, _oriItemList.Count, yFixedPosVal, scaleValue);
        }

    }

    private void Update()
    {
        if (_enableLerpTween)
        {
            TweenViewToTarget();
        }

    }

    /// <summary>
    /// 使用插值动画滑到目标（即把目标转到视框中心）
    /// </summary>
    private void TweenViewToTarget()
    {
        _currentDuration += Time.deltaTime;
        if (_currentDuration > lerpDuration)
        {
            _currentDuration = lerpDuration;
        }
        float percent = _currentDuration / lerpDuration;
        float value = Mathf.Lerp(_originHorizontalValue, _curHorizontalValue, percent);
        UpdateScrollView(value, value > _originHorizontalValue ? 1 : -1);
        if (_currentDuration >= lerpDuration)
        {
            // 此处动画已经播完
            _canChangeCenterItem = true;
            _enableLerpTween = false;
            OnTweenOver();
        }

    }

    /// <summary>
    /// 动画播完后回调
    /// </summary>
    private void OnTweenOver()
    {
        if (_preCenterItem != null)
        {
            _preCenterItem.SetSelectState(false);
        }
        if (_curCenterItem)
        {
            _curCenterItem.SetSelectState(true);
        }

    }

    private float GetScaleValue(float sliderValue, float added)
    {
        float evaluateValue = scaleCurve.Evaluate(sliderValue + added);
        return evaluateValue;

    }

    private float GetXPosValue(float sliderValue, float added)
    {
        // 牌都是默认anchor和pivot，要把移动范围居中，所以在最后减totalHorizontalWidth * 0.5f
        float evaluateValue = positionCurve.Evaluate(sliderValue + added) * _totalHorizontalWidth - _totalHorizontalWidth * 0.5f;
        return evaluateValue;

    }

    private float GetDepthValue(float sliderValue, float added)
    {
        float evaluateValue = depthCurve.Evaluate(sliderValue + added);
        return evaluateValue;

    }

    /// <summary>
    /// 获取指定单位之间的距离，因为是循环滑动，滑到索引5接下来就到索引0，为了让此时两者距离是1而不是5而设计的这个函数
    /// </summary>
    /// <param name="preCenterItem"></param>
    /// <param name="newCenterItem"></param>
    /// <returns></returns>
    private int GetMoveCurveFactorCount(LoopScrollViewWithSelectedEffect_Item preCenterItem, LoopScrollViewWithSelectedEffect_Item newCenterItem)
    {
        SortItemListByXVal();
        int factorCount = newCenterItem.sortXValListIndex - preCenterItem.sortXValListIndex;
        return Mathf.Abs(factorCount);

    }

    private void SortItemListByXVal()
    {
        xValSortItemList.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
        for (int i = xValSortItemList.Count - 1; i >= 0; i--)
        {
            xValSortItemList[i].sortXValListIndex = i;
        }

    }

    /// <summary>
    /// 点击选中的单位滑到中间
    /// </summary>
    /// <param name="selectItem"></param>
    public void SetCenterItem(LoopScrollViewWithSelectedEffect_Item selectItem)
    {
        if (!_canChangeCenterItem)
        {
            return;
        }
        if (_curCenterItem == selectItem)
        {
            return;
        }
        _canChangeCenterItem = false;
        _preCenterItem = _curCenterItem;
        _curCenterItem = selectItem;
        float centerXValue = positionCurve.Evaluate(0.5f) * _totalHorizontalWidth - _totalHorizontalWidth * 0.5f;
        // 选中牌是否在右边
        bool isRight = selectItem.transform.localPosition.x > centerXValue;
        int moveIndexCount = GetMoveCurveFactorCount(_preCenterItem, selectItem);
        float value = isRight ? -_factor : _factor;
        value *= moveIndexCount;
        float targetVal = _curHorizontalValue + value;
        LerpTweenToTarget(_curHorizontalValue, targetVal, targetVal > _curHorizontalValue ? 1 : -1, true);

    }

    public void OnClickRightButton()
    {
        if (!_canChangeCenterItem)
        {
            return;
        }
        int targetIndex = _curCenterItem.oriListIndex + 1;
        if (targetIndex > _oriItemList.Count - 1)
        {
            targetIndex = 0;
        }
        SetCenterItem(_oriItemList[targetIndex]);

    }

    public void OnClickLeftButton()
    {
        if (!_canChangeCenterItem)
        {
            return;
        }
        int targetIndex = _curCenterItem.oriListIndex - 1;
        if (targetIndex < 0)
        {
            targetIndex = _oriItemList.Count - 1;
        }
        SetCenterItem(_oriItemList[targetIndex]);

    }

    public void OnBeginDrag(Vector2 delta)
    {
        if (_preCenterItem)
        {
            _preCenterItem.SetSelectState(false);
        }
        if (_curCenterItem)
        {
            _curCenterItem.SetSelectState(false);
        }

    }

    public void OnDrag(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > 0.0f)
        {
            // eventData.delta值都是整数
            float targetVal = _curHorizontalValue + delta.x * scrollFactor;
            // 因为滑动位置变化小，所以不需要开启插值动画（硬要开启插值动画，会看起来刷新很奇怪）
            LerpTweenToTarget(0.0f, targetVal, targetVal > _curHorizontalValue ? 1 : -1, false, false);
        }

    }

    public void OnEndDrag()
    {
        int closetIndex = 0;
        // 只要小数部分，因为整数部分因为位置x的曲线设置，其实不影响
        float value = _curHorizontalValue - (int)_curHorizontalValue;
        float min = float.MaxValue;
        float tmp = 0.5f * (_curHorizontalValue < 0 ? -1 : 1);
        // 在滑动中，每个单位都在移动，看哪个单位是距离中心位最近的
        for (int i = 0; i < _oriItemList.Count; i++)
        {
            // (tmp - _oriItemList[i].centerOffSet)其实是为了把centerOffSet放大到0-1范围，为什么要放大到0-1范围？因为value在0-1范围。
            float dis = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs(tmp - _oriItemList[i].centerOffSet));
            if (dis < min)
            {
                closetIndex = i;
                min = dis;
            }
        }
        _originHorizontalValue = _curHorizontalValue;
        float target = (int)_curHorizontalValue + (tmp - _oriItemList[closetIndex].centerOffSet);
        _preCenterItem = _curCenterItem;
        _curCenterItem = _oriItemList[closetIndex];
        LerpTweenToTarget(_originHorizontalValue, target, target > _originHorizontalValue ? 1 : -1, true);
        _canChangeCenterItem = false;

    }

    public void SetPreferenceBeforeInit(int dataLength, int startDataCenterIndex = 0)
    {
        this._startCenterDataIndex = startDataCenterIndex;
        if (dataLength < showItemCountInView)
        {
            Debug.LogErrorFormat("数据长度[{0}]应比显示单位总数[{1}]多！", dataLength, showItemCountInView);
        }
        if (startDataCenterIndex > dataLength || startDataCenterIndex < 0)
        {
            Debug.LogErrorFormat("中心显示数据索引[{0}]越界！", startDataCenterIndex);
        }
        _dataLength = dataLength;

    }

    public void SetItemDefaultShow(VoidDelegate callback)
    {
        itemDefaultShowCallback = callback;

    }

    public void SetItemSelectCallback(VoidDelegate callback)
    {
        itemSelectCallback = callback;

    }

    public void SetItemUnSelectCallback(VoidDelegate callback)
    {
        itemUnSelectCallback = callback;

    }

    public void SetItemPassBeginCallback(VoidDelegate callback)
    {
        itemPassBeginCallback = callback;

    }

    public void SetItemPassEndCallback(VoidDelegate callback)
    {
        itemPassEndCallback = callback;

    }

    public void SetItemClickCallback(VoidDelegate callback)
    {
        itemClickCallback = callback;

    }

    public LoopScrollViewWithSelectedEffect_Item GetCurCenterItem()
    {
        if (_curCenterItem)
        {
            return _curCenterItem;
        }
        return null;

    }

    public int GetCurCenterDataIndex()
    {
        if (_curCenterItem)
        {
            return _curCenterItem.dataIndex;
        }
        Debug.LogErrorFormat("GetCurCenterDataIndex: something wrong!");
        return -1;

    }


}
