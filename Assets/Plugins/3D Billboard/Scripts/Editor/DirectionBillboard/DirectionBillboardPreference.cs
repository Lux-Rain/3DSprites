using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com.DiazTeo.DirectionalSpriteEditor
{
    public static class DirectionBillboardPreference
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Directional Sprite Preference", SettingsScope.User)
            {
                label = "Directional Sprite Preference",
                guiHandler = (searchContext) =>
                {
                    var settings = DirectionBillboardSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(settings.FindProperty("currentDirection"), new GUIContent("Forward Position"));
                    settings.ApplyModifiedProperties();

                },
                keywords = new HashSet<string>(new[] { "Billboard", "Directional Sprite", "Current Direction" })
            };
            return provider;
        }
    }
}