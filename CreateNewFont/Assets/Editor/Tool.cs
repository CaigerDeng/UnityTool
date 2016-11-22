using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class Tool : EditorWindow
{
    private Texture2D fontTexture = null;
    private static string fontAssetStorePath = "";
    private static string characterCount;
    private static int nowCharacterCount = 0;
    private static int lastCharacterCount = 0;
    private static string[] characterArr;
    private static bool isFinishAllCharacter = true;
    private static bool isShowCreateNewFont = false;

    [@MenuItem("Tool/创建新字体")]
    static void CreateNewFont()
    {
        fontAssetStorePath = Application.dataPath;
        EditorWindow window = EditorWindow.GetWindow(typeof(Tool), false, "创建新字体", true);
        isShowCreateNewFont = true;
    }

    void OnGUI()
    {
        if (isShowCreateNewFont)
        {
            ShowCreateNewFont();
        }

    }

    private void OnDisable()
    {
        isShowCreateNewFont = false;

    }

    private void ShowCreateNewFont()
    {
        GUILayout.Space(5);
        GUILayout.Label("注：请现将字体所用图片切好（这里图片使用的是png格式）！");
        GUILayout.Label("存储路径：" + fontAssetStorePath);
        fontTexture = (Texture2D)EditorGUILayout.ObjectField("Font Texture2D：", fontTexture, typeof(Texture2D), true);
        characterCount = EditorGUILayout.TextField("字符总数：",characterCount);
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
                characterArr[i] = EditorGUILayout.TextField("      字符：", characterArr[i]);
            }
        }
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("创建新字体"))
        {
            if (fontTexture == null)
            {
                EditorUtility.DisplayDialog("创建新字体", "请放入字体所用图片", "Ok");
                return;
            }
            foreach (string item in characterArr)
            {
                if (string.IsNullOrEmpty(item))
                {
                    EditorUtility.DisplayDialog("创建新字体", "请填写完所有字符！", "Ok");
                    return;
                }
            }
            CreateFont();
        }
        GUI.backgroundColor = Color.white;
    }

    private void CreateFont()
    {
        //直接读取fontTexture的meta文件里的数据，生成所需总数和位置
        //这里用的字体图片都是png的格式
        string contentOld = "";
        using (StreamReader sr = File.OpenText(fontAssetStorePath + "/" + fontTexture.name + ".png.meta"))
        {
            contentOld = sr.ReadToEnd();
            sr.Close();
        }
        //count
        int count = 0;
        int startIndex = contentOld.IndexOf("fileIDToRecycleName:") + "fileIDToRecycleName:".Length + 1;
        int endIndex = contentOld.IndexOf("serializedVersion", startIndex);
        string temp = contentOld.Substring(startIndex, endIndex - startIndex);
        while (temp.Contains("\n"))
        {
            int index = temp.IndexOf("\n");
            temp = temp.Remove(index, 1);
            count++;
        }
        if (count != nowCharacterCount)
        {
            EditorUtility.DisplayDialog("创建新字体", "填入字符总数与切图总数不同！", "Ok");
            return;
        }
        //spriteWidth
        startIndex = contentOld.IndexOf("width: ") + "width: ".Length;
        endIndex = contentOld.IndexOf("\n", startIndex);
        float spriteWidth = 0;
        float.TryParse(contentOld.Substring(startIndex, endIndex - startIndex), out spriteWidth);
        //spriteHeight
        startIndex = contentOld.IndexOf("height: ") + "height: ".Length;
        endIndex = contentOld.IndexOf("\n", startIndex);
        float spriteHeight = 0;
        float.TryParse(contentOld.Substring(startIndex, endIndex - startIndex), out spriteHeight);
        //x pos
        List<float> spriteXPosList = new List<float>();
        startIndex = contentOld.IndexOf("rect:");
        endIndex = contentOld.LastIndexOf("\n");
        temp = contentOld.Substring(startIndex, endIndex - startIndex);
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
        //create assets
        Material fontMaterial = new Material(Shader.Find("UI/Default"));
        fontMaterial.mainTexture = fontTexture;
        AssetDatabase.CreateAsset(fontMaterial, "Assets/" + fontTexture.name + ".mat");
        Font fontFont = new Font();
        fontFont.material = fontMaterial;
        List<CharacterInfo> characterInfoList = new List<CharacterInfo>(nowCharacterCount);
        for (int i = 0; i < nowCharacterCount; i++)
        {
            CharacterInfo info = new CharacterInfo();
            System.Text.ASCIIEncoding code = new System.Text.ASCIIEncoding();
            int ascii = (int)code.GetBytes(characterArr[i])[0];
            info.index = ascii;
            float uvx = 1f * spriteXPosList[i] / fontTexture.width;
            float uvy = 0;//简化计算 1 - (1f *y / fontTexture.height)
            float uvw = 1f * spriteWidth / fontTexture.width;
            float uvh = 1;//简化计算   - 1f *y / fontTexture.height)
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
        fontFont.characterInfo = characterInfoList.ToArray();
        AssetDatabase.CreateAsset(fontFont,"Assets/" + fontTexture.name + ".fontsettings");
        EditorUtility.DisplayDialog("创建新字体","创建完成！","Ok");
    }

}
