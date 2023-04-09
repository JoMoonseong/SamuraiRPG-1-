using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    public enum LogLevel
    {
        Log = 0,
        Warning = 1,
        Error = 2,
        Message = 3,
        NoLogs = 99
    }

    public delegate void LogCallback(string text, LogLevel logLevel = LogLevel.Log);

    public static class Logger
    {
        public static void Log(string text)
        {
            Log(text, LogLevel.Log);
        }

        public static void Warning(string text)
        {
            Log(text, LogLevel.Warning);
        }

        public static void Error(string text)
        {
            Log(text, LogLevel.Error);
        }

        public static void Message(string text)
        {
            Log(text, LogLevel.Message);
        }

        public static void Log(string text, LogLevel logLevel = LogLevel.Log)
        {
            if (!IsLogging(logLevel))
                return;

            switch (logLevel)
            {
                case LogLevel.Log:
                    Debug.Log("Prefab Fixer: " + text);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning("Prefab Fixer: " + text);
                    break;
                case LogLevel.Error:
                    Debug.LogError("Prefab Fixer: " + text);
                    break;
                case LogLevel.Message:
                    Debug.Log("Prefab Fixer: " + text);
                    break;
                default:
                    break;
            }
        }

        public static bool IsLogging(LogLevel logLevel)
        {
            var settingsLogLevel = PrefabFixerSettings.GetOrCreateSettings().LogLevel;
            return logLevel >= settingsLogLevel;
        }
    }

    public enum LineEndingBehaviour { Majority, OSDefault, Windows, Unix }

    public enum TextEncoding
    {
        Default, Unicode, BigEndianUnicode, UTF7, UTF8, UTF32, ASCII
    }

    public class PrefabFixerSettings : ScriptableObject
    {
        public const string Version = "1.0.1";
        public const string SettingsFilePath = "Assets/PrefabFixerSettings.asset";
        protected static PrefabFixerSettings cachedSettings;

        [SerializeField]
        public LogLevel LogLevel = LogLevel.Log;

        [SerializeField, Tooltip(_IgnoreNestedPrefabErrors)]
        public bool IgnoreNestedPrefabErrors = false;
        public const string _IgnoreNestedPrefabErrors = "If enabled then broken nested prefabs will be unpacked and copied." +
            "\nOtherwise an error will be shown and the fix will be aborted. If you have nested broken prefabs you have to " +
            "fix them first before fixing the root.";

        [SerializeField, Tooltip(_ShowWarningDialog)]
        public bool ShowWarningDialog = true;
        public const string _ShowWarningDialog = "If a Prefab contains nested prefabs then these nested prefabs need to be fixed first." +
            "\nSet this to false to skip showing the dialog for each object. An error will be logged in the console anyways.";

        [SerializeField, Tooltip(_DeleteOriginal)]
        public bool DeleteOriginal = true;
        public const string _DeleteOriginal = "Prefabs are fixed by creating a new Prefab instance and copying all data." +
            "\nOnce that is done the original (broken) object is deleted. Set this to FALSE to keep the original object.";

        [SerializeField, Tooltip(_CloseAfterFix)]
        public bool CloseAfterFix = true;
        public const string _CloseAfterFix = "Close the window automatically after a successful fix.";

        [SerializeField, Tooltip(_KeepNameAfterReplaceTooltip)]
        public bool KeepNameAfterReplace = false;
        public const string _KeepNameAfterReplaceTooltip = "Should replaced objects retain their name? If disabled then name of the prefab used as replacement will be used.";

        public static PrefabFixerSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<PrefabFixerSettings>(SettingsFilePath);

                // Not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:PrefabFixerSettings");
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<PrefabFixerSettings>(path);
                    }
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<PrefabFixerSettings>();
                    cachedSettings.LogLevel = LogLevel.Warning;
                    cachedSettings.IgnoreNestedPrefabErrors = false;
                    cachedSettings.ShowWarningDialog = true;
                    cachedSettings.DeleteOriginal = true;
                    cachedSettings.KeepNameAfterReplace = false;
                    cachedSettings.CloseAfterFix = true;

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();
                }

                if (cachedSettings == null)
                {
                    EditorUtility.DisplayDialog("Error", "LineBreakFixer settings could not be found or created.", "Ok");
                }
            }
            return cachedSettings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        public static void SelectSettings()
        {
            var settings = PrefabFixerSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Prefab Fixer settings could not be found or created.", "Ok");
            }
        }
    }

    static class PrefabFixerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreatePrefabFixerSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Prefab Fixer", SettingsScope.Project)
            {
                label = "Prefab Fixer",
                guiHandler = (searchContext) =>
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.wordWrap = true;

                    var settings = PrefabFixerSettings.GetSerializedSettings();

                    EditorGUILayout.LabelField("Version: " + PrefabFixerSettings.Version);

                    drawField("LogLevel", "Log level:", null, settings, style);
                    drawField("IgnoreNestedPrefabErrors", "Ignore nested prefab errors:", PrefabFixerSettings._IgnoreNestedPrefabErrors, settings, style);
                    drawField("ShowWarningDialog", "Show warning dialog:", PrefabFixerSettings._ShowWarningDialog, settings, style);
                    drawField("DeleteOriginal", "Delete original:", PrefabFixerSettings._DeleteOriginal, settings, style);
                    drawField("CloseAfterFix", "Close after fix:", PrefabFixerSettings._CloseAfterFix, settings, style);
                    drawField("KeepNameAfterReplace", "Keep name after replace:", PrefabFixerSettings._KeepNameAfterReplaceTooltip, settings, style);

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "prefab fixer", "fix", "prefab", "broken", "connect", "reconnect" })
            };

            return provider;
        }

        static void drawField(string propertyName, string label, string tooltip, SerializedObject settings, GUIStyle style)
        {
            EditorGUILayout.PropertyField(settings.FindProperty(propertyName), new GUIContent(label));
            if (!string.IsNullOrEmpty(tooltip))
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(tooltip, style);
                GUILayout.EndVertical();
            }
        }
    }
}