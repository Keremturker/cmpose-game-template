using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kturker.UnityBase.Editor.Build
{
    // Release signing from <project>/keystore.properties for one build. Returns the action that restores the
    // previous keystore settings (ProjectSettings.asset never changes), or null when there is no file (debug key).
    public static class AndroidSigning
    {
        public const string FileName = "keystore.properties";
        private const string PlayerSettingsAsset = "ProjectSettings/ProjectSettings.asset";
        private const string KeystoreNameProperty = "AndroidKeystoreName";

        public static Action Apply(string projectRoot)
        {
            var file = Path.Combine(projectRoot, FileName);
            if (!File.Exists(file))
            {
                Debug.Log("CMPOSE_SIGNING debug");
                return null;
            }
            var config = KeystoreConfig.Parse(File.ReadAllText(file), projectRoot, File.Exists);
            var useCustom = PlayerSettings.Android.useCustomKeystore;
            var keystoreName = StoredKeystoreName().stringValue;
            var keystorePass = PlayerSettings.Android.keystorePass;
            var aliasName = PlayerSettings.Android.keyaliasName;
            var aliasPass = PlayerSettings.Android.keyaliasPass;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = config.StoreFile;
            PlayerSettings.Android.keystorePass = config.StorePassword;
            PlayerSettings.Android.keyaliasName = config.KeyAlias;
            PlayerSettings.Android.keyaliasPass = config.KeyPassword;
            Debug.Log($"CMPOSE_SIGNING keystore alias={config.KeyAlias}");
            return () =>
            {
                PlayerSettings.Android.useCustomKeystore = useCustom;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = aliasName;
                PlayerSettings.Android.keyaliasPass = aliasPass;
                // The keystoreName setter stores "" as '{inproject}: ', so the serialized value is put back as it was
                var stored = StoredKeystoreName();
                stored.stringValue = keystoreName;
                stored.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            };
        }

        private static SerializedProperty StoredKeystoreName()
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(PlayerSettingsAsset)[0]);
            return settings.FindProperty(KeystoreNameProperty);
        }
    }
}
