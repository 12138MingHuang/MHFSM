namespace MHFSM
{
    public class EqualCompare : IParameterCompare
    {

        public bool IsMeetCondition(FSMParameterData parameter, float value)
        {
            if (parameter == null) return false;

            return parameter.Value.Equals(value); // 比较是否相等
        }
    }
}
