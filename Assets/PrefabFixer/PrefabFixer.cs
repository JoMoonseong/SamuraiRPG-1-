using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    public class PrefabFixer
    {
        public enum Command { Fix, Replace }

        public static List<string> PrefabPaths = new List<string>();
        public static List<ObjectToFix> ObjectsToFix = new List<ObjectToFix>();

        [MenuItem("GameObject/Prefab/Fix", priority =
#if UNITY_2020_1_OR_NEWER
            100
#else
            47
            // See https://forum.unity.com/threads/solved-adding-context-menus-to-game-object.410353/#post-2677829
            // It seems this is fixed in 2020+
#endif
            )]
        public static void FixSelected(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            // Thanks to https://answers.unity.com/questions/608256/how-to-execute-menuitem-for-multiple-objects-once.html
            if (Selection.objects.Length > 1 && menuCommand.context != Selection.objects[0])
                return;

            FixSelected();
        }

        [MenuItem("GameObject/Prefab/Replace", priority =
#if UNITY_2020_1_OR_NEWER
            101
#else
            48
            // See https://forum.unity.com/threads/solved-adding-context-menus-to-game-object.410353/#post-2677829
            // It seems this is fixed in 2020+
#endif
            )]
        public static void ReplaceSelected(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            // Thanks to https://answers.unity.com/questions/608256/how-to-execute-menuitem-for-multiple-objects-once.html
            if (Selection.objects.Length > 1 && menuCommand.context != Selection.objects[0])
                return;

            ReplaceSelected();
        }

        [MenuItem("Tools/Prefab Fixer/Fix Selected")]
        public static void FixSelected()
        {
            UpdatePrefabList();
            ObjectsToFix.Clear();

            var settings = PrefabFixerSettings.GetOrCreateSettings();

            var selectedObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel);
            foreach (GameObject go in selectedObjects)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                // Disconnected Prefabs only happens on old Unity versions which do not yet have nested prefabs. This is not supported.
                if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Disconnected)
                {
                    Logger.Message(go.name + " is disconnected. Fixing disconnected Prefabs is not supported anymore.");
                    continue;
                }
