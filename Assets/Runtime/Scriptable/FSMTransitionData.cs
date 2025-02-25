using System;
using System.Collections.Generic;

namespace MHFSM
{
    [Serializable]
    public class GroupCondition
    {
        public List<FSMConditionData> conditions = new List<FSMConditionData>();
    }

    [Serializable]
    public class FSMTransitionData
    {
        public string fromStateName;
        public string toStateName;

        public string Key
        {
            get
            {
                return $"{fromStateName}:{toStateName}";
            }
        }
        
        public List<FSMConditionData> conditions = new List<FSMConditionData>();
        public List<GroupCondition> groupConditions = new List<GroupCondition>();
        public bool autoSwitch = false;

        public bool Empty
        {
            get
            {
                if (conditions.Count != 0) return false;

                foreach (GroupCondition groupCondition in groupConditions)
                {
                    if (groupCondition.conditions.Count != 0) return false;
                }
                
                return true;
            }
        }
    }
}
