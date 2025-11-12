using UnityEngine;

public readonly struct DamageInfo
{
    public readonly float Amount;
    public readonly Vector2 Point;
    public readonly Vector2 Normal;
    public readonly GameObject Source;

    public DamageInfo(float amount, Vector2 point, Vector2 normal, GameObject source = null)
    {
        Amount = amount;
        Point = point;
        Normal = normal;
        Source = source;
    }
}