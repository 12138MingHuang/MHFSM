using System;

namespace MHFSM
{
    /// <summary>
    /// 比较类型枚举
    /// </summary>
    public enum CompareType
    {
        /// <summary>
        /// 大于
        /// </summary>
        Greater = 0,
        /// <summary>
        /// 小于
        /// </summary>
        Less,
        /// <summary>
        /// 等于
        /// </summary>
        Equal,
        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual,
    }
    
    [Serializable]    
    public class FSMConditionData
    {
        public float targetValue;
        public string parameterName;
        public CompareType compareType;
        
    }
}