#pragma warning restore CS0618 // Type or member is obsolete

                // Check if the Prefab contains broken prefabs (if yes then skip)
                bool hasBrokenNestedPrefab = PrefabUtilityExtensions.HasBrokenNestedPrefabs(go.transform);
                if (hasBrokenNestedPrefab)
                {
                    string warningMessage = $"'{go.name}' contains broken nested Prefabs. IgnoreNestedPrefabErrors is ON and thus this nested prefab wil be UNPACKED and copied.";
                    if (settings.IgnoreNestedPrefabErrors)
                    {
                        Logger.Warning(warningMessage);
                    }
                    else
                    {
                        bool skip = true;
                        string message = $"'{go.name}' contains broken nested Prefabs. This can not be handled automatically. Please open the prefab and fix the missing nested prefabs first.";
                        if (settings.ShowWarningDialog)
                            skip = EditorUtility.DisplayDialog("Broken Prefab inside Prefab!", message, "Undersstood (skip)", "Unpack and proceed");

                        if (skip)
                        {
                            Logger.Error(message);
                            continue;
                        }
                        else
                        {
                            Logger.Warning(warningMessage);
                        }
                    }
                }

                // Check if the object contins broken prefabs references
                bool hasBrokenPrefab = PrefabUtilityExtensions.HasBrokenPrefabs(go.transform, includeSelf: false);
                if (hasBrokenPrefab)
                {
                    string warningMessage = $"'{go.name}' contains broken Prefabs. IgnoreNestedPrefabErrors is ON and thus this nested prefab wil be UNPACKED and copied.";
                    if (settings.IgnoreNestedPrefabErrors)
                    {
                        Logger.Warning(warningMessage);
                    }
                    else
                    {
                        bool skip = true;
                        string message = $"'{go.name}' contains broken Prefabs. This can not be handled automatically. Please fix the missing prefabs within the object first and then proceed with fixing this.";
                        if (settings.ShowWarningDialog)
                            skip = EditorUtility.DisplayDialog("Broken Prefab inside Prefab!", message, "Undersstood (skip)", "Unpack and proceed");

                        if (skip)
                        {
                            Logger.Error(message);
                            continue;
                        }
                        else
                        {
                            Logger.Warning(warningMessage);
                        }
                    }
                }

                // Schedule all for fixing
                var bestMatch = FindBestMatchingPrefabFor(go.name, PrefabPaths, 3);
                // try to find again with a more premissive setting
                if(bestMatch == null)
                    bestMatch = FindBestMatchingPrefabFor(go.name, PrefabPaths, 6);
                var obj = new ObjectToFix(go, bestMatch);
                ObjectsToFix.Add(obj);
            }

            ObjectsToFix.Sort(ObjectToFix.CompareAsc);

            var window = PrefabFixerWindow.GetOrOpen();
            window.SetData(ObjectsToFix, Command.Fix);
            window.Show(immediateDisplay: true);
        }

        [MenuItem("Tools/Prefab Fixer/Replace Selected")]
        public static void ReplaceSelected()
        {
            UpdatePrefabList();
            ObjectsToFix.Clear();

            var settings = PrefabFixerSettings.GetOrCreateSettings();

            var selectedObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel);
            foreach (GameObject go in selectedObjects)
            {
                // Schedule all for replacing
                // var bestMatch = FindBestMatchingPrefabFor(go.name); // Don't suggest best mtach for replacing.
                var obj = new ObjectToFix(go, null);
                ObjectsToFix.Add(obj);
            }

            ObjectsToFix.Sort(ObjectToFix.CompareAsc);

            var window = PrefabFixerWindow.GetOrOpen();
            window.SetData(ObjectsToFix, Command.Replace);
            window.Show(immediateDisplay: true);
        }

        public static void UpdatePrefabList()
        {
            PrefabPaths.Clear();

            var guids = AssetDatabase.FindAssets("t:prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                PrefabPaths.Add(path);
            }
        }

        public static void ReplaceObject(GameObject source, GameObject prefab, bool keepName)
        {
            var settings = PrefabFixerSettings.GetOrCreateSettings();

            // Instantiate new prefab
            var target = PrefabUtility.InstantiatePrefab(prefab, source.transform.parent) as GameObject;
            if (keepName)
                target.name = source.name;
            target.transform.SetSiblingIndex(source.transform.GetSiblingIndex());
            Undo.RegisterCreatedObjectUndo(target, "Add new instance (prefab fixer).");
            
            // Copy data
            copySingleComponentData(source.transform, target.transform);

            // Replace references (this takes a lot of time as it visits
            // EVERY components properties in the entire scene).
            ReferenceReplacer.ReplaceReference(source, target);

            // Delete original
            if (settings.DeleteOriginal)
                Undo.DestroyObjectImmediate(source);
        }

        public static async Task FixObjectAsync(GameObject source, GameObject prefab, CancellationToken ct)
        {
            var settings = PrefabFixerSettings.GetOrCreateSettings();

            // Instantiate new prefab
            var target = PrefabUtility.InstantiatePrefab(prefab, source.transform.parent) as GameObject;
            target.name = source.name;
            target.transform.SetSiblingIndex(source.transform.GetSiblingIndex());
            Undo.RegisterCreatedObjectUndo(target, "Add new instance (prefab fixer).");

            // copy data & children recursively
            copyComponentDataTo(source, target);
            copyChildObjectsTo(source, target, copyComponentData: true);

            // Replace references (this takes a lot of time as it visits
            // EVERY components properties in the entire scene).
            await ReferenceReplacer.ReplaceReferencesHierarchicallyAsync(source, target, ct);

            // Fix Prefab overrides
            PrefabOverrideCleaner.CleanOverrides(target);

            // Delete broken prefab instance
            if (settings.DeleteOriginal)
                Undo.DestroyObjectImmediate(source);
        }

        /// <summary>
        /// Recurses into the source and copies all the children to the target.<br />
        /// NOTICE: Neither source nor target themselves are changed (it's children only!).<br />
        /// If the targe already contains a child with the same name then this will be used as copy target.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="copyComponentData"></param>
        static void copyChildObjectsTo(GameObject source, GameObject target, bool copyComponentData)
        {
            // memorize equally named target children to avoid handling the same target child twice.
            var handledTargetObjects = new List<Transform>();

            // children in source which are not found by name in target will be appended to the target as new objects
            var sourceChildrenToAdd = new List<Transform>();

            for (int s = 0; s < source.transform.childCount; s++)
            {
                var sourceChild = source.transform.GetChild(s);
                // Handle equally named objects
                bool foundInTarget = false;
                for (int t = 0; t < target.transform.childCount; t++)
                {
                    var targetChild = target.transform.GetChild(t);

                    if (handledTargetObjects.Contains(targetChild))
                        continue;

                    if (sourceChild.name.ToLower() == targetChild.name.ToLower())
                    {
                        handledTargetObjects.Add(targetChild);
                        if (copyComponentData)
                        {
                            copyComponentDataTo(sourceChild.gameObject, targetChild.gameObject);
                        }

                        copyChildObjectsTo(sourceChild.gameObject, targetChild.gameObject, copyComponentData);
                        foundInTarget = true;
                    }
                }

                if(!foundInTarget)
                {
                    sourceChildrenToAdd.Add(sourceChild);
                }
            }

            foreach (var sourceChild in sourceChildrenToAdd)
            {
                // copy missing game object hierarchy
                var copy = HierarchyUtils.CopyHierarchyTo(sourceChild.gameObject, target.transform);
                copy.transform.SetAsLastSibling();

                copyComponentDataTo(sourceChild.gameObject, copy);

                // Component data in children does not have to be copied because
                // copyHierarchyTo() has already taken care of that.
                //
                // Keep in mind: There may still be references from the copy back to
                // the sourceChild. These will be fixed later in ReplaceReferences().
            }
        }

        /// <summary>
        /// Copies all the component(s) data from source to target.<br />
        /// <br />
        /// If a component on source does not exists on target then it is 
        /// created and then filled with the data from source.<br />
        /// <br />
        /// If target has components that do not exist on the source
        /// then these are removed from the target.<br />
        /// <br />
        /// In the end the target will have the same amount and type
        /// of components as the source with all the source data
        /// copied over.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        static void copyComponentDataTo(GameObject source, GameObject target)
        {
            List<System.Type> handledType = new List<System.Type>();

            // Remove all components that do exist in target but not in source.
            handledType.Clear();
            var targetComps = target.GetComponents<Component>();
            foreach (var comp in targetComps)
            {
                if (handledType.Contains(comp.GetType()))
                    continue;

                var targetCompsForType = target.GetComponents(comp.GetType());
                var sourceCompsForType = source.GetComponents(comp.GetType());
                if (sourceCompsForType.Length < targetCompsForType.Length)
                {
                    for (int i = sourceCompsForType.Length; i < targetCompsForType.Length; i++)
                    {
                        // If case you are trying to replace a RectTransform with
                        // a Transform then this will throw and error (ie. the object you
                        // are trying to replace/fix as a Transform but the Prefab has
                        // a RectTransform (or the other way around).
                        // Switching transforms is not supported at the moment.
                        GameObject.DestroyImmediate(targetCompsForType[i]);
                        if (Logger.IsLogging(LogLevel.Log))
                            Logger.Log($"Removing component[{i}] {comp.GetType()} from new {target.name}");
                    }
                }

                // remember type to not handle it again
                handledType.Add(comp.GetType());
            }

            // Add all components that are missing (source to target)
            handledType.Clear();
            var sourceComps = source.GetComponents<Component>();
            foreach (var comp in sourceComps)
            {
                if (handledType.Contains(comp.GetType()))
                    continue;

                var targetCompsForType = target.GetComponents(comp.GetType());
                var sourceCompsForType = source.GetComponents(comp.GetType());
                if (sourceCompsForType.Length > targetCompsForType.Length)
                {
                    for (int i = targetCompsForType.Length; i < sourceCompsForType.Length; i++)
                    {
                        target.AddComponent(comp.GetType());
                        if (Logger.IsLogging(LogLevel.Log))
                            Logger.Log($"Adding component[{i}] {comp.GetType()} to new {target.name}");
                    }
                }

                // Remember the type to avoid handling it again (multiple comps with same type).
                handledType.Add(comp.GetType());
            }

            // Copy all serializable data
            handledType.Clear();
            sourceComps = source.GetComponents<Component>();
            foreach (var comp in sourceComps)
            {
                if (handledType.Contains(comp.GetType()))
                    continue;

                var targetCompsForType = target.GetComponents(comp.GetType());
                var sourceCompsForType = source.GetComponents(comp.GetType());
                if (sourceCompsForType.Length > targetCompsForType.Length)
                {
                    Logger.Warning("Source component count is greater than target component count! Some data will NOT be copied.");
                }

                // copy data per component
                int compCount = Mathf.Min(sourceCompsForType.Length, targetCompsForType.Length);
                for (int i = 0; i < compCount; i++)
                {
                    var sourceComp = sourceCompsForType[0];
                    var targetComp = targetCompsForType[0];

                    copySingleComponentData(sourceComp, targetComp);
                }

                // Remember the type to avoid handling it again (multiple comps with same type).
                handledType.Add(comp.GetType());
            }
        }

        private static void copySingleComponentData(Component source, Component target)
        {
            SerializedObject targetSerializedObj = new SerializedObject(target);
            targetSerializedObj.Update();

            SerializedObject sourceSerializedObj = new SerializedObject(source);
            sourceSerializedObj.Update();

            if (Logger.IsLogging(LogLevel.Log))
                Logger.Log($"Copying data from {source.gameObject.name}/{target.GetType()} to {target.gameObject.name}/{target.GetType()}");

            SerializedProperty prop = sourceSerializedObj.GetIterator();
            while (prop.NextVisible(true))
            {
                // Keep in mind: A) Copying object references will make them all prefab overrides, even
                //                  if they are the same before and after.
                //               B) Some references will be broken because after this we do delete the
                //                  old object and thus all refs to its children will be broken
                // We fix both in the ReplaceReferences() step AFTER all the copy work has finished.
                targetSerializedObj.CopyFromSerializedProperty(prop);
            }
            targetSerializedObj.ApplyModifiedProperties();
        }

        /// <summary>
        /// Finds the best machting prefab in prefabPaths.
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="prefabPaths">Paths to search in for similar names.</param>
        /// <param name="maxDistance">The maximum Levenshtein Distance which is allowed.</param>
        /// <returns></returns>
        public static GameObject FindBestMatchingPrefabFor(string objectName, List<string> prefabPaths, int maxDistance)
        {
            // Remove some common stuff to make the matching easier
            string canonicalObjectName = objectName.Replace(" ", "").ToLowerInvariant();
            var regEx = new Regex(@"\([0-9]+\)");
            canonicalObjectName = regEx.Replace(canonicalObjectName, "");

            // Remove "(Missing Prefab with guid: 197c7389b2d10c04ca1bf35fe1e073dd)" like messages.
            // That's the message Unity appends to broken nested prefabs.
            regEx = new Regex(@"\(missing.*guid:[a-z0-9]{30,60}\)");
            canonicalObjectName = regEx.Replace(canonicalObjectName, "");

            string resultPath = null;
            int minDistance = 99099;
            foreach (var path in prefabPaths)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                string canonicalfileName = fileName.Replace(" ", "").ToLowerInvariant();
                int distance = TextUtils.CalcLevenshteinDistance(canonicalfileName, canonicalObjectName);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    resultPath = path;
                }
            }

            if (minDistance > maxDistance)
                return null;

            var result = AssetDatabase.LoadAssetAtPath<GameObject>(resultPath);
            return result;
        }
    }
}