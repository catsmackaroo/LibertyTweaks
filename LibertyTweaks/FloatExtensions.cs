internal static class FloatExtensions
{

    /// <summary>
    /// Checks if this float is in between two other floats
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min">The min value.</param>
    /// <param name="max">The max value.</param>
    /// <returns>True if float is in range. Otherwise, false.</returns>
    public static bool InRange(this float value, float min, float max)
    {
        return min <= value && max >= value;
    }

}