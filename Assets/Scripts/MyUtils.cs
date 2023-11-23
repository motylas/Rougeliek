using Random = System.Random;

public static class MyUtils
{
    public static readonly Random Random = new();
    
    public static float RandomFloat(float min, float max)
    {
        return (float)(Random.NextDouble() * (max - min) + min);
    }
}
