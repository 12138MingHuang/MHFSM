namespace MHFSM
{
    public interface IParameterCompare
    {
        /// <summary>
        /// 比较参数是否满足条件，例如：参数大于或者等于某个值。
        /// </summary>
        /// <param name="parameter"> 参数</param>
        /// <param name="value"> 条件值</param>
        /// <returns> 是否满足条件</returns>
        bool IsMeetCondition(FSMParameterData parameter, float value);
    }
}
