using MHFSM;
using System;
using System.Collections.Generic;

namespace Runtime
{
    public enum ConditionState
    {
        /// <summary>
        /// 条件满足
        /// </summary>
        Meet,
        /// <summary>
        /// 条件不满足
        /// </summary>
        NotMeet,
    }
    
    public class FSMCondition
    {
        private FSMConditionData _data;
        private FSMParameterData _parameter;
        public Action onConditionMeet;
        private static Dictionary<CompareType, IParameterCompare> _compareDict = new Dictionary<CompareType, IParameterCompare>();

        public ConditionState State
        {
            get
            {
                return compare.IsMeetCondition(_parameter, _data.targetValue) ? ConditionState.Meet : ConditionState.NotMeet;
            }
        }
        
        private IParameterCompare compare => GetCompare(_data.compareType);
        public FSMParameterData Parameter => _parameter;

        static FSMCondition()
        {
            InitCompares();
        }

        public FSMCondition(FSMConditionData data, RuntimeFSMControllerInstance controller)
        {
            _data = data;
            // TODO: 参数名hash冲突问题
            int parameterNameHash;
            
        }

        /// <summary>
        /// 检查参数值变化，如果满足条件则执行回调函数
        /// </summary>
        public void CheckParameterValueChange()
        {
            // 判断条件是否满足
            if(State == ConditionState.Meet)
                onConditionMeet?.Invoke();
        }
        
        /// <summary>
        /// 初始化比较器字典
        /// </summary>
        private static void InitCompares()
        {
            if (_compareDict.Count == 0)
            {
                _compareDict.Add(CompareType.Equal, new EqualCompare());
                _compareDict.Add(CompareType.Greater, new GreaterCompare());
                _compareDict.Add(CompareType.Less, new LessCompare());
                _compareDict.Add(CompareType.NotEqual, new NotEqualCompare());
            }
        }

        /// <summary>
        /// 获取比较器
        /// </summary>
        /// <param name="compareType"> 比较类型 </param>
        /// <returns> 比较器 </returns>
        public static IParameterCompare GetCompare(CompareType compareType)
        {
            return _compareDict.GetValueOrDefault(compareType);
        }
    }

    public class FSMCondtionGroup
    {
        private List<FSMCondition> _conditions = new List<FSMCondition>();
        public Action onConditionMeet;

        public bool IsMeet
        {
            get
            {
                // 当前过渡没有条件并且自动切换时，直接返回true，已满足
                if(_transition.Empty && _transition.autoSwitch)
                    return true;

                if (ConditionCount == 0)
                    return false;
                
                foreach (FSMCondition condition in _conditions)
                {
                    if(condition.State == ConditionState.NotMeet)
                        return false;
                }
                return true;
            }
        }

        public int ConditionCount
        {
            get
            {
                return _conditions == null ? 0 : _conditions.Count;
            }
        }

        private RuntimeFSMControllerInstance _controller;
        private FSMTransitionData _transition;

        public FSMCondtionGroup(RuntimeFSMControllerInstance controller, List<FSMConditionData> conditions, FSMTransitionData transition)
        {
            _controller = controller;
            _transition = transition;
            _conditions.Clear();

            foreach (FSMConditionData condition in conditions)
            {
                FSMCondition fsmCondition = new FSMCondition(condition, _controller);
                fsmCondition.onConditionMeet += CheckIsMeet;
                _conditions.Add(fsmCondition);
            }
        }
        private void CheckIsMeet()
        {
            if (IsMeet) onConditionMeet?.Invoke();
        }

        internal void ResetTrigger()
        {
            foreach (FSMCondition condition in _conditions)
            {
                if (condition.Parameter.parameterType == ParameterType.Trigger)
                {
                    // TODO: 重置触发器
                }
            }
        }
    }
}
