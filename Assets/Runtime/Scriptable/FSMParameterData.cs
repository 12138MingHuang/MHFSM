using System;
using UnityEngine;

namespace MHFSM
{
    public enum ParameterType
    {
        Float = 0,
        Int,
        Bool,
        Trigger
    }
    
    [Serializable]
    public class FSMParameterData
    {
        #region 字段
        
        public string name;
        public float value;
        public ParameterType parameterType;
        public Action onValueChange;
        public int nameHash;
        
        #endregion

        public float Value
        {
            get { return value; }
            set
            {
                if(Mathf.Approximately(this.value, value)) return;

                this.value = value;
                onValueChange?.Invoke();
            }
        }
    }
}
