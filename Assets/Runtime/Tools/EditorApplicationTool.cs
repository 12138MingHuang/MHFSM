using System;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    /// <summary>
    /// EditorApplicationTool工具类，用于编辑器相关功能封装。
    /// </summary>
    internal class EditorApplicationTool
    {
        /// <summary>
        /// 默认的编辑器播放状态。
        /// </summary>
        private static bool _isPlaying = true;

        /// <summary>
        /// 判断当前编辑器是否处于运行状态(UnityEngine.Application.isPlaying在停止运行编辑器时触发OnDestroy时仍返回true)
        /// </summary>
        public static bool isPlaying
        {
            get { return _isPlaying; }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
        }
        
#if UNITY_EDITOR
        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    _isPlaying = false;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    _isPlaying = true;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    _isPlaying = true;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    _isPlaying = false;
                    break;
            }
        }
#endif
    }
}
