using PlasticGui.Help.Conditions;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class Context
    {
        private static Context _instance;
        public static Context Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Context();

                return _instance;
            }
        }
        private Context() { }

        #region 字段

        private RuntimeFSMController _runtimeFSMController;
        public List<FSMStateNodeData> selectNodesList = new List<FSMStateNodeData>();
        public FSMTransitionData selectTransitionData;
        public bool isPreviewTransition = false;
        public FSMStateNodeData fromState = null;
        public FSMStateNodeData hoverState = null;

        private FSMController _fsmController;
        
        #endregion

        #region 属性

        // 这个逻辑修改为如果只有一个,逻辑不变,
        // 如果有多个,判断当前显示的是不是在多个中,如果在不做处理,如果不在设置为第一个
        public RuntimeFSMController RuntimeFSMController
        {
            get
            {
                if (!IsEmpty(RuntimeFSMControllers))
                {
                    if(FSMControllerIndex < 0 || FSMControllerIndex >= RuntimeFSMControllers.Count)
                        FSMControllerIndex = 0;
                    
                    RuntimeFSMController controller = RuntimeFSMControllers[FSMControllerIndex];

                    if (controller != null && !string.IsNullOrEmpty(controller.originGUID) && _runtimeFSMControllerGUID != controller.originGUID)
                    {
                        PlayerPrefs.SetString("MHFSMRuntimeFSMControllerGUID", controller.originGUID);
                        _runtimeFSMControllerGUID = controller.originGUID;
                    }
                    
                    return controller;
                }

                if (_runtimeFSMController != null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(RuntimeFSMControllerGUID);
                    _runtimeFSMController = AssetDatabase.LoadAssetAtPath<RuntimeFSMController>(path);
                }
                
                return _runtimeFSMController;
            }

            internal set
            {
                if (_runtimeFSMController == value) return;
                
                _runtimeFSMController = value;
            }
        }
        
        public FSMController FSMController
        {
            get
            {
                if (_fsmController == null)
                {
                    GameObject go = EditorUtility.InstanceIDToObject(FSMControllerInstanceID) as GameObject;
                    if (go == null) return null;
                    _fsmController = go.GetComponent<FSMController>();
                }
                
                return _fsmController;
            }
        }
        
        private List<RuntimeFSMController> currentFSMControllers = new List<RuntimeFSMController>();
        public List<RuntimeFSMController> RuntimeFSMControllers
        {
            get
            {
                if (FSMController != null)
                    return FSMController.RuntimeFSMControllersList;
                
                currentFSMControllers.Clear();

                if (_runtimeFSMController == null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(RuntimeFSMControllerGUID);
                    _runtimeFSMController = AssetDatabase.LoadAssetAtPath<RuntimeFSMController>(path);
                }

                if (_runtimeFSMController != null)
                    currentFSMControllers.Add(_runtimeFSMController);
                
                return currentFSMControllers;
            }
        }
        

        internal int FSMControllerInstanceID
        {
            get
            {
                return PlayerPrefs.GetInt("MHFSMControllerInstanceID", 0);
            }
            set
            {
                PlayerPrefs.SetInt("MHFSMControllerInstanceID", value);
                UnityEngine.Object obj = EditorUtility.InstanceIDToObject(value);
                GameObject go = obj as GameObject;
                if (go != null && go.GetComponent<FSMController>() != null)
                    _fsmController = go.GetComponent<FSMController>();
                else _fsmController = null;
            }
        }

        private string _runtimeFSMControllerGUID;
        internal string RuntimeFSMControllerGUID
        {
            get
            {
                return PlayerPrefs.GetString("MHFSMRuntimeFSMControllerGUID", string.Empty);
            }
            set
            {
                PlayerPrefs.SetString("MHFSMRuntimeFSMControllerGUID", value);
                string path = AssetDatabase.GUIDToAssetPath(value);
                RuntimeFSMController = AssetDatabase.LoadAssetAtPath<RuntimeFSMController>(path);
            }
        }

        internal int FSMControllerIndex
        {
            get
            {
                return PlayerPrefs.GetInt("FSMControllerIndex", 0);
            }
            set
            {
                if(FSMControllerIndex == value) return;

                PlayerPrefs.SetInt("FSMControllerIndex", value);
                
                // 刷新RuntimeGUID
                RefreshRuntimeFSMControllerGUID();
            }
        }
        
        #endregion

        #region Layers
        
        private List<string> _emptyLayers = new List<string>();
        public List<string> Layers
        {
            get
            {
                if (RuntimeFSMController != null)
                    return RuntimeFSMController.Layers;
                
                return _emptyLayers;
            }
        }

        public string LayerParent
        {
            get
            {
                if(Layers.Count == 0)
                    return string.Empty;
                
                return Layers[^1];
            }
        }

        public void AddLayer(string layer)
        {
            if(RuntimeFSMController == null) return;
            RuntimeFSMController.AddLayer(layer);
        }
        
        public void RemoveLayer(int index, int count)
        {
            if(RuntimeFSMController == null) return;
            RuntimeFSMController.RemoveLayer(index, count);
        }

        public void RemoveLast(string name)
        {
            if(RuntimeFSMController == null) return;
            int index = RuntimeFSMController.Layers.Count - 1;
            if (index >= 0 && index < RuntimeFSMController.Layers.Count)
            {
                string last = RuntimeFSMController.Layers[index];
                if(!last.Equals(name)) return;

                RuntimeFSMController.RemoveLayer(RuntimeFSMController.Layers.Count - 1, 1);
            }
        }
        
        #endregion

        public void ClearSelections()
        {
            // 如果不是 InspectorWindow 此时就清空
            selectNodesList.Clear();
            selectTransitionData = null;
            Selection.activeObject = null;
        }

        /// <summary>
        /// 获取当前显示的 StateNodeData 列表
        /// </summary>
        /// <returns> StateNodeData 列表 </returns>
        public List<FSMStateNodeData> GetCurrentShowStateNodeData()
        {
            if (RuntimeFSMController != null)
                return RuntimeFSMController.GetCurrentShowStateNodeData(LayerParent);
            
            return null;
        }

        /// <summary>
        /// 获取当前显示的 TransitionData 列表
        /// </summary>
        /// <returns> TransitionData 列表 </returns>
        public List<FSMTransitionData> GetCurrentShowTransitionData()
        {
            if (RuntimeFSMController != null)
                return RuntimeFSMController.GetCurrentShowTransitionData(LayerParent);
            
            return null;
        }

        public bool IsEmpty(List<RuntimeFSMController> controllers)
        {
            if(controllers == null || controllers.Count == 0) return true;

            foreach (RuntimeFSMController controller in controllers)
            {
                if (controller != null) return false;
            }
            
            return true;
        }
        
        // 运行时选中某一个FSMController 组件，当取消运行，这个游戏物体被销毁时
        // 仍选中这个状态配置
        
        private List<RuntimeFSMControllerLayer> _tempLayers = new List<RuntimeFSMControllerLayer>();

        public void RefreshRuntimeFSMControllerGUID()
        {
            if(!Application.isPlaying) return;
            if(FSMController == null) return;
            if(IsEmpty(FSMController.RuntimeFSMControllersList)) return;
            
            Type type = typeof(FSMController);
            FieldInfo fieldInfo = type.GetField("_runtimeFSMControllerLayers", BindingFlags.NonPublic | BindingFlags.Instance);
            List<RuntimeFSMControllerLayer> layers = fieldInfo.GetValue(FSMController) as List<RuntimeFSMControllerLayer>;

            if (layers == null || layers.Count == 0) return;
            
            _tempLayers.Clear();

            foreach (RuntimeFSMControllerLayer layer in layers)
            {
                if (layer.controller == null) continue;
                _tempLayers.Add(layer);
            }

            if (FSMControllerIndex < 0 || FSMControllerIndex >= _tempLayers.Count) return;

            string assetPath = AssetDatabase.GetAssetPath(_tempLayers[FSMControllerIndex].controller);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid)) return;
            
            RuntimeFSMControllerGUID = guid;
        }
    }
}
