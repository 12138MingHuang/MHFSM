namespace MHFSM
{
    public class LessCompare : IParameterCompare
    {

        public bool IsMeetCondition(FSMParameterData parameter, float value)
        {
            if(parameter == null) return false;
            
            return parameter.Value.CompareTo(value) < 0; // 比较是否小于传入的值 (如果传入的值更大，则不满足条件)
        }
    }
}
