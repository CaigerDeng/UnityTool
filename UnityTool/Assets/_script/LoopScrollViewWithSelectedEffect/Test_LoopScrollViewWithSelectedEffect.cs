using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Test_LoopScrollViewWithSelectedEffect : MonoBehaviour
{
    public LoopScrollViewWithSelectedEffect scrollView;


    void Start()
    {
        if (scrollView != null)
        {
            scrollView.SetPreferenceBeforeInit(10, 1);
            scrollView.SetItemDefaultShow((itemSelf) =>
            {
                if (itemSelf.GetComponentInChildren<Text>())
                {
                    itemSelf.GetComponentInChildren<Text>().text = itemSelf.dataIndex.ToString();

                }

            });

            scrollView.SetItemSelectCallback((itemSelf) =>
            {
                if (itemSelf.GetComponent<Image>())
                {
                    itemSelf.GetComponent<Image>().color = Color.green;
                }

            });

            scrollView.SetItemUnSelectCallback((itemSelf) =>
            {
                if (itemSelf.GetComponent<Image>())
                {
                    itemSelf.GetComponent<Image>().color = Color.gray;
                }

            });

            scrollView.SetItemPassBeginCallback((itemSelf) =>
            {
                // 刷新数据
                if (itemSelf.GetComponentInChildren<Text>())
                {
                    itemSelf.GetComponentInChildren<Text>().text = itemSelf.dataIndex.ToString();
                }
            });

            scrollView.SetItemPassEndCallback((itemSelf) =>
            {
                // 刷新数据
                if (itemSelf.GetComponentInChildren<Text>())
                {
                    itemSelf.GetComponentInChildren<Text>().text = itemSelf.dataIndex.ToString();
                }
            });

            scrollView.Init();
        }


    }





}
