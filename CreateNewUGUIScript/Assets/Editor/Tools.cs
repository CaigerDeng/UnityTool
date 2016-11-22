using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class Tools : EditorWindow
{
    [SerializeField]
    public class MyContent
    {
        public string componentNameInScript = "";
        public string componentType = "";
        public string componentPath = "";
        public string componentFunctionNameInScript = "";
        public bool isShowFold = false;             //show content's details
        public bool isFinishCheckUIType = false;
        public Component myComponent = new Component();

    }

    private static Tools createNewUGUIScriptWindow;
    private string scriptName = "";
    private string prefabName = "";
    private List<MyContent> contentList = new List<MyContent>();
    private Vector2 scrollPos = new Vector2();

    [MenuItem("Tools/Create New UGUI Script")]
    static void CreateNewUGUIScript()
    {
        EditorWindow.GetWindow(typeof(Tools), false, "Create New UGUI Script", true);
    }

    void OnGUI()
    {
        ShowCreateNewUGUIWindow();

    }

    private void ManageScript()
    {
        string strTempPath = Path.Combine(Application.dataPath, "Editor/NewUGUIScript.txt");
        // you can change your script store path here.
        string scriptStorePath = Path.Combine(Application.dataPath, "Scripts/UI/");
        string newScriptPath = scriptStorePath + scriptName + ".cs";
        string content = "";
        string temp0 = @"private #COMPONENT_TYPE# #COMPONENT_NAME# = null;";
        string temp1 = @"#COMPONENT_NAME# = prefabGo.transform.Find(#COMPONENT_PATH#).GetComponent<#COMPONENT_TYPE#>();";
        string temp2 = @"#COMPONENT_NAME#.#ONCLICK#.AddListener(delegate{#FUNCTION_NAME#();});";
        string temp3 = @"private void #FUNCTION_NAME#(){}";
        if (File.Exists(newScriptPath))
        {
            EditorUtility.DisplayDialog("Create New UGUI Script", "Other script gots same name, please rename it.", "Ok");
            return;
        }
        using (StreamReader sr = File.OpenText(strTempPath))
        {
            content = sr.ReadToEnd();
            content = content.Replace("#SCRIPT_NAME#", scriptName);
            content = content.Replace("#PREFAB_NAME#", "\"" + prefabName + "\"");
            for (int i = 0; i < contentList.Count; i++)
            {
                if (contentList[i].myComponent == null)
                {
                    continue;
                }
                if (content.Contains(temp0))
                {
                    content = content.Replace(temp0, "private " + contentList[i].componentType + " " + contentList[i].componentNameInScript + " = null;" + "\r\n" + temp0);
                }
                if (content.Contains(temp1))
                {
                    switch (contentList[i].componentType)
                    {
                        case "GameObject":
                            content = content.Replace(temp1, contentList[i].componentNameInScript + " = prefabGo.transform.Find(\"" + contentList[i].componentPath + "\").gameObject;" + "\r\n" + temp1);
                            break;
                        default:
                            content = content.Replace(temp1, contentList[i].componentNameInScript + " = prefabGo.transform.Find(\"" + contentList[i].componentPath + "\").GetComponent<" + contentList[i].componentType + ">();" + "\r\n" + temp1);
                            break;
                    }

                }
                if (contentList[i].componentFunctionNameInScript != "")
                {
                    if (content.Contains(temp2))
                    {
                        switch (contentList[i].componentType)
                        {
                            case "Button":
                                content = content.Replace(temp2, contentList[i].componentNameInScript + "." + "onClick" + ".AddListener(delegate{" + contentList[i].componentFunctionNameInScript + "();});" + "\r\n" + temp2);
                                break;
                            case "Toggle":
                                content = content.Replace(temp2, contentList[i].componentNameInScript + "." + "onValueChanged" + ".AddListener(delegate{" + "if(" + contentList[i].componentNameInScript + ".isOn)" + contentList[i].componentFunctionNameInScript + "();});" + "\r\n" + temp2);
                                break;
                        }
                    }
                    if (content.Contains(temp3))
                    {
                        content = content.Replace(temp3, "private void " + contentList[i].componentFunctionNameInScript + "(){}" + "\r\n" + temp3);
                    }
                }
            }
            //remove extra
            content = content.Replace(temp0, "");
            content = content.Replace(temp1, "");
            content = content.Replace(temp2, "");
            content = content.Replace(temp3, "");
            sr.Close();
        }
        using (StreamWriter sw = File.CreateText(newScriptPath))
        {
            sw.Write(content);
            sw.Close();
        }
        EditorUtility.DisplayDialog("Create New UGUI Script", "Succeed.", "Ok");
        AssetDatabase.Refresh();
    }

    private void ShowCreateNewUGUIWindow() //Draw the window
    {
        /////////////GUI,GUILayout don't support the common hotkey, like Ctrl + c ///////////////////      
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUILayout.Label("Prefab Name: ", EditorStyles.boldLabel);
        if (Selection.activeObject == null)
        {
            GUILayout.Label("Please choose prefab.");
            EditorGUILayout.EndScrollView();
            return;
        }
        else
        {
            if (PrefabUtility.GetPrefabType(Selection.activeObject) == PrefabType.PrefabInstance)
            {

                prefabName = Selection.activeGameObject.name;
                GUI.contentColor = Color.yellow;
                GUILayout.Label("   " + prefabName);
                GUI.contentColor = Color.white;
            }
            else
            {
                GUILayout.Label("Please choose prefab.");
                EditorGUILayout.EndScrollView();
                return;
            }
        }
        GUILayout.Label("Path: ", EditorStyles.boldLabel);
        GUILayout.Label("   Scripts/UI/");
        GUILayout.Label("Script Name: ", EditorStyles.boldLabel);
        scriptName = EditorGUILayout.TextField(scriptName);

        GUILayout.Label("Contents: ", EditorStyles.boldLabel);
        for (int i = 0; i < contentList.Count; i++)
        {
            GUILayout.BeginVertical();
            contentList[i].myComponent = (Component)EditorGUILayout.ObjectField("Content" + i, contentList[i].myComponent, typeof(Component), true);
            if (contentList[i].myComponent)
            {
                //put component
                contentList[i].isShowFold = EditorGUILayout.Foldout(contentList[i].isShowFold, "---Details" + i);
                if (!contentList[i].isFinishCheckUIType)
                {
                    contentList[i].componentType = CheckUIType(contentList[i].myComponent);
                    contentList[i].isFinishCheckUIType = true;
                }
                //calculate the path, remove the root transform name
                if (contentList[i].myComponent.transform != contentList[i].myComponent.transform.root)
                {
                    string str = AnimationUtility.CalculateTransformPath(contentList[i].myComponent.transform, contentList[i].myComponent.transform.root);
                    int index = str.IndexOf("/");
                    str = str.Remove(0, index + 1);
                    contentList[i].componentPath = str;
                    if (str == "")
                    {
                        Debug.LogError("componentPath is null.");
                    }
                }
                //show details

                if (contentList[i].isShowFold)
                {
                    GUILayout.Label("component name in script:");
                    contentList[i].componentNameInScript = EditorGUILayout.TextField(contentList[i].componentNameInScript);
                    GUILayout.Label("funcion name in script ( can ignore when no function):");
                    contentList[i].componentFunctionNameInScript = EditorGUILayout.TextField(contentList[i].componentFunctionNameInScript);
                }
                if (contentList[i].componentNameInScript == "")
                {
                    contentList[i].componentNameInScript = contentList[i].myComponent.name;
                }
            }
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Content"))
        {
            MyContent info = new MyContent();
            contentList.Add(info);
        }
        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Finish And Create Script"))
        {
            if (scriptName == "")
            {
                EditorUtility.DisplayDialog("Create New UGUI Script", "Please enter script name.", "Ok");
                return;
            }
            ManageScript();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();

    }

    private string CheckUIType(Component com)
    {
        string[] uiTypeArr = { "Button", "Toggle", "Slider", "Scrollbar", "Dropdown", "InputField", "Image", "RawImage", "Text" };
        for (int i = 0; i < uiTypeArr.Length; i++)
        {
            if (com.GetComponent(uiTypeArr[i]))
            {
                Debug.Log("uiTypeArr[i] " + uiTypeArr[i]);
                return uiTypeArr[i];
            }
        }
        return "GameObject";
    }


    ///////////middle button event////////////
    [InitializeOnLoadMethod]
    static void StartInitializeLoadMethod()
    {
        EditorApplication.hierarchyWindowItemOnGUI += MineMiddleHierarchyGUI;
    }

    static void MineMiddleHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (Event.current != null && selectionRect.Contains(Event.current.mousePosition) && Event.current.button == 2 &&
            Event.current.type <= EventType.MouseUp)
        {
            GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (selectedGameObject)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                EditorUtility.DisplayPopupMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), "Tools", null);
                Event.current.Use();
            }
        }
    }

}
