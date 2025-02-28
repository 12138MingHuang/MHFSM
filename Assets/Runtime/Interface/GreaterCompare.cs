namespace MHFSM
{
    public class GreaterCompare : IParameterCompare
    {

        public bool IsMeetCondition(FSMParameterData parameter, float value)
        {
            if(parameter == null) return false;
            return parameter.Value.CompareTo(value) == 1; 
        }
    }
}
