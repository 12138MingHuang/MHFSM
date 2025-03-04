using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class MHFSMMenu
    {
        // 这里不使用CreateAssetMenu来创建配置的原因，是因为创建完成之后，需要对配置添加一些默认数据
        [MenuItem("Assets/Create/MHFSM/FSMController")]
        private static void CreateFSM()
        {
            RuntimeFSMControllerCreator runtimeFSMControllerCreator = ScriptableObject.CreateInstance<RuntimeFSMControllerCreator>();
            
            string fileName = GetName();

#if UNITY_2018
            GUIContent content = EditorGUIUtility.IconContent("icons/processed/unityengine/billboardasset icon.asset");    
#elif UNITY_2019_1_OR_NEWER
            GUIContent content = EditorGUIUtility.IconContent("d_ScriptableObject Icon");
#endif

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(runtimeFSMControllerCreator.GetInstanceID(), 
                runtimeFSMControllerCreator, fileName, (Texture2D)content.image, null);
        }

        /// <summary>
        /// 获取一个不重复的名字，这个名字在项目中是唯一的
        /// </summary>
        /// <param name="templateName"> 模板名字</param>
        /// <param name="suffix"> 后缀</param>
        /// <returns> 不重复的名字</returns>
        private static string GetName(string templateName = "New FSMController", string suffix = "asset")
        {
            string name;
            string[] files;

            int i = 0;

            do
            {
                name = $"{templateName}{(i == 0 ? string.Empty : i.ToString())}";
                files = AssetDatabase.FindAssets(name);

                i++;
            } while (files != null && files.Length != 0);
            
            return $"{name}.{suffix}";
        }

        [MenuItem("Window/MHFSM/FSMController")]
        static void StateMachineWindow()
        {
            // TODO: 创建窗口
        }

        [MenuItem("Assets/Create/MHFSM/FSMState")]
        static void CreateFSMState()
        {
            // TODO: 创建FSMState
        }

        [MenuItem("Window/MHFSM/About", false, 5000)]
        static void About()
        {
            // TODO: 关于窗口
        }
    }
}