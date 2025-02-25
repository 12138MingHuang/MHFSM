using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class RuntimeFSMController : ScriptableObject
    {
        #if UNITY_EDITOR

        /// <summary>
        /// 背景线的中心点坐标
        /// </summary>
        [HideInInspector]
        public Vector3 viewPosition = new Vector3(100, 100, 0);
        
        /// <summary>
        /// 背景线缩放比例
        /// </summary>
        [HideInInspector]
        public Vector3 viewScale = Vector3.one * 0.8f;
        
        /// <summary>
        /// 当前旋转的状态机层级
        /// </summary>
        [HideInInspector]
        public List<string> Layers = new List<string>();

        /// <summary>
        /// 实例化该对象的源文件的GUID
        /// </summary>
        [HideInInspector]
        public string originGUID;

        /// <summary>
        /// 添加层级到层级列表中
        /// </summary>
        /// <param name="layer"> 层级名称 </param>
        public void AddLayer(string layer)
        {
            if(Layers.Contains(layer)) return;
            
            Layers.Add(layer);
            Save();
        }

        /// <summary>
        /// 从层级列表中移除层级
        /// </summary>
        /// <param name="index"> 移除的起始索引 </param>
        /// <param name="count"> 移除的数量 </param>
        public void RemoveLayer(int index, int count)
        {
            Layers.RemoveRange(index, count);
            Save();
        }
        
        /// <summary>
        /// 清空层级列表中所有层级
        /// </summary>
        public void ClearLayer()
        {
            Layers.Clear();
            Save();
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 刷新状态脚本
        /// </summary>
        public void RefreshStateScripts()
        {
            foreach (FSMStateNodeData stateNodeData in states)
            {
                stateNodeData.RefreshStateScripts(this);
            }
        }

        /// <summary>
        /// 刷新所有状态脚本
        /// </summary>
        [InitializeOnLoadMethod]
        private static void RefreshAllStateScripts()
        {
            string[] controllers = AssetDatabase.FindAssets("t:RuntimeFSMController");

            foreach (string controller in controllers)
            {
                string path = AssetDatabase.GUIDToAssetPath(controller);
                RuntimeFSMController controllerObject = AssetDatabase.LoadAssetAtPath<RuntimeFSMController>(path);
                controllerObject.RefreshStateScripts();
            }
        }

        #endif

        /// <summary>
        /// 所有的状态
        /// </summary>
        [HideInInspector]
        public List<FSMStateNodeData> states = new List<FSMStateNodeData>();
        
        /// <summary>
        /// 所有的参数
        /// </summary>
        [HideInInspector]
        public List<FSMParameterData> parameters = new List<FSMParameterData>();
        
        /// <summary>
        /// 所有的过渡
        /// </summary>
        [HideInInspector]
        public List<FSMTransitionData> transitions = new List<FSMTransitionData>();
        
        private Dictionary<string, FSMStateNodeData> statesDict = new Dictionary<string, FSMStateNodeData>();
        private Dictionary<string, FSMParameterData> parametersDict = new Dictionary<string, FSMParameterData>();
        private Dictionary<string, FSMTransitionData> transitionsDict = new Dictionary<string, FSMTransitionData>();
        private Dictionary<string, FSMStateNodeData> defaultStatesDict = new Dictionary<string, FSMStateNodeData>();

        /// <summary>
        /// 获取状态数据，如果不存在则返回null
        /// </summary>
        /// <param name="name"> 状态名称 </param>
        /// <returns> 状态数据 </returns>
        /// <exception cref="Exception"> 状态名称重复 </exception>
        public FSMStateNodeData GetStateNodeData(string name)
        {
            if(name == null) return null;

            if (statesDict.Count == 0)
            {
                foreach (FSMStateNodeData stateNodeData in states)
                {
                    if(!statesDict.TryAdd(stateNodeData.name, stateNodeData))
                        throw new Exception($"状态名称重复: {stateNodeData.name}");
                }
            }
            
            return statesDict.GetValueOrDefault(name);
        }

        /// <summary>
        /// 获取参数数据，如果不存在则返回null
        /// </summary>
        /// <param name="name"> 参数名称 </param>
        /// <returns> 参数数据 </returns>
        /// <exception cref="Exception"> 参数名称重复 </exception>
        public FSMParameterData GetParameterData(string name)
        {
            if(name == null) return null;
            
            if (parametersDict.Count == 0)
            {
                foreach (FSMParameterData parameter in parameters)
                {
                    if(!parametersDict.TryAdd(parameter.name, parameter))
                        throw new Exception($"参数名称重复: {parameter.name}");
                }
            }
            
            return parametersDict.GetValueOrDefault(name);
        }

        /// <summary>
        /// 获取过渡数据，如果不存在则返回null
        /// </summary>
        /// <param name="from"> 过渡起始状态名称 </param>
        /// <param name="to"> 过渡目标状态名称 </param>
        /// <returns> 过渡数据 </returns>
        /// <exception cref="Exception"> 过渡名称重复 </exception>
        public FSMTransitionData GetTransitionData(string from, string to)
        {
            if (transitionsDict.Count == 0)
            {
                foreach (FSMTransitionData transition in transitions)
                {
                    if(!transitionsDict.TryAdd(transition.Key, transition))
                        throw new Exception($"过渡名称重复: {transition.fromStateName} -> {transition.toStateName}");
                }
            }

            string key = $"{from}:{to}";
            
            return transitionsDict.GetValueOrDefault(key);
        }

        /// <summary>
        /// 获取默认状态数据，如果不存在则返回null
        /// </summary>
        /// <param name="parent"> 父状态名称</param>
        /// <returns> 默认状态数据 </returns>
        /// <exception cref="Exception"> 默认状态名称重复 </exception>
        public FSMStateNodeData GetDefaultStateNodeData(string parent)
        {
            if(parent == null) return null;

            if (defaultStatesDict.Count == 0)
            {
                foreach (FSMStateNodeData stateNodeData in states)
                {
                    if(!stateNodeData.defaultState) continue;
                    
                    if(!defaultStatesDict.TryAdd(stateNodeData.Parent, stateNodeData))
                        throw new Exception($"检测到层级:{stateNodeData.Parent}有多个默认状态!");
                }
            }
            
            return defaultStatesDict.GetValueOrDefault(parent);
        }

        #if UNITY_EDITOR

        private Dictionary<string, List<FSMStateNodeData>> currentShowStates = new Dictionary<string, List<FSMStateNodeData>>();
        private Dictionary<string, List<FSMTransitionData>> currentShowTransitions = new Dictionary<string, List<FSMTransitionData>>();

        /// <summary>
        /// 添加状态数据到列表中
        /// </summary>
        /// <param name="state"> 状态数据 </param>
        public void AddState(FSMStateNodeData state)
        {
            ClearCache();
            states.Add(state);
            Save();
        }
        /// <summary>
        /// 移除状态数据到列表中
        /// </summary>
        /// <param name="state"> 状态数据 </param>
        public void RemoveState(FSMStateNodeData state)
        {
            ClearCache();
            states.Remove(state);
            Save();
        }
        /// <summary>
        /// 添加参数数据到列表中
        /// </summary>
        /// <param name="data"> 参数数据 </param>
        public void AddParameters(FSMParameterData data) {
            ClearCache();
            parameters.Add(data);
            Save();
        }
        /// <summary>
        /// 移除参数数据到列表中
        /// </summary>
        /// <param name="data"> 参数数据 </param>
        public void RemoveParameters(FSMParameterData data) {
            ClearCache();
            parameters.Remove(data);
            Save();
        }
        /// <summary>
        /// 添加过渡数据到列表中
        /// </summary>
        /// <param name="data"> 过渡数据 </param>
        public void AddTransition(FSMTransitionData data) {
            ClearCache();
            transitions.Add(data);
            Save();
        }
        /// <summary>
        /// 移除过渡数据到列表中
        /// </summary>
        /// <param name="data"> 过渡数据 </param>
        public void RemoveTransition(FSMTransitionData data) {
            ClearCache();
            transitions.Remove(data);
            Save();
        }
        
        /// <summary>
        /// 清除缓存数据
        /// </summary>
        public void ClearCache()
        {
            statesDict.Clear();
            parametersDict.Clear();
            defaultStatesDict.Clear();
            currentShowStates.Clear();
            currentShowTransitions.Clear();
            transitionsDict.Clear();
        }

        /// <summary>
        /// 获取当前状态数据，如果不存在则返回null
        /// </summary>
        /// <param name="parent"> 父状态名称</param>
        /// <returns> 层级数据 </returns>
        public List<FSMStateNodeData> GetCurrentShowStateNodeData(string parent)
        {
            if(parent == null) return null;
            
            if (currentShowStates.Count == 0)
            {
                foreach (FSMStateNodeData stateNodeData in states)
                {
                    if (currentShowStates.ContainsKey(stateNodeData.Parent))
                    {
                        currentShowStates[stateNodeData.Parent].Add(stateNodeData);
                    }
                    else
                    {
                        currentShowStates.Add(stateNodeData.Parent, new List<FSMStateNodeData> { stateNodeData });
                    }
                }
            }

            if (currentShowStates.TryGetValue(parent, out List<FSMStateNodeData> currentShowState))
            {
                if (currentShowStates.Count == 0)
                    ClearLayer();
                
                return currentShowState;
            }
            
            ClearLayer();
            
            return null;
        }

        /// <summary>
        /// 获取当前过渡数据，如果不存在则返回null
        /// </summary>
        /// <param name="parent"> 父状态名称</param>
        /// <returns> 层级数据 </returns>
        /// <exception cref="Exception"> 过渡的起始状态和结束状态不在同一个层级 </exception>
        public List<FSMTransitionData> GetCurrentShowTransitionData(string parent)
        {
            if (parent == null) return null;

            if (currentShowTransitions.Count == 0)
            {
                foreach (FSMTransitionData transitionData in transitions)
                {
                    FSMStateNodeData from = GetStateNodeData(transitionData.fromStateName);
                    FSMStateNodeData to = GetStateNodeData(transitionData.toStateName);

                    if (!from.Parent.Equals(to.Parent))
                        throw new Exception($"过渡的起始状态{from.name}和结束状态{to.name}不在同一个层级!");

                    if (currentShowTransitions.ContainsKey(from.Parent))
                    {
                        currentShowTransitions[from.Parent].Add(transitionData);
                    }
                    else
                    {
                        currentShowTransitions.Add(from.Parent, new List<FSMTransitionData> { transitionData });
                    }
                }
            }
            
            return currentShowTransitions.GetValueOrDefault(parent);

        }

        #endif
    }
}
