using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class ListenerFile
    {
        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            Selection.selectionChanged += OnSelectionChange;
            EditorApplication.quitting += OnEditorApplicationQuitting;
        }
        
        private static void OnSelectionChange()
        {
            RuntimeFSMController controller = Selection.activeObject as RuntimeFSMController;
            if (controller != null)
            {
                // 把InstanceID设置为空
                Context.Instance.FSMControllerInstanceID = 0;
                // 设置给Context 保存起来
                string assetPath = AssetDatabase.GetAssetPath(controller);
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                Context.Instance.RuntimeFSMControllerGUID = guid;
            }
            else
            {
                GameObject go = Selection.activeGameObject;

                if (go == null) return;

                if (!go.activeInHierarchy)
                {
                    string path = AssetDatabase.GetAssetPath(go);
                    
                    if(string.IsNullOrEmpty(path)) // 路径不为空说明是预制体
                        return;
                }
                
                // 判断有没有FSMController组件
                FSMController fsmController = go.GetComponent<FSMController>();
                if (FSMControllerIsEmpty(fsmController))
                    return;

                for (int i = 0; i < fsmController.RuntimeFSMControllersList.Count; i++)
                {
                    if (fsmController.RuntimeFSMControllersList[i] != null)
                    {
                        Context.Instance.FSMControllerIndex = i;
                        break;
                    }
                }

                Context.Instance.FSMControllerInstanceID = go.GetInstanceID();
            }
            
            Context.Instance.RefreshRuntimeFSMControllerGUID();
        }
        private static bool FSMControllerIsEmpty(FSMController fsmController)
        {
            if (fsmController == null) return true;
            if (fsmController.RuntimeFSMControllersList == null) return true;
            if (fsmController.RuntimeFSMControllersList.Count == 0) return true;

            for (int i = 0; i < fsmController.RuntimeFSMControllersList.Count; i++)
            {
                if (fsmController.RuntimeFSMControllersList[i] != null) return false;
            }
            
            return true;
        }

        private static void OnEditorApplicationQuitting()
        {
            Context.Instance.FSMControllerIndex = 0;
            Context.Instance.FSMControllerInstanceID = 0;
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            RuntimeFSMController obj = EditorUtility.InstanceIDToObject(insId) as RuntimeFSMController;

            if (obj != null)
                EditorWindow.GetWindow<FSMEditorWindow>().Show();

            return obj != null;
        }
    }
}
