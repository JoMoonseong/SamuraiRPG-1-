using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    public static class PrefabUtilityExtensions
    {
        /// <summary>
        /// Searches all child prefabs in the hierarchy for broken prefabs.<br />
        /// Notice: It does not detect broken prefab instances at root level!
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static bool HasBrokenNestedPrefabs(Transform transform)
        {
            // Extract all prefab roots from the hierarchy.
            var prefabsRoots = new List<Transform>();
            SearchThroughHierarchy(transform, isPrefabRoot, prefabsRoots);

            foreach (var prefab in prefabsRoots)
            {
                // Load the prefab into a temporary stage to check for missing nested prefabs.
                // see: https://forum.unity.com/threads/how-to-detect-broken-nested-prefabs.1321188/
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
                if (!string.IsNullOrEmpty(path))
                {
                    var root = PrefabUtility.LoadPrefabContents(path);
                    var missingPrefab = SearchThroughHierarchy(root.transform, isMissingAsset);
                    // missingPrefab will be NULL after PrefabUtility.UnloadPrefabContents().
                    // Thus the hasMissingPrefab bool
                    bool hasMissingPrefab = missingPrefab != null;
                    PrefabUtility.UnloadPrefabContents(root);

                    if (hasMissingPrefab)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Searches all child objects for broken prefab references.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="includeSelf"></param>
        /// <returns></returns>
        public static bool HasBrokenPrefabs(Transform transform, bool includeSelf)
        {
            var result = SearchThroughHierarchy(transform, isMissingAsset, includeSelf);
            return result != null && result != transform;
        }

        static bool isPrefabRoot(Transform transform)
        {
            return PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject);
        }

        static bool isMissingAsset(Transform transform)
        {
            return
                PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)
                && PrefabUtility.GetPrefabInstanceStatus(transform.gameObject) == PrefabInstanceStatus.MissingAsset;
        }

        /// <summary>
        /// Goes through all the children in the hierarchy and adds those which satisfy the addCondition to results.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="endCondition"></param>
        /// <param name="includeSelf"></param>
        /// <returns>Returns the one Transform that satisfied the endCondition OR NULL if none was found.</returns>
        public static void SearchThroughHierarchy(Transform transform, System.Predicate<Transform> addCondition, List<Transform> results, bool includeSelf = true)
        {
            if (includeSelf && addCondition(transform))
            {
                results.Add(transform);
                return;
            }

            includeSelf = true;

            for (int i = 0; i < transform.childCount; i++)
            {
                SearchThroughHierarchy(transform.GetChild(i), addCondition, results);
            }
        }

        /// <summary>
        /// Goes through all the children in the hierarchy and stops at the first which satisfies the endCondition.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="endCondition"></param>
        /// <param name="includeSelf"></param>
        /// <returns>Returns the one Transform that satisfied the endCondition OR NULL if none was found.</returns>
        public static Transform SearchThroughHierarchy(Transform transform, System.Predicate<Transform> endCondition, bool includeSelf = true)
        {
            if (includeSelf && endCondition(transform))
                return transform;

            includeSelf = true;

            Transform result = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                result = SearchThroughHierarchy(transform.GetChild(i), endCondition, includeSelf);
                if (result != null)
                    break;
            }
            return result;
        }

        public static bool IsPartOfPrefabContents(GameObject gameObject)
        {
#if UNITY_2021_2_OR_NEWER
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
            if (prefabStage == null)
                return false;

            return prefabStage.IsPartOfPrefabContents(gameObject);
        }
    }
}