using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    [Serializable]
    public class FSMStateScriptInfo
    {
        public string className;
        public string guid;
    }
    
    [Serializable]
    public class FSMStateNodeData
    {
        /// <summary>
        /// 是否为默认状态
        /// </summary>
        public bool defaultState;
        
        /// <summary>
        /// 状态名称
        /// </summary>
        public string name;
        
        /// <summary>
        /// 状态脚本名称
        /// </summary>
        public List<FSMStateScriptInfo> StateScripts = new List<FSMStateScriptInfo>();

        /// <summary>
        /// 当前状态的父节点名称列表
        /// </summary>
        public List<string> parents;

        /// <summary>
        /// 是否为子状态机
        /// </summary>
        public bool isSubStateMachine = false;

        /// <summary>
        /// 是否为内置状态
        /// </summary>
        public bool isBuildInState = false;

        /// <summary>
        /// 内置状态名称
        /// </summary>
        public string buildInStateName = null;

        #region 属性

        /// <summary>
        /// 判断当前状态是否为Any状态
        /// </summary>
        public bool IsAnyState
        {
            get
            {
                if(name.Equals(FSMConst.AnyState))
                    return true;
                
                if(isBuildInState && buildInStateName.Equals(FSMConst.AnyState))
                    return true;
                
                return false;
            }
        }

        /// <summary>
        /// 判断当前状态是否为Entry状态
        /// </summary>
        public bool IsEntryState
        {
            get
            {
                if(name.Equals(FSMConst.EntryState))
                    return true;
                
                if(isBuildInState && buildInStateName.Equals(FSMConst.EntryState))
                    return true;
                
                return false;
            }
        }

        /// <summary>
        /// 判断是否为返回上一层的按钮
        /// </summary>
        public bool IsUpstate
        {
            get
            {
                if(isBuildInState && buildInStateName.Equals(FSMConst.Up))
                    return true;
                
                return false;
            }
        }

        /// <summary>
        /// 显示名称，用于显示在编辑器中。
        /// </summary>
        public string DisplayName
        {
            get
            {
                if(IsAnyState)
                    return FSMConst.AnyState;
                if(IsEntryState)
                  return FSMConst.EntryState;
                if (IsUpstate)
                {
                    if (parents.Count > 1)
                    {
                        return $"(Up){parents[^2]}";
                    }
                    else
                    {
                        return "(Up)BaseLayer";
                    }
                }
                
                return name;
            }
        }

        /// <summary>
        /// 父节点路径，用于在编辑器中显示。例如：A/B/C 表示 A->B->C 的路径。
        /// </summary>
        public string ParentPath
        {
            get
            {
                if(parents == null || parents.Count == 0) return string.Empty;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < parents.Count; i++)
                {
                    sb.Append(parents[i]);
                    if (i < parents.Count - 1)
                        sb.Append("/");
                }
                
                return sb.ToString();
            }
        }

        /// <summary>
        /// 下一父节点路径
        /// </summary>
        public string NextParentPath
        {
            get
            {
                List<string> nextParents = new List<string>(parents);

                if (parents != null && parents.Count != 0) nextParents.AddRange(parents);
                
                nextParents.Add(name);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < nextParents.Count; i++)
                {
                    sb.Append(nextParents[i]);
                    if (i < nextParents.Count - 1)
                        sb.Append("/");
                }
                
                return sb.ToString();
            }
        }

        /// <summary>
        /// 父节点名称，用于在编辑器中显示。
        /// </summary>
        public string Parent
        {
            get
            {
                if(parents == null || parents.Count == 0) return string.Empty;

                return parents[^1];
            }
        }
        
        #endregion

        /// <summary>
        /// 比较父节点是否相同。如果父节点为空，则认为是顶层状态。
        /// </summary>
        /// <param name="parents"> 父节点列表 </param>
        /// <returns> 是否相同 </returns>
        public bool CompareParent(List<string> parents)
        {
            if(BaseLayer()) return null == parents || 0 == parents.Count;

            if (parents == null || this.parents == null) return false;
            
            if (parents.Count != this.parents.Count) return false;

            for (int i = 0; i < this.parents.Count; i++)
            {
                if(!this.parents[i].Equals(parents[i]))
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// 判断是否为顶层状态。顶层状态的父节点列表为空或长度为0。
        /// </summary>
        /// <returns> 是否为顶层状态 </returns>
        public bool BaseLayer()
        {
            return parents == null || parents.Count == 0;
        }

        /// <summary>
        /// 判断是否包含某个父节点。
        /// </summary>
        /// <param name="parent"> 父节点名称 </param>
        /// <returns> 是否包含 </returns>
        public bool ContainsParent(string parent)
        {
            if(BaseLayer()) return false;

            return parents.Contains(parent);
        }

        /// <summary>
        /// 判断是否属于某个父节点。
        /// </summary>
        /// <param name="parent"> 父节点名称 </param>
        /// <returns> 是否属于 </returns>
        public bool BelongToParent(string parent)
        {
            if(BaseLayer()) return false;
            
            return parents[^1].Equals(parent);
        }

        /// <summary>
        /// 重命名父节点。
        /// </summary>
        /// <param name="oldParent"> 旧父节点名称 </param>
        /// <param name="newName"> 新父节点名称 </param>
        public void RenameParent(string oldParent, string newName)
        {
            int index = parents.IndexOf(oldParent);
            parents[index] = newName;
        }

        /// <summary>
        /// 获取父节点列表的副本。如果父节点列表为空，则返回一个空的副本。
        /// </summary>
        /// <returns> 父节点列表的副本 </returns>
        public List<string> GetParents()
        {
            List<string> parents = new List<string>();
            if(parents == null) return parents;
            parents.AddRange(this.parents);
            
            return parents;
        }
        
#if UNITY_EDITOR
        public Rect rect;

        /// <summary>
        /// 添加状态脚本到状态节点数据中。
        /// </summary>
        /// <param name="script"> 要添加的脚本 </param>
        /// <exception cref="Exception"> 脚本重复添加 </exception>
        public void AddStateScript(MonoScript script)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
            if(string.IsNullOrEmpty(guid))
                throw new Exception($"脚本添加失败，获取GUID失败:{script.name},请检查脚本路径是否正确。");

            foreach (FSMStateScriptInfo stateScript in StateScripts)
            {
                if (stateScript.guid.Equals(guid))
                    throw new Exception("脚本重复添加!");
            }

            FSMStateScriptInfo info = new FSMStateScriptInfo();
            info.guid = guid;
            Type scriptType = script.GetClass();
            if (scriptType != null)
            {
                info.className = scriptType.FullName;
            }
            
            StateScripts.Add(info);

        }

        /// <summary>
        /// 刷新状态脚本信息。如果脚本不存在，则移除该脚本。如果有新的脚本被添加到项目中，则需要手动调用此方法更新脚本信息。
        /// </summary>
        /// <param name="controller"></param>
        public void RefreshStateScripts(RuntimeFSMController controller)
        {
            List<FSMStateScriptInfo> invalid = new List<FSMStateScriptInfo>();

            foreach (FSMStateScriptInfo scriptInfo in StateScripts)
            {
                if(string.IsNullOrEmpty(scriptInfo.guid) && !string.IsNullOrEmpty(scriptInfo.className))
                    scriptInfo.guid = AssemblyTool.GetGUIDByStateClassFullName(scriptInfo.className);
                
                string path = AssetDatabase.GUIDToAssetPath(scriptInfo.guid);
                if (string.IsNullOrEmpty(path))
                {
                    invalid.Add(scriptInfo);
                    continue;
                }
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if(script == null)
                    continue;
                
                Type scriptType = script.GetClass();
                if(scriptType == null)
                    continue;
                
                scriptInfo.className = scriptType.FullName;
            }
            
            // 移除无效脚本
            foreach (FSMStateScriptInfo info in invalid)
            {
                StateScripts.Remove(info);
            }
            
            if(controller != null)
                controller.Save();
        }

        /// <summary>
        /// 移除状态脚本从状态节点数据中。
        /// </summary>
        /// <param name="script"> 要移除的脚本 </param>
        /// <exception cref="Exception"> 脚本不存在 </exception>
        public void RemoveStateScript(MonoScript script)
        {
            Type scriptType = script.GetClass();
            if (scriptType == null) return;
            
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));
            if (string.IsNullOrEmpty(guid))
                throw new Exception($"脚本移除失败，获取GUID失败:{script.name},请检查脚本路径是否正确。");

            FSMStateScriptInfo info = null;

            foreach (FSMStateScriptInfo scriptInfo in StateScripts)
            {
                if (scriptInfo.guid.Equals(guid))
                {
                    info = scriptInfo;
                    break;
                }
            }
            
            if (info == null) return;
            StateScripts.Remove(info);
        }

#endif
    }
}
