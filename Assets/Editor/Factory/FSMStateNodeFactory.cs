using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class FSMStateNodeFactory
    {
        /// <summary>
        /// 创建状态节点
        /// </summary>
        /// <param name="controller"> fsm控制器</param>
        /// <param name="name"> 状态节点名称</param>
        /// <param name="rect"> 状态节点rect</param>
        /// <param name="defaultState"> 是否默认状态</param>
        /// <param name="isSubStateMachine"> 是否子状态机</param>
        /// <param name="parents"> 父状态节点名称</param>
        /// <param name="isBuildInState"> 是否内置状态</param>
        /// <param name="buildInStateName"> 内置状态名称</param>
        /// <returns></returns>
        public static FSMStateNodeData CreateStateNode(RuntimeFSMController controller, string name, Rect rect, bool defaultState, bool isSubStateMachine = false,
            List<string> parents = null, bool isBuildInState = false, string buildInStateName = "")
        {
            // 判断名称是否重复
            if (controller.GetStateNodeData(name) != null)
            {
                string message = $"创建状态节点失败,名称:{name}重复";
                Debug.LogError(message);
                FSMEditorWindow.ShowNotification(message);
                return null;
            }
            
            FSMStateNodeData nodeData = new FSMStateNodeData();
            nodeData.name = name;
            nodeData.rect = rect;
            nodeData.isSubStateMachine = isSubStateMachine;
            nodeData.parents = parents;
            nodeData.isBuildInState = isBuildInState;
            nodeData.buildInStateName = buildInStateName;

            if (defaultState)
            {
                foreach (FSMStateNodeData state in controller.states)
                {
                    // 把相同层级的默认状态设置成false
                    if(!state.CompareParent(nodeData)) continue;

                    state.defaultState = false;
                }
            }
            
            nodeData.defaultState = defaultState;
            controller.AddState(nodeData);
            AssetDatabase.SaveAssets();
            
            return nodeData;
        }
        
        public static FSMStateNodeData CreateStateNode(RuntimeFSMController controller, Rect rect, bool defaultState, 
            bool isSubStateMachine = false, List<string> parent = null,bool isBuildInState = false, string buildInStateName = "")
        {
            return CreateStateNode(controller, GetStateNodeName(controller), rect, defaultState, isSubStateMachine, parent, isBuildInState, buildInStateName);
        }

        private static string GetStateNodeName(RuntimeFSMController controller)
        {
            string name = null;
            int i = 1;
            do
            {
                name = $"New State{i}";
                i++;
            } while (controller.GetStateNodeData(name) != null);

            return name;
        }

        /// <summary>
        /// 删除状态节点数据和过渡数据
        /// </summary>
        /// <param name="controller"> fsm控制器</param>
        /// <param name="stateNodeData"> 状态节点数据</param>
        public static void DeleteState(RuntimeFSMController controller, FSMStateNodeData stateNodeData)
        {
            if (stateNodeData.IsAnyState || stateNodeData.IsEntryState || stateNodeData.IsUpstate)
            {
                if (stateNodeData.BaseLayer())
                {
                    string message = $"状态:{stateNodeData.DisplayName}不能删除!";
                    Debug.LogError(message);
                    FSMEditorWindow.ShowNotification(message);
                    return;
                }
            }
            
            // 删除相关的过渡
            for (int i = controller.transitions.Count - 1; i >= 0; i--)
            {
                FSMTransitionData transitionData = controller.transitions[i];
                if (transitionData.fromStateName.Equals(stateNodeData.name) || transitionData.toStateName.Equals(stateNodeData.name))
                    controller.RemoveTransition(transitionData);
            }
            
            controller.RemoveState(stateNodeData);
            
            // 判断是不是默认状态
            if (stateNodeData.defaultState)
            {
                foreach (FSMStateNodeData state in controller.states)
                {
                    if(state.IsAnyState || state.IsEntryState || state.IsUpstate) continue;

                    if (!state.CompareParent(stateNodeData)) continue;
                    
                    state.defaultState = false;
                    break;
                }
            }
            
            // 判断是不是子状态机
            if (stateNodeData.isSubStateMachine)
            {
                List<FSMStateNodeData> subStates = new List<FSMStateNodeData>();
                foreach (FSMStateNodeData state in controller.states)
                {
                    if(state.BelongToParent(stateNodeData.name))
                        subStates.Add(state);
                }

                foreach (FSMStateNodeData subState in subStates)
                {
                    // 删除子状态机状态节点
                    DeleteState(controller, subState);
                }
            }
        }

        /// <summary>
        /// 重命名状态节点
        /// </summary>
        /// <param name="controller"> fsm控制器</param>
        /// <param name="node"> 状态节点数据</param>
        /// <param name="newName"> 新名称</param>
        /// <returns> 是否重命名成功</returns>
        public static bool Rename(RuntimeFSMController controller, FSMStateNodeData node, string newName)
        {
            if (node.name.Equals(FSMConst.EntryState) || node.name.Equals(FSMConst.AnyState)) return false;

            if (string.IsNullOrEmpty(newName))
            {
                FSMEditorWindow.ShowNotification("名称不能为空!");
                return false;
            }

            if (controller.GetStateNodeData(newName) != null)
            {
                string message = $"状态重命名失败,名称:{newName}已经存在,请使用其他的名称!";
                FSMEditorWindow.ShowNotification(message);
                return false;
            }

            foreach (FSMTransitionData transition in controller.transitions)
            {
                if(transition.fromStateName.Equals(node.name))
                    transition.fromStateName = newName;
                
                if (transition.toStateName.Equals(node.name))
                    transition.toStateName = newName;
            }
            
            string oldName = node.name;
            node.name = newName;
            
            // 修改子状态机的层级
            if (node.isSubStateMachine)
            {
                // 查询所有包含该状态的状态节点
                List<FSMStateNodeData> subStates = new List<FSMStateNodeData>();
                foreach (FSMStateNodeData state in controller.states)
                {
                    if (state.ContainsParent(oldName)) subStates.Add(state);
                }

                foreach (FSMStateNodeData subState in subStates)
                {
                    subState.RenameParent(oldName, newName);

                    if (subState.isBuildInState)
                    {
                        string subStateNewName = $"{subState.ParentPath}/{subState.buildInStateName}";
                        Rename(controller, subState, subStateNewName);
                    }
                }
            }
            
            controller.ClearCache();
            EditorUtility.SetDirty(controller);

            return true;
        }
    }
}
