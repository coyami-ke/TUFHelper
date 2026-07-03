using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property)]
public class ShowInOverlayerSettingsAttribute : Attribute
{
    public string LabelName { get; }
    public ShowInOverlayerSettingsAttribute(string labelName)
    {
        LabelName = labelName;
    }
}
