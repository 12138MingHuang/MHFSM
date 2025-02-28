using System;
using System.Collections.Generic;
using UnityEngine;

namespace MHFSM
{
    /// <summary>
    /// 状态节点
    /// </summary>
    public class FSMStateNode
    {
        /// <summary>
        /// 状态数据
        /// </summary>
        public FSMStateNodeData data;
        internal RuntimeFSMControllerInstance instance;
        private List<FSMState> _states = new List<FSMState>();
        internal string lastState;
        internal string nextState;
        /// <summary>
        /// 是否正在运行
        /// </summary>
        internal bool isRunning = false;
        /// <summary>
        /// 是否是第一次进入状态
        /// </summary>
        internal bool isFirstEnter = true;

        [Tooltip("当前状态名称Hash,可通过方法FSMController.StringToHash()转换!")]
        public int nameHash;

        internal FSMStateNode(FSMStateNodeData data, RuntimeFSMControllerInstance instance)
        {
            this.data = data;
            this.instance = instance;
            nameHash = FSMController.StringToHash(data.name);
            
            _states.Clear();

            foreach (FSMStateScriptInfo scriptInfo in this.data.StateScripts)
            {
                Type type = AssemblyTool.GetType(scriptInfo.className);
                FSMState state = null;
                if (type != null)
                    state = Activator.CreateInstance(type) as FSMState;

                if (state != null)
                {
                    state.controller = instance.FSMController;
                    state.userData = instance.FSMController.userData;
                    state.currentStateInfo = this;
                    _states.Add(state);
                }
            }
        }

        internal void OnEnter()
        {
            isRunning = true;

            foreach (FSMState state in _states)
            {
                state.lastState = lastState;
                state.nextState = nextState;
                
                if(isFirstEnter)
                    state.OnCreate();
                
                state.OnEnter();
            }
            
            isFirstEnter = false;
        }
        
        internal void OnUpdate() 
        {
            foreach (FSMState state in _states)
            {
                state.OnUpdate();
            } 
        }

        internal void OnFixedUpdate() 
        {
            foreach (FSMState state in _states)
            {
                state.OnFixedUpdate();
            } 
        }

        internal void OnLateUpdate() 
        {
            foreach (FSMState state in _states)
            {
                state.OnLateUpdate();
            } 
        }

        internal void OnExit()
        {
            foreach (FSMState state in _states)
            {
                state.lastState = lastState;
                state.nextState = nextState;
                state.OnExit();
            }
            isRunning = false;
        }
    }
}
