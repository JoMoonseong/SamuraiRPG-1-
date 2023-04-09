using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using static Kamgam.PF.PrefabFixer;
using System.Threading;
using System.Threading.Tasks;

namespace Kamgam.PF
{
    public class PrefabFixerWindow : EditorWindow
    {
        public static GUIStyle ErrorGroupDetailsButtonBoxStyle;
        public static Color WarningTextColor = new Color(0.9f, 0.2f, 0.2f);

        private static Color _DefaultBackgroundColor;
        public static Color DefaultBackgroundColor
        {
            get
            {
                if (_DefaultBackgroundColor.a == 0)
                {
                    try
                    {
                        var method = typeof(EditorGUIUtility).GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                        _DefaultBackgroundColor = (Color)method.Invoke(null, null);
                    }
                    catch
                    {
                        // fallback if reflection fails
                        _DefaultBackgroundColor = new Color32(56, 56, 56, 255);
                    }
                }
                return _DefaultBackgroundColor;
            }
        }

        // Input
        protected List<ObjectToFix> objectsToFix;
        protected Command command;

        // State
        protected Vector2 mainScrollPos = Vector2.zero;
        protected GameObject allPrefab;
        protected int isFixing;
        protected CancellationTokenSource workCTS;
        protected string statusLog;

        [MenuItem("Window/Prefab Fixer")]
        static PrefabFixerWindow openWindow()
        {
            PrefabFixerWindow window = (PrefabFixerWindow)EditorWindow.GetWindow(typeof(PrefabFixerWindow));
            window.titleContent = new GUIContent("Prefab Fixer");
            window.Initialize();
            window.Show();
            return window;
        }

        public static PrefabFixerWindow GetOrOpen()
        {
            if (!HasOpenInstances<PrefabFixerWindow>())
            {
                var window = openWindow();
                window.Focus();
                return window;
            }
            else
            {
                var window = GetWindow<PrefabFixerWindow>();
                window.Focus();
                return window;
            }
        }

        public string GetCommandName(bool upperCaseFirst = false)
        {
            switch (command)
            {
                case Command.Fix:
                    if (upperCaseFirst)
                        return "Fix";
                    else
                        return "fix";
                    
                case Command.Replace:
                    if (upperCaseFirst)
                        return "Replace";
                    else
                        return "replace";
                default:
                    return "Unknown";
            }
        }

        public void OnEnable()
        {
            Initialize();
        }

        public void OnDisable()
        {
            if(objectsToFix != null)
                objectsToFix.Clear();
        }

        public void Initialize()
        {
            ErrorGroupDetailsButtonBoxStyle = BackgroundStyle.Get(DefaultBackgroundColor);

            if (!isDocked())
            {
                if (position.width < 415 || position.height < 100)
                {
                    const int width = 415;
                    const int height = 200;
                    var x = Screen.currentResolution.width / 2 - width;
                    var y = Screen.currentResolution.height / 2 - height;
                    position = new Rect(x, y, width, height);
                }
            }
        }

        public void SetData(List<ObjectToFix> objectsToFix, Command command)
        {
            this.objectsToFix = objectsToFix;
            this.command = command;
            isFixing = 0;
            allPrefab = null;

            EditorApplication.update -= updateHeight;
            EditorApplication.update += updateHeight;
        }

        void updateHeight()
        {
            EditorApplication.update -= updateHeight;

            var newPos = position;
            newPos.height = Mathf.Min(500, objectsToFix.Count * 25 + 110 + (command == Command.Replace ? 50 : 0));
            position = newPos;
        }

        protected bool isDocked()
        {
#if UNITY_2020_1_OR_NEWER
            return docked;
#else
            return true;
#endif
        }

