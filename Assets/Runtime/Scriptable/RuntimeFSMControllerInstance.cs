using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    /// <summary>
    /// RuntimeFSMController 运行时状态机控制器实例。
    /// </summary>
    public class RuntimeFSMControllerInstance
    {
        public string name;
        public string layerName;
        public RuntimeFSMController runtimeFSMController;
        
        /// <summary>
        /// 所有的参数
        /// </summary>
        internal Dictionary<int, FSMParameterData> parameters = new Dictionary<int, FSMParameterData>();
        
        /// <summary>
        /// 所有参数的默认值
        /// </summary>
        [Tooltip("参数的默认值")]
        internal Dictionary<int, float> parameterDefault = new Dictionary<int, float>();

        /// <summary>
        /// 所有状态节点
        /// </summary>
        internal Dictionary<string, FSMStateNode> states = new Dictionary<string, FSMStateNode>();
        
        /// <summary>
        /// 所有状态转换
        /// </summary>
        internal List<FSMTransition> transitions = new List<FSMTransition>();

        /// <summary>
        /// 默认状态
        /// </summary>
        internal FSMStateNode defaultState { get; private set; } = null;

        /// <summary>
        /// 当前状态字典
        /// </summary>
        private Dictionary<string, FSMStateNode> _currentStates = new Dictionary<string, FSMStateNode>();
        
        /// <summary>
        /// 当前状态列表
        /// </summary>
        private List<FSMStateNode> currentStatesList = new List<FSMStateNode>();

        /// <summary>
        /// 当前的过渡
        /// </summary>
        public FSMTransition currentTransition { get; private set; } = null;
        
        /// <summary>
        /// 状态机控制器
        /// </summary>
        public FSMController FSMController { get; private set; }
        
        /// <summary>
        /// 状态切换计数(为了避免状态切换进入死循环)
        /// </summary>
        private Dictionary<string, int> stateCount = new Dictionary<string, int>();

        /// <summary>
        /// 当前的帧数
        /// </summary>
        private int currentSeconds = -1;

        /// <summary>
        /// 记录Trigger触发的次数
        /// </summary>
        private Dictionary<int, int> triggerCount = new Dictionary<int, int>();
        
        /// <summary>
        /// 记录Trigger触发的Key
        /// </summary>
        private List<int> triggerKeys = new List<int>();

        /// <summary>
        /// 状态机是否已经启动了
        /// </summary>
        public bool Started { get; private set; } = false;

        public RuntimeFSMControllerInstance(RuntimeFSMController runtimeFSMController, FSMController controller, string layerName)
        {
            FSMController = controller;
            if(runtimeFSMController == null) return;
            name = runtimeFSMController.name;
            // 默认不执行任何状态
            currentStates.Clear();

            this.runtimeFSMController = GameObject.Instantiate(runtimeFSMController);
#if UNITY_EDITOR
            this.runtimeFSMController.originGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this.runtimeFSMController));
