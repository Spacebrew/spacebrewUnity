#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Quin Kennedy

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class SBCustomTypeAttribute : PropertyAttribute
{
    public string ConditionalSourceField = "";
    public bool Inverse = false;

	// Use this for initialization
    public SBCustomTypeAttribute(string conditionalSourceField, bool inverse)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.Inverse = inverse;
    }

}
#endif