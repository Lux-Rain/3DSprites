using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Com.DiazTeo.DirectionalSpriteEditor
{
    [Serializable]
    public class DirectionBillboardSettings : ScriptableObject
    {
        public const string k_DirectionBillboardSettingsPath = "Assets/Editor/DirectionBillboardSettings.asset";
        public const string k_directoryPath = "Assets/Editor";
        public enum DirectionSettings
        {
            up,
            down,
        }
        [SerializeField]
        protected DirectionSettings currentDirection;

        internal static DirectionBillboardSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<DirectionBillboardSettings>(k_DirectionBillboardSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<DirectionBillboardSettings>();
                settings.currentDirection = DirectionSettings.up;
                if (!Directory.Exists(k_directoryPath))
                {
                    //if it doesn't, create it
                    Directory.CreateDirectory(k_directoryPath);

                }
                AssetDatabase.CreateAsset(settings, k_DirectionBillboardSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}
