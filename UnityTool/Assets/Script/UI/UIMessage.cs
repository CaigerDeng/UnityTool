using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMessage 
{
    private static UIMessage self = null;
    public static UIMessage Inst()
    {
        if (self == null)
        {
            self = new UIMessage();
        }
		return self;
    }

	private GameObject prefabGo = null;
    private string prefabName = "";
    private Button closeButton = null;
	private Image headImage = null;
	private Text contentText = null;
	private Button okButton = null;
	

    public UIMessage()
    {
        prefabName = "Message";
    }

    private void InitElement()
    {
		prefabGo = GameObject.Find(prefabName);
        closeButton = prefabGo.transform.Find("ImageBg/closeButton").GetComponent<Button>();
		headImage = prefabGo.transform.Find("ImageBg/headImage").GetComponent<Image>();
		contentText = prefabGo.transform.Find("ImageBg/contentImage/contentText").GetComponent<Text>();
		okButton = prefabGo.transform.Find("ImageBg/okButton").GetComponent<Button>();
		
        
        closeButton.onClick.AddListener(delegate{ClickClose();});
		okButton.onClick.AddListener(delegate{ClickOk();});
		

    }

    private void ClickClose(){}

	private void ClickOk(){}

	

}
