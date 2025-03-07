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
        private float _value;
        public ParameterType parameterType;
        public Action onValueChange;
        public int nameHash;
        
        #endregion

        public float Value
        {
            get { return _value; }
            set
            {
                if(Mathf.Approximately(_value, value)) return;

                _value = value;
                onValueChange?.Invoke();
            }
        }
    }
}
