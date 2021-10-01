#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Quin Kennedy

[CustomPropertyDrawer(typeof(SBCustomTypeAttribute))]
public class SBCustomTypePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        SBCustomTypeAttribute condHAtt = (SBCustomTypeAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SBCustomTypeAttribute condHAtt = (SBCustomTypeAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            //The property is not being drawn
            //We want to undo the spacing added before and after the property
            return -EditorGUIUtility.standardVerticalSpacing;
            //return 0.0f;
        }


        /*
        //Get the base height when not expanded
        var height = base.GetPropertyHeight(property, label);

        // if the property is expanded go through all its children and get their height
        if (property.isExpanded)
        {
            var propEnum = property.GetEnumerator();
            while (propEnum.MoveNext())
                height += EditorGUI.GetPropertyHeight((SerializedProperty)propEnum.Current, GUIContent.none, true);
        }
        return height;*/
    }

    private bool GetConditionalHideAttributeResult(SBCustomTypeAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = true;

        //Handle primary property
        SerializedProperty sourcePropertyValue = null;

        //Get the full relative property path of the sourcefield so we can have nested hiding.
        string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
        string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
        sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            SpacebrewClient.type pubTypeValue = (SpacebrewClient.type)sourcePropertyValue.enumValueIndex;
            enabled = pubTypeValue == SpacebrewClient.type.CUSTOM;
        }
        else
        {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }



        //wrap it all up
        if (condHAtt.Inverse) enabled = !enabled;

        return enabled;
    }
}
#endif