        void OnGUI()
        {
            // Title bar with buttons
            var settings = PrefabFixerSettings.GetOrCreateSettings();

            GUILayout.BeginHorizontal();
            DrawLabel(GetCommandName(true), bold: true, wordwrap: false);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version " + PrefabFixerSettings.Version + " ");
            if (DrawButton(" Manual ", icon: "_Help"))
            {
                OpenManual();
            }
            if (DrawButton(" Settings ", icon: "_Popup"))
            {
                OpenSettings();
            }
            GUILayout.EndHorizontal();


            // List of objects

            GUILayout.Space(5);

            // header
            float prefabWidth = position.width * 0.5f - 55;
            GUILayout.BeginHorizontal();
            DrawLabel("Object", bold: true, wordwrap: false);
            GUILayout.FlexibleSpace();
            if(position.width < 300)
                DrawLabel("-->", color: Color.gray);
            else
                DrawLabel("-- will become -->", color: Color.gray);
            DrawLabel("Prefab", bold: true, wordwrap: false, options: GUILayout.MaxWidth(prefabWidth + 75));
            GUILayout.EndHorizontal();

            // Settings (replace)
            if (command == Command.Replace)
            {
                GUILayout.BeginVertical("Replace Settings", EditorStyles.helpBox);
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();
                settings.KeepNameAfterReplace = EditorGUILayout.Toggle(new GUIContent("Keep name:", PrefabFixerSettings._KeepNameAfterReplaceTooltip), settings.KeepNameAfterReplace);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            // prefab for all
            if (objectsToFix != null && objectsToFix.Count > 1)
            {
                GUILayout.BeginHorizontal();
                DrawLabel("Set Prefab for all:", wordwrap: false);
                GUILayout.FlexibleSpace();
                var newAllPrefab = EditorGUILayout.ObjectField(allPrefab, typeof(GameObject), false, GUILayout.MaxWidth(prefabWidth)) as GameObject;
                if (newAllPrefab != allPrefab)
                {
                    allPrefab = newAllPrefab;
                    foreach (var obj in objectsToFix)
                    {
                        obj.Prefab = allPrefab;
                    }
                }
                GUILayout.EndHorizontal();
            }

            // List of objects
            if (objectsToFix != null && objectsToFix.Count > 0)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                mainScrollPos = GUILayout.BeginScrollView(mainScrollPos);
                GUI.enabled = isFixing == 0;

                foreach (var obj in objectsToFix)
                {
                    if (obj == null || obj.GameObject == null)
                        continue;

                    GUILayout.BeginHorizontal();
                    DrawLabel(obj.GameObject.name, wordwrap: false, options: GUILayout.MaxWidth(prefabWidth));
                    GUILayout.FlexibleSpace();
                    DrawLabel(" --> ", color: Color.gray, wordwrap: false);
                    obj.Prefab = EditorGUILayout.ObjectField(obj.Prefab, typeof(GameObject), false, GUILayout.MaxWidth(prefabWidth)) as GameObject;
                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = obj.Prefab != null && GUI.enabled;
                    if (GUILayout.Button(GetCommandName(true), GUILayout.MaxWidth(63)))
                    {
                        executeCommandOnObjectAsync(obj, settings);
                    }
                    GUI.enabled = wasEnabled;
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView(); 

                // Fix ALL button
                string buttonText = GetCommandName(true) + ((objectsToFix.Count > 1) ? " All" : "");
                if (isFixing != 0)
                    buttonText = "Working ..";
                if (GUILayout.Button(buttonText))
                {
                    isFixing = 10;
                    EditorApplication.update -= executeOnAll;
                    EditorApplication.update += executeOnAll;
                }

                if (!string.IsNullOrEmpty(statusLog))
                {
                    DrawLabel(statusLog);
                }

                GUI.enabled = true;
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            else
            {
                DrawLabel("Nothing selected.");
            }
        }

        void onStatusLog(string log)
        {
            statusLog = log;
            Repaint();
        }

        private async void executeOnAll()
        {
            if (workCTS != null)
                Logger.Error("Please wait until the previous Pref fix task completes.");

            workCTS = new CancellationTokenSource();
            var ct = workCTS.Token;
            try
            {
                // Give the window time to refresh
                if (isFixing > 1)
                    isFixing--;

                if (isFixing == 1)
                {
                    EditorApplication.update -= executeOnAll;
                    Undo.IncrementCurrentGroup();
                    foreach (var obj in objectsToFix)
                    {
                        await executeCommandOnObjectAsync(obj, PrefabFixerSettings.GetOrCreateSettings(), ct);
                    }
                    Undo.IncrementCurrentGroup();
                    isFixing = 0;
                    statusLog = null;

                    var settings = PrefabFixerSettings.GetOrCreateSettings();
                    if (settings.CloseAfterFix)
                        Close();
                }
            }
            finally
            {
                workCTS.Dispose();
                workCTS = null;
            }
        }

        private async void executeCommandOnObjectAsync(ObjectToFix obj, PrefabFixerSettings settings)
        {
            if (workCTS != null)
                Logger.Error("Please wait until the previous Pref fix task completes.");

            workCTS = new CancellationTokenSource();
            var ct = workCTS.Token;
            try
            {
                await executeCommandOnObjectAsync(obj, settings, ct);
            }
            finally
            {
                workCTS.Dispose();
            }
        }

        private async Task executeCommandOnObjectAsync(ObjectToFix obj, PrefabFixerSettings settings, CancellationToken ct)
        {
            if (obj == null || obj.GameObject == null || obj.Prefab == null)
                return;

            switch (command)
            {
                case Command.Fix:
                    
                    ReferenceReplacer.OnStatusLog = onStatusLog;
                    statusLog = "Working on it ..";

                    await PrefabFixer.FixObjectAsync(obj.GameObject, obj.Prefab, ct);
                    break;

                case Command.Replace:
                    PrefabFixer.ReplaceObject(obj.GameObject, obj.Prefab, settings.KeepNameAfterReplace);
                    break;

                default:
                    break;
            }
        }

        public void CancelCommand()
        {
            workCTS?.Cancel();
        }

        public static void OpenManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/PrefabFixer/PrefabFixerManual.pdf");
        }

        public void OpenSettings()
        {
            PrefabFixerSettings.SelectSettings();
        }

        #region Utilities

        public static class BackgroundStyle
        {
            private static GUIStyle style = new GUIStyle();
            private static Texture2D texture;

            public static GUIStyle Get(Color color)
            {
                if (texture == null)
                    texture = new Texture2D(1, 1);

                texture.SetPixel(0, 0, color);
                texture.Apply();
                style.normal.background = texture;

                return style;
            }
        }

        public static void DrawLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true, Texture icon = null, params GUILayoutOption[] options)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;

            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;
            style.imagePosition = ImagePosition.ImageLeft;

