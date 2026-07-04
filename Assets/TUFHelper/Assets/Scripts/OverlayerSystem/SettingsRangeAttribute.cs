using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class SettingsRangeAttribute : Attribute
{
    public float MinValue { get; }
    public float MaxValue { get; }
    public SettingsRangeAttribute(float min, float max)
    {
        MinValue = min;
        MaxValue = max;
    }
}