#endif
            this.runtimeFSMController.name = runtimeFSMController.name;
            
            parameters.Clear();
            for (int i = 0; i < this.runtimeFSMController.parameters.Count; i++)
            {
                FSMParameterData data = this.runtimeFSMController.parameters[i];
                int nameHash = FSMController.StringToHash(data.name);

                if (parameters.ContainsKey(nameHash)) continue;
                
                data.nameHash = nameHash;
                data.onValueChange = null;
                parameters.Add(nameHash, data);
                parameterDefault.Add(nameHash, data.value);
            }
            
            states.Clear();
            for (int i = 0; i < this.runtimeFSMController.states.Count; i++)
            {
                FSMStateNodeData stateNodeData = this.runtimeFSMController.states[i];
                FSMStateNode stateNode = new FSMStateNode(stateNodeData, this);

                if (states.ContainsKey(stateNodeData.name)) continue;
                
                if(stateNodeData.defaultState && stateNodeData.BaseLayer())
                    defaultState = stateNode;
                
                states.Add(stateNodeData.name, stateNode);
            }
            
            transitions.Clear();
            for (int i = 0; i < this.runtimeFSMController.transitions.Count; i++)
            {
                FSMTransition transition = new FSMTransition(this, this.runtimeFSMController.transitions[i]);
                transitions.Add(transition);
            }
            
            Started = false;
            this.layerName = layerName;
        }
        
        /// <summary>
        /// 启动状态机控制器实例
        /// </summary>
        public void StartUp()
        {
            Started = true;
            // 找到默认状态，并切换到该状态
            SwitchState(defaultState, null);
        }

        public void Close()
        {
            // 切换到空状态 (当切换到空状态之后 就没有办法通过参数来切换状态)
            // 因为在判断条件是否满足时 会查询当前状态 如果当前状态为空 会直接return false;
            SwitchState(null, null);
            // 设置为非启动状态
            Started = false;
            // 重置参数
            ResetParameter();
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="state"> 状态节点</param>
        /// <param name="transition"> 过渡</param>
        internal void SwitchState(FSMStateNode state, FSMTransition transition)
        {
            int second = Mathf.FloorToInt(Time.time);

            if (second != currentSeconds)
            {
                currentSeconds = second;
                stateCount.Clear();
            }
            // 添加状态
            if (state != null)
                AddStateCount(state.data.name);

            string parent = state != null ? state.data.Parent : string.Empty;

            ExitState(parent, state);
            EnterState(state);
            
            currentTransition = transition;
            
            // 检测当前状态是否已经满足条件
            CheckConditionAndSwitch();
        }
        
        private void CheckConditionAndSwitch()
        {
            // 检测当前的状态是否有已经满足条件的过渡
            foreach (FSMTransition transition in transitions)
            {
                // 检测条件是否满足
                if (transition.CheckConditionAndSwitch())
                    break;
            }
        }
        
        private void EnterState(FSMStateNode state)
        {
            if(state == null) return;

            string parent = state.data.Parent;

            if (_currentStates.ContainsKey(parent))
                _currentStates[parent] = state;
            else
                _currentStates.Add(parent, state);
            
            state?.OnEnter();
            // 触发事件
            FSMController.onStateChange?.Invoke(this.runtimeFSMController.name, state.data.name);
            // 判断是否有子状态机
            if (state.data.isSubStateMachine)
            {
                // 进入到子状态机的默认状态
                FSMStateNodeData data = this.runtimeFSMController.GetDefaultStateNodeData(state.data.name);
                if (data != null && states.ContainsKey(data.name))
                    EnterState(states[data.name]);
            }
        }
        
        private void ExitState(string parent, FSMStateNode nextState)
        {
            if (!_currentStates.TryGetValue(parent, out FSMStateNode state)) return;
            if (state == null) return;

            FSMStateNode currentState = _currentStates[parent];

            if (nextState != null)
                currentState.nextState = nextState.data.name;

            if (nextState != null && nextState.data.CompareParent(currentState.data))
                nextState.lastState = currentState.data.name;

            if (nextState != null)
                nextState.nextState = null;

            currentState.OnExit();
            
            // 移除状态
            _currentStates.Remove(parent);
            
            // 判断一下当前状态是否子状态机
            if (currentState.data.isSubStateMachine)
                ExitState(currentState.data.name, nextState);
        }

        /// <summary>
        /// bool参数
        /// </summary>
        /// <param name="nameHash"> 参数名称</param>
        /// <param name="value"> 值</param>
        public void SetBool(int nameHash, bool value)
        {
            SetParameter(nameHash, value ? 1 : 0, ParameterType.Bool);
        }

        /// <summary>
        /// 设置浮点参数
        /// </summary>
        /// <param name="nameHash"> 参数名称</param>
        /// <param name="value"> 值</param>
        public void SetFloat(int nameHash, float value)
        {
            SetParameter(nameHash, value, ParameterType.Float);
        }
        
        /// <summary>
        /// 设置整形参数
        /// </summary>
        /// <param name="nameHash"> 参数名称</param>
        /// <param name="value"> 值</param>
        public void SetInt(int nameHash, int value)
        {
            SetParameter(nameHash, value, ParameterType.Int);
        }

        /// <summary>
        /// 设置触发器参数,触发器参数只在当前帧有效，如果要持续有效 需要多次调用SetTrigger方法
        /// </summary>
        /// <param name="name"> 参数名称</param>
        public void SetTrigger(int name)
        {
            FSMParameterData parameterData;
            if (parameters.TryGetValue(name, out parameterData))
            {
                if (parameterData.parameterType != ParameterType.Trigger) return;

                if (Mathf.Approximately(parameterData.value, 1))
                {
                    if (!triggerCount.TryAdd(name, 1))
                        triggerCount[name]++;
                }
                else
                {
                    SetParameter(name, 1, ParameterType.Trigger);
                }
            }
        }

        /// <summary>
        /// 当Trigger触发后还原trigger
        /// </summary>
        /// <param name="name"> 参数名称</param>
        internal void ClearTrigger(int name)
        {
            SetParameter(name, 1, ParameterType.Trigger);
        }

        public void ResetTrigger(int name)
        {
            if (triggerCount.ContainsKey(name))
                triggerCount[name] = 0;
            
            ClearTrigger(name);
        }
        
        private void SetParameter(int name, float value, ParameterType type)
        {
            FSMParameterData parameterData;
            if(parameters.TryGetValue(name, out parameterData))
            {
                if (parameterData.parameterType == type)
                    parameterData.value = value;
            }
        }

        /// <summary>
        /// 获取参数值,如果参数不存在则返回0f
        /// </summary>
        /// <param name="name"> 参数名称</param>
        /// <returns> 参数值</returns>
        internal float GetParameter(int name)
        {
            FSMParameterData parameterData;
            if(parameters.TryGetValue(name, out parameterData))
                return parameterData.value;
            
            return 0;
        }

        /// <summary>
        /// 获取触发器次数
        /// </summary>
        /// <param name="name"> 参数名称</param>
        /// <returns> 触发器次数</returns>
        internal int GetTriggerCount(string name)
        {
            return GetTriggerCount(FSMController.StringToHash(name));
        }

        /// <summary>
        /// 获取触发器次数
        /// </summary>
        /// <param name="name"> 参数名称</param>
        /// <returns> 触发器次数</returns>
        internal int GetTriggerCount(int name)
        {
            return triggerCount.GetValueOrDefault(name, 0);
        }

        public void Update()
        {
            currentStatesList.Clear();

            foreach (string statesKey in _currentStates.Keys)
            {
                if (_currentStates[statesKey] == null) continue;

                currentStatesList.Add(_currentStates[statesKey]);
            }

            foreach (FSMStateNode stateNode in currentStatesList)
            {
                if (!stateNode.isRunning) continue;
                
                stateNode.OnUpdate();
            }
            
            // 更新Trigger
            UpdateTrigger();
        }

        private void AddStateCount(string dataName)
        {
            
        }

        private void ResetParameter()
        {
            
        }
    }
}
