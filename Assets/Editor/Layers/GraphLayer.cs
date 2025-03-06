using UnityEditor;
using UnityEngine;

namespace MHFSM
{
    public class GraphLayer
    {
        protected Rect position;
        
        public EditorWindow EditorWindow { get; private set; }
        
        public GraphLayer(EditorWindow editorWindow)
        {
            EditorWindow = editorWindow;
        }

        public virtual void OnGUI(Rect rect)
        {
            position = rect;
            UpdateTransformationMatrix();
        }
        private void UpdateTransformationMatrix()
        {
            
        }

        public virtual void ProcessEvents()
        {
            
        }

        public virtual void Update()
        {
            
        }

        public virtual void OnLostFocus()
        {
            if (EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType().ToString().Equals("UnityEditor.InspectorWindow"))
                return;
            
            Context.Instance.ClearSelections();
        }
    }
}
