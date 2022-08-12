/// <summary>
/// Usage: RandomEnumUtil<EnumType>.Get();
/// </summary>
public static class RandomEnumUtil<T>
{
    private static readonly T[] m_Values;
    static RandomEnumUtil()
    {
        var values = System.Enum.GetValues(typeof(T));

        m_Values = new T[values.Length];
        for (int i = 0; i < m_Values.Length; i++)
            m_Values[i] = (T)values.GetValue(i);
    }
    public static T Get()
    {
        return m_Values[UnityEngine.Random.Range(0, m_Values.Length)];
    }
}