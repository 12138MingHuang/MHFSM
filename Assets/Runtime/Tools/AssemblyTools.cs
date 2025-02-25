using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace MHFSM
{
    /// <summary>
    /// 程序集工具
    /// </summary>
    public class AssemblyTool
    {
        private static Dictionary<string, Type> _typeCaches = new Dictionary<string, Type>();

        /// <summary>
        /// 根据名称来获取类型信息，如果找不到则返回null。
        /// </summary>
        /// <param name="classFullName"> 类型全名(含命名空间)</param>
        /// <returns> 类型信息 </returns>
        internal static Type GetType(string classFullName)
        {
            if(string.IsNullOrEmpty(classFullName)) return null;
            
            if(_typeCaches.ContainsKey(classFullName) && _typeCaches[classFullName] != null) return _typeCaches[classFullName];
            
            _typeCaches.Remove(classFullName);
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                Type type = assembly.GetType(classFullName);
                if(type != null)
                {
                    _typeCaches.Add(classFullName, type);
                    break;
                }
            }
            
            return _typeCaches.GetValueOrDefault(classFullName);

        }

#if UNITY_EDITOR

        /// <summary>
        /// 获取所有状态脚本(仅仅编辑器有效)
        /// </summary>
        /// <returns></returns>
        public static List<MonoScript> GetAllStatesType()
        {
            List<MonoScript> states = new List<MonoScript>();
            
            string[] scripts = AssetDatabase.FindAssets("t:Script");

            foreach (string script in scripts)
            {
                string path = AssetDatabase.GUIDToAssetPath(script);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                
                if(monoScript == null) continue;

                Type type = monoScript.GetClass();
                
                if(type == null) continue;
                
                if(type.IsSubclassOf(typeof(FSMState)))
                   states.Add(monoScript);
            }
            
            return states;
        }

        /// <summary>
        /// 根据状态脚本全名称获取脚本guid(仅编辑器有效)
        /// </summary>
        /// <param name="fullName"> 状态脚本全名称 </param>
        /// <returns> 脚本guid </returns>
        public static string GetGUIDByStateClassFullName(string fullName)
        {
            List<MonoScript> scripts = GetAllStatesType();

            foreach (MonoScript script in scripts)
            {
                if (script.GetClass() != null && script.GetClass().FullName!.Equals(fullName))
                {
                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
                }
            }
            
            return string.Empty;
        }
#endif
    }
}