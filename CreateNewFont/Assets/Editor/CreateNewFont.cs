using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CreateNewFont : EditorWindow
{
    private Vector2 scrollPos;
    private Texture2D fontTexture = null;
    private string characterCount;
    private int nowCharacterCount = 0;
    private int lastCharacterCount = -1;
    private string[] characterArr;
    private bool isFinishAllCharacter = true;

    [@MenuItem("Tool/创建新图片字体")]
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
        GUILayout.Space(20);
        GUILayout.Label("请先准备已切好字体图片的sprite，然后将图片拖入下方图片框中");
        GUILayout.Label("新图片字体会存储在字体图片所在文件夹中");
        fontTexture = (Texture2D)EditorGUILayout.ObjectField("", fontTexture, typeof(Texture2D), true);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        characterCount = EditorGUILayout.TextField("字符总数：", characterCount);
        int.TryParse(characterCount, out nowCharacterCount);
        if (nowCharacterCount > 0)
        {
            if (nowCharacterCount != lastCharacterCount)
            {
                characterArr = new string[nowCharacterCount];
                lastCharacterCount = nowCharacterCount;
            }
            for (int i = 0; i < nowCharacterCount; i++)
            {
                characterArr[i] = EditorGUILayout.TextField(string.Format("第{0}个字符", i), characterArr[i]);
            }
        }

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("创建"))
        {
            if (fontTexture == null)
            {
                EditorUtility.DisplayDialog("创建新图片字体", "请放入字体用图片", "确定");
                return;
            }
            if (characterArr == null || characterArr.Length == 0)
            {
                EditorUtility.DisplayDialog("创建新图片字体", "请输入所需字符总数", "确定");
                return;
            }
            foreach (string item in characterArr)
            {
                if (string.IsNullOrEmpty(item))
                {
                    EditorUtility.DisplayDialog("创建新图片字体", "请输入所有所需字符", "确定");
                    return;
                }
            }
            CreateFont();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }

    private void CreateFont()
    {
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

        // count
        int count = 0;
        int startIndex = metaContent.IndexOf("fileIDToRecycleName:") + "fileIDToRecycleName:".Length + 1;
        int endIndex = metaContent.IndexOf("externalObjects", startIndex);
        string temp = metaContent.Substring(startIndex, endIndex - startIndex);
        while (temp.Contains("\n"))
        {
            int index = temp.IndexOf("\n");
            temp = temp.Remove(index, 1);
            count++;
        }
        if (nowCharacterCount != count)
        {
            EditorUtility.DisplayDialog("创建新图片字体", string.Format("填入字符总数{0}与切图总数{1}不同", nowCharacterCount, count), "确定");
            return;
        }

        // spriteWidth
        float spriteWidth = 0;
        startIndex = metaContent.IndexOf("width: ") + "width: ".Length;
        endIndex = metaContent.IndexOf("\n", startIndex);
        float.TryParse(metaContent.Substring(startIndex, endIndex - startIndex), out spriteWidth);

        // spriteHeight
        float spriteHeight = 0;
        startIndex = metaContent.IndexOf("height: ") + "height: ".Length;
        endIndex = metaContent.IndexOf("\n", startIndex);
        float.TryParse(metaContent.Substring(startIndex, endIndex - startIndex), out spriteHeight);

        // x pos
        List<float> spriteXPosList = new List<float>();
        startIndex = metaContent.IndexOf("rect:");
        endIndex = metaContent.LastIndexOf("\n");
        temp = metaContent.Substring(startIndex, endIndex - startIndex);
        while (temp.Contains("rect:"))
        {
            int index = temp.IndexOf("rect:");
            int xStartIndex = temp.IndexOf("x: ", index) + "x: ".Length;
            int xEndIndex = temp.IndexOf("\n", xStartIndex);
            float x = 0;
            float.TryParse(temp.Substring(xStartIndex, xEndIndex - xStartIndex), out x);
            temp = temp.Remove(index, "rect:".Length);
            spriteXPosList.Add(x);
        }

        // create assets
        Material fontMaterial = new Material(Shader.Find("UI/Default"));
        fontMaterial.mainTexture = fontTexture;
        string matPath = storePath + fontTexture.name + ".mat";
        AssetDatabase.CreateAsset(fontMaterial, matPath);
        Font font = new Font();
        font.material = fontMaterial;
        List<CharacterInfo> characterInfoList = new List<CharacterInfo>(nowCharacterCount);
        for (int i = 0; i < nowCharacterCount; i++)
        {
            CharacterInfo info = new CharacterInfo();
            System.Text.ASCIIEncoding code = new System.Text.ASCIIEncoding();
            int ascii = (int)code.GetBytes(characterArr[i])[0];
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

}