            var content = new GUIContent();
            content.text = text;
            if (icon != null)
            {
                GUILayout.Space(16);
                var position = GUILayoutUtility.GetRect(content, style);
                GUI.DrawTexture(new Rect(position.x - 16, position.y, 16, 16), icon);
                GUI.Label(position, content, style);
            }
            else
            {
                GUILayout.Label(text, style, options);
            }
        }

        public static void DrawSelectableLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;

            var content = new GUIContent(text);
            var position = GUILayoutUtility.GetRect(content, style);
            EditorGUI.SelectableLabel(position, text, style);
        }

        public static bool DrawButton(string text, string tooltip = null, string icon = null, params GUILayoutOption[] options)
        {
            GUIContent content;

            // icon
            if (!string.IsNullOrEmpty(icon))
                content = EditorGUIUtility.IconContent(icon);
            else
                content = new GUIContent();

            // text
            content.text = text;

            // tooltip
            if (!string.IsNullOrEmpty(tooltip))
                content.tooltip = tooltip;

            return GUILayout.Button(content, options);
        }

        public static void BeginHorizontalIndent(int indentAmount = 10, bool beginVerticalInside = true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentAmount);
            if (beginVerticalInside)
                GUILayout.BeginVertical();
        }

        public static void EndHorizontalIndent(float indentAmount = 10, bool begunVerticalInside = true, bool bothSides = false)
        {
            if (begunVerticalInside)
                GUILayout.EndVertical();
            if (bothSides)
                GUILayout.Space(indentAmount);
            GUILayout.EndHorizontal();
        }

        public static string WrapInRichTextColor(string text, Color color)
        {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);
            return "<color=#" + hexColor + ">" + text + "</color>";
        }

        #endregion
    }
}
