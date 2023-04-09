using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    /// <summary>
    /// Reverts unnecessary overrides on prefabs.
    /// </summary>
    public static class PrefabOverrideCleaner
    {
        public static void CleanOverrides(GameObject prefabInstance)
        {
            string name = prefabInstance.name;

            if (PrefabUtility.IsPartOfAnyPrefab(prefabInstance) && PrefabUtility.HasPrefabInstanceAnyOverrides(prefabInstance, includeDefaultOverrides: true))
            {
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance);
                if (path != null)
                {
                    var unalteredPrefab = PrefabUtility.LoadPrefabContents(path);
                    HierarchyUtils.WalkHierarchiesInParallel(unalteredPrefab.transform, prefabInstance.transform, fixPrefabOverridesOnGameObject);
                    PrefabUtility.UnloadPrefabContents(unalteredPrefab);
                }
            }

            // Not sure why the name is reverted. We reset it here. TODO: investigate
            prefabInstance.name = name;
        }

        static void fixPrefabOverridesOnGameObject(Transform prefab, Transform target)
        {
            if (prefab == null || target == null)
                return;

            HierarchyUtils.IterateOverComponentsInParallel(prefab, target, fixPrefabOverridesOnComponent);
        }

        static void fixPrefabOverridesOnComponent(Component prefab, Component target)
        {
            if (prefab == null || target == null)
                return;

            var prefabRoot = prefab.transform.root; // Keep in mind, this is not a prefab so PrefabUtility methods won't work in this.
            var targetRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(target.transform);
            SerializedObject prefabData = new SerializedObject(prefab);
            SerializedObject targetData = new SerializedObject(target);
            SerializedProperty targetProp = targetData.GetIterator();
            while (targetProp.NextVisible(true))
            {
                if (targetProp.prefabOverride)
                {
                    var prefabProp = prefabData.FindProperty(targetProp.propertyPath);
                    if (prefabProp == null)
                    {
                        // The property does not exist on the prefab, thus we should keep the override as is.
                    }
                    else
                    {
                        if (targetProp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (targetProp.objectReferenceValue != null && prefabProp.objectReferenceValue != null)
                            {
                                // The transform of the object which the targetProp.objectReferenceValue points to.
                                Transform targetRefTransform = null;
                                GameObject targetGameObject = targetProp.objectReferenceValue as GameObject;

                                Component targetComponent = targetProp.objectReferenceValue as Component;
                                if (targetGameObject != null)
                                    targetRefTransform = targetGameObject.transform;
                                if (targetComponent != null)
                                    targetRefTransform = targetComponent.transform;

                                // If the reference points to something INSIDE the target prefab
                                // then we check if the override should be reverted by comparing
                                // the relative path to the unaltered prefab.
                                if (targetRefTransform != null && targetRefTransform.IsChildOf(targetRoot.transform))
                                {
                                    string pathInTarget;
                                    if (targetComponent != null)
                                        pathInTarget = HierarchyUtils.GetPathAsString(targetComponent, targetRoot.transform, addRoot: false);
                                    else
                                        pathInTarget = HierarchyUtils.GetPathAsString(targetGameObject.transform, targetRoot.transform, addRoot: false);

                                    // get the path of the objectReference in the property within the unaltered prefab.
                                    GameObject prefabGameObject = prefabProp.objectReferenceValue as GameObject;
                                    Component prefabComponent = prefabProp.objectReferenceValue as Component;
                                    string pathInUnalteredPrefab;
                                    if (prefabComponent != null)
                                        pathInUnalteredPrefab = HierarchyUtils.GetPathAsString(prefabComponent, prefabRoot.transform, addRoot: false);
                                    else
                                        pathInUnalteredPrefab = HierarchyUtils.GetPathAsString(prefabGameObject.transform, prefabRoot.transform, addRoot: false);

                                    // If the relative path withing the unaltered prefab is the SAME as the path in the
                                    // new instance (target) then we revert the reference.
                                    // TODO: Replace all this one we have found an easier way to check overridden values.
                                    if (pathInTarget == pathInUnalteredPrefab)
                                    {
                                        // Debug.Log("Reverting override on " + pathInTarget);
                                        PrefabUtility.RevertPropertyOverride(targetProp, InteractionMode.AutomatedAction);
                                    }
                                }
                                else
                                {
                                    // The object reference points to something outside the prafb which means
                                    // it has to be an override and therefor we leave it alone.

                                    // References to Assets like Materials, ScriptableObjects, etc. are handled
                                    // by Unity just fine (no overrides are created).
                                }
                            }
                            else
                            {
                                // if both are null -> revert
                                if (prefabProp == null && prefabProp.objectReferenceValue == null)
                                {
                                    PrefabUtility.RevertPropertyOverride(targetProp, InteractionMode.AutomatedAction);
                                }
                            }
                        }
                        else
                        {
                            // Unity gets data property overrides right in it's own. No need to change anything here.
                        }
                    }
                }
            }
            targetData.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}