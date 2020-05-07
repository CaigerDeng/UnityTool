using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class CreateNewUGUIScript : EditorWindow
{
    [SerializeField]
    public class UIContent
    {
        public string comNameInScript = "";
        public string comType = "";
        public string comPath = "";
        public string comFuncNameInScript = "";
        public bool isShowFold = false;             //show content's details
        public bool isFinishCheckUIType = false;
        public Component myCom = new Component();

    }

    private string scriptName = "";
    private string prefabName = "";
    private List<UIContent> contentList = new List<UIContent>();
    private Vector2 scrollPos = new Vector2();

    [MenuItem("Tool/CreateNewUGUIScript")]
    static void Tool_CreateNewUGUIScript()
    {
        EditorWindow.GetWindow(typeof(CreateNewUGUIScript), false, "CreateNewUGUIScript", true);
    }

    void OnGUI()
    {
        ShowCreateNewUGUIWindow();
    }

    private void ManageScript()
    {
        string strTempPath = Path.Combine(Application.dataPath, "Editor/CreateNewUGUIScript/NewUGUIScriptTemplate.txt");
        string scriptStorePath = Path.Combine(Application.dataPath, "Script/UI/");
        string newScriptPath = scriptStorePath + scriptName + ".cs";
        string content = "";
        string comDeclTmp = @"private #COMPONENT_TYPE# #COMPONENT_NAME# = null;";
        string comFindTmp = @"#COMPONENT_NAME# = prefabGo.transform.Find(#COMPONENT_PATH#).GetComponent<#COMPONENT_TYPE#>();";
        string comClickTmp = @"#COMPONENT_NAME#.#ONCLICK#.AddListener(delegate{#FUNCTION_NAME#();});";
        string funcTmp = @"private void #FUNCTION_NAME#(){}";
        if (File.Exists(newScriptPath))
        {
            EditorUtility.DisplayDialog("CreateNewUGUIScript", "Other script gots same name, please rename it", "Ok");
            return;
        }
        using (StreamReader sr = File.OpenText(strTempPath))
        {
            content = sr.ReadToEnd();
            content = content.Replace("#SCRIPT_NAME#", scriptName);
            content = content.Replace("#PREFAB_NAME#", "\"" + prefabName + "\"");
            for (int i = 0; i < contentList.Count; i++)
            {
                if (contentList[i].myCom == null)
                {
                    continue;
                }
                if (content.Contains(comDeclTmp))
                {
                    content = content.Replace(comDeclTmp, "private " + contentList[i].comType + " " + contentList[i].comNameInScript + " = null;" + "\r\n\t" + comDeclTmp);
                }
                if (content.Contains(comFindTmp))
                {
                    switch (contentList[i].comType)
                    {
                        case "GameObject":
                            content = content.Replace(comFindTmp, contentList[i].comNameInScript + " = prefabGo.transform.Find(\"" + contentList[i].comPath + "\").gameObject;" + "\r\n\t\t" + comFindTmp);
                            break;
                        default:
                            content = content.Replace(comFindTmp, contentList[i].comNameInScript + " = prefabGo.transform.Find(\"" + contentList[i].comPath + "\").GetComponent<" + contentList[i].comType + ">();" + "\r\n\t\t" + comFindTmp);
                            break;
                    }
                }
                if (contentList[i].comFuncNameInScript != "")
                {
                    if (content.Contains(comClickTmp))
                    {
                        switch (contentList[i].comType)
                        {
                            case "Button":
                                content = content.Replace(comClickTmp, contentList[i].comNameInScript + "." + "onClick" + ".AddListener(delegate{" + contentList[i].comFuncNameInScript + "();});" + "\r\n\t\t" + comClickTmp);
                                break;
                            case "Toggle":
                                content = content.Replace(comClickTmp, contentList[i].comNameInScript + "." + "onValueChanged" + ".AddListener(delegate{" + "if(" + contentList[i].comNameInScript + ".isOn)" + contentList[i].comFuncNameInScript + "();});" + "\r\n\t\t" + comClickTmp);
                                break;
                        }
                    }
                    if (content.Contains(funcTmp))
                    {
                        content = content.Replace(funcTmp, "private void " + contentList[i].comFuncNameInScript + "(){}" + "\r\n\n\t" + funcTmp);
                    }
                }
            }
            //remove extra
            content = content.Replace(comDeclTmp, "");
            content = content.Replace(comFindTmp, "");
            content = content.Replace(comClickTmp, "");
            content = content.Replace(funcTmp, "");
            sr.Close();
        }
        using (StreamWriter sw = File.CreateText(newScriptPath))
        {
            sw.Write(content);
            sw.Close();
        }
        EditorUtility.DisplayDialog("CreateNewUGUIScript", "Succeed", "Ok");
        AssetDatabase.Refresh();
    }

    private void ShowCreateNewUGUIWindow()
    {
        /////////////GUI, GUILayout don't support the common hotkey, like Ctrl + c ///////////////////      
        if (Selection.activeObject == null || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.PrefabInstance)
        {
            GUILayout.Label("Please choose prefab in hierarchy.");
            return;
        }
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUILayout.BeginHorizontal();
        prefabName = Selection.activeGameObject.name;
        GUILayout.Label("Prefab Name", EditorStyles.boldLabel);
        GUI.contentColor = Color.yellow;
        GUILayout.Label(prefabName, EditorStyles.boldLabel);
        GUI.contentColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("ScriptName", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField(scriptName);
        GUILayout.EndHorizontal();

        GUILayout.Label("Script Store Path: Script/UI/", EditorStyles.boldLabel);
        GUILayout.Label("Contents", EditorStyles.boldLabel);

        for (int i = 0; i < contentList.Count; i++)
        {
            GUILayout.BeginVertical();
            contentList[i].myCom = (Component)EditorGUILayout.ObjectField("Content" + i, contentList[i].myCom, typeof(Component), true);
            if (contentList[i].myCom)
            {
                //put component
                contentList[i].isShowFold = EditorGUILayout.Foldout(contentList[i].isShowFold, "--- ");
                if (!contentList[i].isFinishCheckUIType)
                {
                    contentList[i].comType = CheckUIType(contentList[i].myCom);
                    contentList[i].isFinishCheckUIType = true;
                }
                //calculate the path, remove the root transform name
                if (contentList[i].myCom.transform != null)
                {
                    Transform rootTrans = contentList[i].myCom.transform.parent;
                    while (true)
                    {
                        if (rootTrans.GetComponent<Canvas>() != null)
                        {
                            break;
                        }
                        rootTrans = rootTrans.parent;
                    }
                    string str = AnimationUtility.CalculateTransformPath(contentList[i].myCom.transform, rootTrans);
                    int index = str.IndexOf("/");
                    str = str.Remove(0, index + 1);
                    contentList[i].comPath = str;
                    if (str == "")
                    {
                        Debug.LogError("componentPath is null.");
                    }
                }
                //show details
                if (contentList[i].isShowFold)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name");
                    contentList[i].comNameInScript = EditorGUILayout.TextField(contentList[i].comNameInScript);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Function Name(CanIgnore)");
                    contentList[i].comFuncNameInScript = EditorGUILayout.TextField(contentList[i].comFuncNameInScript);
                    GUILayout.EndHorizontal();
                }
                if (contentList[i].comNameInScript == "")
                {
                    contentList[i].comNameInScript = contentList[i].myCom.name;
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.Space(5);
        if (GUILayout.Button("Add Content"))
        {
            UIContent info = new UIContent();
            contentList.Add(info);
        }
        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Finish And Create Script"))
        {
            if (scriptName == "")
            {
                EditorUtility.DisplayDialog("CreateNewUGUIScript", "Please enter script name", "Ok");
                return;
            }
            ManageScript();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }

    private string CheckUIType(Component com)
    {
        // 此处有序，比如Button优先级比Image高
        string[] uiTypeArr = { "Button", "Toggle", "Slider", "Scrollbar", "Dropdown", "InputField", "Image", "RawImage", "Text" };
        for (int i = 0; i < uiTypeArr.Length; i++)
        {
            if (com.GetComponent(uiTypeArr[i]))
            {
                return uiTypeArr[i];
            }
        }
        return "GameObject";
    }


    ///////////middle button event////////////
    [InitializeOnLoadMethod]
    static void StartInitializeLoadMethod()
    {
        EditorApplication.hierarchyWindowItemOnGUI += MiddleHierarchyGUI;
    }

    // 点击鼠标中键打开Tool列表
    static void MiddleHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (Event.current != null && selectionRect.Contains(Event.current.mousePosition) && Event.current.button == 2 && Event.current.type <= EventType.MouseUp)
        {
            GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (selectedGameObject)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                EditorUtility.DisplayPopupMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), "Tool", null);
                Event.current.Use();
            }
        }
    }

}
