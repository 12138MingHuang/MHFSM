using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MHFSM
{
    [Serializable]
    public class RuntimeFSMControllerLayer
    {
        /// <summary>
        /// 状态机层的名称
        /// </summary>
        public string name;

        /// <summary>
        /// 状态机层对应的控制器
        /// </summary>
        public RuntimeFSMController controller;
    }
    
    public class FSMController : MonoBehaviour
    {
        #region 字段
        
        [SerializeField]
        private List<RuntimeFSMController> _runtimeFSMControllers = new List<RuntimeFSMController>();
        [SerializeField]
        private List<RuntimeFSMControllerLayer> _runtimeFSMControllerLayers = new List<RuntimeFSMControllerLayer>();

        private List<RuntimeFSMControllerInstance> _runtimeFSMControllerInstances = new List<RuntimeFSMControllerInstance>();

        private List<RuntimeFSMController> _runtimeFSMControllerList = new List<RuntimeFSMController>();

        private List<RuntimeFSMControllerLayer> _runtimeFSMControllerLayerList = new List<RuntimeFSMControllerLayer>();

        internal object userData;

        [SerializeField]
        [Tooltip("是否在OnDisable时重置状态机")]
        [Header("是否在OnDisable时重置状态机")]
        private bool _resetOnDisable = true;

        #endregion

        #region 事件

        /// <summary>
        /// 初始化完成的回调
        /// </summary>
        public Action onInitFinish;

        /// <summary>
        /// 状态改变的回调 参数1:状态机名称 参数2:当前状态 
        /// </summary>
        public Action<string, string> onStateChange;

        #endregion

        #region 属性

        /// <summary>
        /// 当前状态机执行的状态配置文件列表
        /// </summary>
        public List<RuntimeFSMController> RuntimeFSMControllersList
        {
            get
            {
                _runtimeFSMControllerList.Clear();

                if (Application.isPlaying)
                {
                    foreach (RuntimeFSMControllerLayer controllerLayer in _runtimeFSMControllerLayerList)
                    {
                        if (controllerLayer.controller == null) continue;
                        
                        _runtimeFSMControllerList.Add(controllerLayer.controller);
                    }
                }
                else
                {
                    foreach (RuntimeFSMControllerLayer controllerLayer in _runtimeFSMControllerLayers)
                    {
                        if (controllerLayer.controller == null) continue;
                        
                        _runtimeFSMControllerList.Add(controllerLayer.controller);
                    }
                }
                
                return _runtimeFSMControllerList;
            }
        }

        public List<RuntimeFSMControllerLayer> RuntimeFSMControllerLayerList
        {
            get
            {
                if(Application.isPlaying) return _runtimeFSMControllerLayerList;

                return _runtimeFSMControllerLayers;
            }
        }

        /// <summary>
        /// 状态机是否初始化完成
        /// </summary>
        public bool Initialized { get; private set; } = false;

        #endregion

        #region 生命周期

        private void Start()
        {
            Init();
        }

        private void FixedUpdate()
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                instance.FixedUpdate();
            }
        }

        private void Update()
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                instance.Update();
            }
        }

        private void LateUpdate()
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                instance.LateUpdate();
            }
        }

        private void OnDisable()
        {
            if (_resetOnDisable)
            {
                foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                {
                    instance.Close();
                }
            }
        }

        private void OnDestroy()
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                instance.Close();
            }
        }
        #endregion

        #region 方法

        private void Init()
        {
            _runtimeFSMControllerInstances.Clear();

            if (_runtimeFSMControllerLayers.Count == 0 && _runtimeFSMControllers.Count > 0)
            {
                for (int i = 0; i < _runtimeFSMControllers.Count; i++)
                {
                    if (_runtimeFSMControllers[i] == null) continue;
                    RuntimeFSMControllerLayer layer = new RuntimeFSMControllerLayer();
                    layer.name = $"Layer {i}";
                    layer.controller = _runtimeFSMControllers[i];
                    _runtimeFSMControllerLayers.Add(layer);
                }
            }
            
            _runtimeFSMControllerLayerList.Clear();

            foreach (RuntimeFSMControllerLayer controllerLayer in _runtimeFSMControllerLayers)
            {
                if (controllerLayer.controller == null) continue;

                RuntimeFSMControllerInstance instance = new RuntimeFSMControllerInstance(controllerLayer.controller, this, controllerLayer.name);
                instance.StartUp();

                RuntimeFSMControllerLayer layer = new RuntimeFSMControllerLayer();
                layer.name = controllerLayer.name;
                layer.controller = instance.runtimeFSMController;
                _runtimeFSMControllerLayerList.Add(layer);
                _runtimeFSMControllerInstances.Add(instance);
            }
            
            Initialized = true;
            onInitFinish?.Invoke();
        }

        public void SetBool(string name, bool value)
        {
            SetBool(StringToHash(name), value);
        }
        /// <summary>
        /// 设置bool类型参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <param name="value"> 参数值</param>
        public void SetBool(int nameHash, bool value)
        {
            if (Initialized)
            {
                if (!IsContainParameter(nameHash, ParameterType.Bool))
                {
                    Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Bool);
                    return;
                }

                foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                {
                    instance.SetBool(nameHash, value);
                }
            }
            else
            {
                onInitFinish += () =>
                {
                    if (!IsContainParameter(nameHash, ParameterType.Bool))
                    {
                        Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Bool);
                        return;
                    }

                    foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                    {
                        instance.SetBool(nameHash, value);
                    }
                };
            }
        }
        
        public void SetFloat(string name, float value)
        {
            SetFloat(StringToHash(name), value);
        }
        /// <summary>
        /// 设置float类型参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <param name="value"> 参数值</param>
        public void SetFloat(int nameHash, float value)
        {
            if(Initialized)
            {
                if (!IsContainParameter(nameHash, ParameterType.Float))
                {
                    Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Float);
                    return;
                }

                foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                {
                    instance.SetFloat(nameHash, value);
                }
            }
            else
            {
                onInitFinish += () =>
                {
                    if (!IsContainParameter(nameHash, ParameterType.Float))
                    {
                        Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Float);
                        return;
                    }

                    foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                    {
                        instance.SetFloat(nameHash, value);
                    }
                };
            }
        }

        public void SetInt(string name, int value)
        {
            SetInt(StringToHash(name), value);
        }
        /// <summary>
        /// 设置int类型参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <param name="value"> 参数值</param>
        public void SetInt(int nameHash, int value)
        {
            if(Initialized)
            {
                if (!IsContainParameter(nameHash, ParameterType.Int))
                {
                    Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Int);
                    return;
                }

                foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                {
                    instance.SetInt(nameHash, value);
                }
            }
            else
            {
                onInitFinish += () =>
                {
                    if (!IsContainParameter(nameHash, ParameterType.Int))
                    {
                        Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Int);
                        return;
                    }

                    foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                    {
                        instance.SetInt(nameHash, value);
                    }
                };
            }
        }
        
        public void SetTrigger(string name)
        {
            SetTrigger(StringToHash(name));
        }
        /// <summary>
        /// 设置触发器参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        public void SetTrigger(int nameHash)
        {
            if(Initialized)
            {
                if (!IsContainParameter(nameHash, ParameterType.Trigger))
                {
                    Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Trigger);
                    return;
                }
                
                foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                {
                    instance.SetTrigger(nameHash);
                }
            }
            else
            {
                onInitFinish += () =>
                {
                    if (!IsContainParameter(nameHash, ParameterType.Trigger))
                    {

                        Debug.LogWarningFormat("未查询到参数:{0} 类型{1}", HashToString(nameHash), ParameterType.Trigger);
                        return;
                    }
                    
                    foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
                    {
                        instance.SetTrigger(nameHash);
                    }
                };
            }
        }
        
        public void ResetTrigger(string name)
        {
            ResetTrigger(StringToHash(name));
        }
        /// <summary>
        /// 重置触发器参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        public void ResetTrigger(int nameHash)
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                instance.SetTrigger(nameHash);
            }
        }

        public bool GetBool(string name)
        {
            return GetBool(StringToHash(name));
        }
        /// <summary>
        /// 获取bool类型参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <returns> 参数值</returns>
        public bool GetBool(int nameHash)
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                if(instance.GetParameter(nameHash) == 1)
                    return true;
            }
            
            return false;
        }

        public float GetFloat(string name)
        {
            return GetFloat(StringToHash(name));
        }
        /// <summary>
        /// 获取float类型参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <returns> 参数值</returns>
        public float GetFloat(int nameHash)
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                float value = instance.GetParameter(nameHash);
                if(value != 0)
                    return value;
            }
            
            return 0;
        }

        public int GetInt(string name)
        {
            return GetInt(StringToHash(name));
        }
        /// <summary>
        /// 获取int类型参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <returns> 参数值</returns>
        public int GetInt(int nameHash)
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                int value = (int)instance.GetParameter(nameHash);
                if(value != 0)
                    return value;
            }
            
            return 0;
        }

        public bool GetTrigger(string name)
        {
            return GetTrigger(StringToHash(name));
        }
        /// <summary>
        /// 获取触发器参数的值
        /// </summary>
        /// <param name="nameHash"> 参数名称的hash值</param>
        /// <returns> 参数值</returns>
        public bool GetTrigger(int nameHash)
        {

            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                if(instance.GetTriggerCount(nameHash) > 0 || instance.GetParameter(nameHash) == 1)
                    return true;
            }
            
            return false;
        }
        
        private bool IsContainParameter(int name, ParameterType type)
        {
            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                if (instance.parameters.ContainsKey(name) && instance.parameters[name].parameterType == type)
                    return true;
            }

            return false;
        }

        public void AddRuntimeFSMController(RuntimeFSMController controller)
        {
            string layerName = $"Layer:{controller.GetHashCode()}";
            AddRuntimeFSMController(controller, layerName);
        }
        /// <summary>
        /// 运行时添加并执行状态配置文件
        /// </summary>
        /// <param name="controller"> 状态配置文件</param>
        /// <param name="layerName"> 状态机所在的层级</param>
        public void AddRuntimeFSMController(RuntimeFSMController controller, string layerName)
        {
            if (controller == null) return;

            if (Contains(_runtimeFSMControllerLayers, controller))
            {
                Debug.LogWarningFormat("状态机:{0}已经执行,请勿重复添加!", controller.name);
                return; // 说明已经添加了该状态机
            }

            RuntimeFSMControllerLayer layer = new RuntimeFSMControllerLayer();
            layer.name = layerName;
            layer.controller = controller;
            _runtimeFSMControllerLayers.Add(layer);

            // 如果没有初始化完成 不需要处理 在初始化的时候会处理
            if (Initialized)
            {
                // 如果初始化完成了 启动这个状态
                RuntimeFSMControllerInstance controllerInstance = new RuntimeFSMControllerInstance(controller, this, layerName);
                controllerInstance.StartUp();

                RuntimeFSMControllerLayer layer2 = new RuntimeFSMControllerLayer();
                layer2.name = layerName;
                layer2.controller = controllerInstance.runtimeFSMController;
                _runtimeFSMControllerLayerList.Add(layer2);
                
                _runtimeFSMControllerInstances.Add(controllerInstance);
            }
        }

        public void RemoveRuntimeFSMController(string layerName)
        {
            if(string.IsNullOrEmpty(layerName))
                return;

            int index = -1;

            for (int i = 0; i < RuntimeFSMControllerLayerList.Count; i++)
            {
                if(layerName == RuntimeFSMControllerLayerList[i].name)
                {
                    index = i;
                    break;
                }
            }
            
            if(index != -1)
                RemoveRuntimeFSMController(index);
        }
        /// <summary>
        /// 移除运行时状态机配置文件
        /// </summary>
        /// <param name="index"> 配置文件下标</param>
        public void RemoveRuntimeFSMController(int index)
        {
            if (index < 0 || index >= RuntimeFSMControllerLayerList.Count) return;
            
            RuntimeFSMControllerLayer layer = RuntimeFSMControllerLayerList[index];
            RuntimeFSMControllerLayerList.RemoveAt(index);

            for (int i = 0; i < _runtimeFSMControllerInstances.Count; i++)
            {
                if (_runtimeFSMControllerInstances[i].runtimeFSMController == layer.controller)
                {
                    _runtimeFSMControllerInstances[i].Close();
                    _runtimeFSMControllerInstances.Remove(_runtimeFSMControllerInstances[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// 获取当前状态信息
        /// </summary>
        /// <param name="index"> 状态机下标</param>
        /// <param name="subStateName"> 子状态名称</param>
        /// <returns> 当前状态信息</returns>
        public FSMStateNode GetCurrentStateInfo(int index, string subStateName = "")
        {
            if (index < 0 || index >= _runtimeFSMControllerInstances.Count) return null;
            
            return _runtimeFSMControllerInstances[index].GetCurrentState(subStateName);
        }

        public FSMStateNode GetCurrentStateInfo(string layerName, string subStateName = "")
        {
            if (string.IsNullOrEmpty(layerName))
                return null;

            foreach (RuntimeFSMControllerInstance instance in _runtimeFSMControllerInstances)
            {
                if (instance.layerName.Equals(layerName))
                    return instance.GetCurrentState(subStateName);
            }
            
            return null;
        }
        /// <summary>
        /// 获取当前状态机正在执行的转换信息
        /// </summary>
        /// <param name="index"> 状态机下标</param>
        /// <returns> 当前状态机正在执行的转换信息</returns>
        public FSMTransition GetCurrentTransition(int index)
        {
            if (index < 0 || index >= _runtimeFSMControllerInstances.Count) return null;

            return _runtimeFSMControllerInstances[index].currentTransition;
        }

        private bool Contains(List<RuntimeFSMControllerLayer> layers, RuntimeFSMController controller)
        {
            if (controller == null) return false;

            foreach (RuntimeFSMControllerLayer layer in layers)
            {
                if (layer == null) continue;
                if (layer.controller.Equals(controller)) return true;
            }
            
            return false;
        }
        
        #endregion

        #region 静态字段

        private static Dictionary<string, FSMController> _fsmDict = new Dictionary<string, FSMController>(); 
        
        #endregion
        
        #region 静态属性
        
        private static Transform FSMParent {get; set; }
        
        #endregion
        
        #region 静态方法

        [RuntimeInitializeOnLoadMethod]
        static void InitFSMController()
        {
            _fsmDict.Clear();
            
            GameObject go = new GameObject("FSMControllers");
            DontDestroyOnLoad(go);
            FSMParent = go.transform;
        }

        /// <summary>
        /// 启动状态(适用于没有具体游戏物体的状态管理,例如:游戏状态)
        /// </summary>
        /// <param name="fsmName"> 状态名称</param>
        /// <param name="fsm"> 状态机</param>
        /// <param name="userData"> 用户数据</param>
        /// <returns> 状态机</returns>
        public static FSMController StartupFSM(string fsmName, RuntimeFSMController fsm, object userData = null)
        {
            if (_fsmDict.ContainsKey(fsmName)) return null;

            GameObject go = new GameObject(fsmName);
            go.transform.SetParent(FSMParent, false);
            FSMController controller = go.AddComponent<FSMController>();
            controller.userData = userData;
            controller.AddRuntimeFSMController(fsm);
            _fsmDict.Add(fsmName, controller);
            
            return controller;
        }
        
        /// <summary>
        /// 查询通过FSMController.StartupFSM启动的状态机
        /// </summary>
        /// <param name="fsmName"> 状态机名称</param>
        /// <returns> 状态机</returns>
        public static FSMController GetFSM(string fsmName)
        {
            return _fsmDict.GetValueOrDefault(fsmName);
        }

        /// <summary>
        /// 移除通过FSMController.StartupFSM启动的状态机
        /// </summary>
        /// <param name="fsmName">状态机名称</param>
        public static void RemoveFSM(string fsmName)
        {
            if(!_fsmDict.ContainsKey(fsmName)) return;

            FSMController controller = _fsmDict[fsmName];
            _fsmDict.Remove(fsmName);
            Destroy(controller.gameObject);
        }
        
        #endregion
        
        #region StringToHash

        private static Dictionary<string, int> stringHashs = new Dictionary<string, int>();
        private static Dictionary<int, string> hashStrings = new Dictionary<int, string>();
        
        public static int StringToHash(string eventName)
        {
            if(stringHashs.TryGetValue(eventName, out int toHash)) return toHash;
            
            int hash = Animator.StringToHash(eventName);
            stringHashs.Add(eventName, hash);
            
            hashStrings.TryAdd(hash, eventName);

            return stringHashs[eventName];
        }
        
        internal static string HashToString(int hash)
        {
            if (hashStrings.TryGetValue(hash, out string toString)) return toString;
            
            return string.Empty;
        }
        
        #endregion
    }
}
