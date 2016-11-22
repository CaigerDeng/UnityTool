using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CPanel
{
    private static CPanel self = null;
    public static CPanel Inst()
    {
        if (self == null)
        {
            self = new CPanel();
        }
        return self;
    }

    private GameObject prefabGo = null;
    private string prefabName = "";
    private Text text = null;
    private Button button = null;


    public CPanel()
    {
        prefabName = "Panel";
    }

    private void InitElement()
    {
        prefabGo = GameObject.Find(prefabName);
        text = prefabGo.transform.Find("ImageBg/Text").GetComponent<Text>();
        button = prefabGo.transform.Find("ImageBg/Button").GetComponent<Button>();


        button.onClick.AddListener(delegate { ClickButton(); });


    }

    private void ClickButton() { }


}
