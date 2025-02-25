namespace MHFSM
{
    public class NotEqualCompare : IParameterCompare
    {

        public bool IsMeetCondition(FSMParameterData parameter, float value)
        {
            if(parameter == null) return false;

            return !parameter.Value.Equals(value); // 比较是否不相等
        }
    }
}
