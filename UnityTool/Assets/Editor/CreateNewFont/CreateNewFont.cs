using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Enumerable = System.Linq.Enumerable;

public class CreateNewFont : EditorWindow
{
    private Vector2 scrollPos;
    private Texture2D fontTexture = null;
    private string allCharacterContent = "";
    private List<char> characterList = new List<char>();
    private bool isFinishAllCharacter = true;

    [@MenuItem("Tool/CreateNewFont")]
    static void Tool_CreateNewFont()  
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(CreateNewFont), false, "创建新图片字体", true);
    }

    void OnGUI()
    {
        ShowWindow();
    }

    private void ShowWindow()
    {
        GUILayout.Space(10);
        GUILayout.Label("# 请先准备已切好字体图片的sprite，然后将图片拖入下方图片框中");
        GUILayout.Label("# 新图片字体会存储在字体图片所在文件夹中");
        fontTexture = (Texture2D)EditorGUILayout.ObjectField("", fontTexture, typeof(Texture2D), true);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        allCharacterContent = EditorGUILayout.TextField("请输入所有字符内容：", allCharacterContent, GUILayout.Width(380));         
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("创建"))
        {
            if (fontTexture == null)
            {
                EditorUtility.DisplayDialog("创建新图片字体", "请放入字体用图片", "确定");
                return;
            }
            if (string.IsNullOrEmpty(allCharacterContent))
            {
                EditorUtility.DisplayDialog("创建新图片字体", "请输入所需字符总数", "确定");
                return;
            }           
            CreateFont();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();

    }

    private void CreateFont()
    {
        characterList = allCharacterContent.ToList();

        // 直接读取fontTexture的meta文件里的数据，生成所需总数和位置
        // 这里用的字体图片是png格式，并且sprite中已经设为Multiple
        string metaContent = "";
        string imagePath = AssetDatabase.GetAssetPath(fontTexture);
        string storePath = imagePath.Replace(fontTexture.name + ".png", "");
        string metaPath = Application.dataPath + imagePath.Replace("Assets", "") + ".meta";
        using (StreamReader sr = File.OpenText(metaPath))
        {
            metaContent = sr.ReadToEnd();
            sr.Close();
        }
        string temp;
        string[] arr = metaContent.Split('\n');
        int startIndex = Find(arr, 0, "fileIDToRecycleName", out temp);   
        int endIndex = Find(arr, 0, "externalObjects", out temp); 
        int spriteCount = endIndex - startIndex - 1;
        if (characterList.Count != spriteCount)
        {
            EditorUtility.DisplayDialog("创建新图片字体", string.Format("填入字符总数{0}与切图总数{1}不同", characterList.Count, spriteCount), "确定");
            return;
        }

        // spriteWidth
        float spriteWidth = 0;
        Find(arr, 0, "width: ", out temp); 
        startIndex = temp.IndexOf("width: ") + "width: ".Length;
        endIndex = temp.Length - 1;
        float.TryParse(temp.Substring(startIndex, endIndex - startIndex + 1), out spriteWidth);

        // spriteHeight
        float spriteHeight = 0;
        Find(arr, 0, "height: ", out temp);
        startIndex = temp.IndexOf("height: ") + "height: ".Length;
        endIndex = temp.Length - 1;
        float.TryParse(temp.Substring(startIndex, endIndex - startIndex), out spriteHeight);

        // x pos
        List<float> spriteXPosList = new List<float>();
        startIndex = Find(arr, 0, "rect:", out temp);   
        while (startIndex != -1)
        {
            startIndex = Find(arr, startIndex, "x:", out temp);
            int xStartIndex = temp.IndexOf("x: ") + "x: ".Length;
            int xEndIndex = temp.Length - 1;
            float x = 0;
            float.TryParse(temp.Substring(xStartIndex, xEndIndex - xStartIndex), out x);           
            spriteXPosList.Add(x);
            startIndex = Find(arr, startIndex, "rect:", out temp);
        }

        // create assets
        Material fontMaterial = new Material(Shader.Find("UI/Default"));
        fontMaterial.mainTexture = fontTexture;
        string matPath = storePath + fontTexture.name + ".mat";
        AssetDatabase.CreateAsset(fontMaterial, matPath);
        Font font = new Font();
        font.material = fontMaterial;
        List<CharacterInfo> characterInfoList = new List<CharacterInfo>(characterList.Count);
        for (int i = 0; i < characterList.Count; i++)
        {
            CharacterInfo info = new CharacterInfo();
            string s = characterList[i].ToString();
            System.Text.ASCIIEncoding code = new System.Text.ASCIIEncoding();
            int ascii = (int)code.GetBytes(s)[0];
            info.index = ascii;
            float uvx = 1f * spriteXPosList[i] / fontTexture.width;
            float uvy = 0; // 简化计算 1 - (1f * y / fontTexture.height)
            float uvw = 1f * spriteWidth / fontTexture.width;
            float uvh = 1; // 简化计算   - 1f * y / fontTexture.height)
            info.uvBottomLeft = new Vector2(uvx, uvy);
            info.uvBottomRight = new Vector2(uvx + uvw, uvy);
            info.uvTopLeft = new Vector2(uvx, uvy + uvh);
            info.uvTopRight = new Vector2(uvx + uvw, uvy + uvh);
            info.minX = 0;
            info.minY = -(int)spriteWidth;
            info.glyphWidth = (int)spriteWidth;
            info.glyphHeight = (int)spriteHeight;
            info.advance = (int)spriteWidth;
            characterInfoList.Add(info);
        }
        font.characterInfo = characterInfoList.ToArray();
        string fontPath = storePath + fontTexture.name + ".fontsettings";
        AssetDatabase.CreateAsset(font, fontPath);
        EditorUtility.DisplayDialog("创建新图片字体", "创建完成", "确定");

    }

    private int Find(string[] arr, int startIndex, string str, out string fullStr)
    {
        for (int i = startIndex; i < arr.Length; i++)
        {
            if (arr[i].Contains(str))
            {
                fullStr = arr[i];
                return i;
            }
        }
        fullStr = "";
        return -1;
    }

}
