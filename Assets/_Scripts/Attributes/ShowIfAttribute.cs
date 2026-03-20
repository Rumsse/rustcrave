using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionName { get; private set; }
    public bool ShowValue { get; private set; } // The value required to show the field

    // Default showValue is true, so [ShowIf("name")] still works as before
    public ShowIfAttribute(string conditionName, bool showValue = true)
    {
        this.ConditionName = conditionName;
        this.ShowValue = showValue;
    }
}