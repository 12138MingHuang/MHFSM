using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace MHFSM
{
    public class FSMStateCreator : EndNameEditAction
    {

        private static string _regex = "^[a-zA-Z][a-zA-Z0-9_]*$";
        
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            CreateFSMState(pathName);
        }

        internal static bool CreateFSMState(string pathName, bool errorTip = true)
        {
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            
            string[] guids = AssetDatabase.FindAssets(fileName);

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (script == null || script.GetClass() == null) continue;
                if (script.GetClass().Name.Equals(fileName))
                {
                    if (errorTip)
                    {
                        string message = $"脚本名称{fileName}已经存在!";
                        EditorUtility.DisplayDialog("提示", message, "确定");
                    }
                }
            }

            if (!Regex.Match(fileName, _regex).Success)
            {
                EditorUtility.DisplayDialog("提示", $"文件名:{fileName}不可用!,必须以字母开头，只能包含字母、数字和下划线!", "确定");
                return false;
            }

            TextAsset template = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Editor/Template/FSMStateTemplate.txt");
            if (template == null)
            {
                template = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.zb.mhfsm/Editor/Template/FSMStateTemplate.txt");
            }
            string content = template.text;
            
            content = content.Replace("{0}", fileName);

            FileStream stream = File.Create(pathName);
            stream.Write(Encoding.UTF8.GetBytes(content), 0, content.Length);
            stream.Close();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            
            return true;
        }
    }
}
