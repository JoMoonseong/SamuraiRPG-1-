using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    /// <summary>
    /// Depends on HierarchyUtils.
    /// </summary>
    public static class ReferenceReplacer
    {
        public static System.Action<string> OnStatusLog;

        public static List<Component> AllComponentsInScene = new List<Component>();

        private static Transform tmpSource;
        private static Transform tmpTarget;
        private static Transform tmpSourceRoot;
        private static Component tmpSourceComp;
        private static Component tmpTargetComp;

        private static double lastYieldTime;
        private static double startTime;

        public static void UpdateAllComponentsInScene()
        {
            AllComponentsInScene.Clear();

            foreach (GameObject g in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                Component[] myComponents = g.GetComponents(typeof(Component));
                foreach (Component myComp in myComponents)
                {
                    if (myComp == null)
                        continue;

                    AllComponentsInScene.Add(myComp);
                }
            }
        }

        /// <summary>
        /// Replaces references to "source" with "target".<br />
        /// Has some hidden internal state. It's not thread safe.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void ReplaceReference(GameObject source, GameObject target)
        {
            if (source == null || target == null)
                return;

            UpdateAllComponentsInScene();

            // Object
            // Replace references to "source" with "target" in ALL the components in the scene (that's a lot).
            tmpSource = source.transform;
            tmpTarget = target.transform;
            HierarchyUtils.ForEveryComponentProperty(AllComponentsInScene, replaceGameObjectReferenceInProperty);
        }

        /// <summary>
        /// Here is what it does:<br />
        /// 1) Goes through every game object in "target" and checks if there is a
        ///    corresponding game object in "source". If yes it replaces all
        ///    references in the entire scene with the "target" object.<br />
        /// 2) Goes through every component in "target"  and checks if there is a
        ///    corresponding component in "source". If yes it replaces all
        ///    references in the entire scene with the "target" component.<br />
        /// <br />
        /// Has some hidden internal state. It's not thread safe.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static async Task ReplaceReferencesHierarchicallyAsync(GameObject source, GameObject target, CancellationToken ct)
        {
            startTime = EditorApplication.timeSinceStartup;

            UpdateAllComponentsInScene();

            tmpSourceRoot = source.transform;
            await HierarchyUtils.WalkHierarchiesInParallelByNameAsync(source.transform, target.transform, replaceReferencesOnTransformAsync, ct);
        }

        /// <summary>
        /// Depends on AllComponentsInScene, tmpSource, tmpTarget, tmpSourceComponent, tmpTargetComponent.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        static async Task replaceReferencesOnTransformAsync(Transform source, Transform target, CancellationToken ct)
        {
            if (source == null || target == null)
                return;

            await yieldAfterTime();

            tmpSource = source;
            tmpTarget = target;

            // Object
            // Replace references to "source" with "target" in ALL the components in the scene (that's a lot).
            HierarchyUtils.ForEveryComponentProperty(AllComponentsInScene, replaceGameObjectReferenceInProperty);

            // Components
            // For each component replace references to "sourceComp" with "targetComp" in ALL the components in the scene (that's a lot).
            HierarchyUtils.IterateOverComponentsInParallel(source, target, replaceComponentReferences);

        }

        static async Task yieldAfterTime()
        {
            // Yield if more than N seconds have passed.
            if (EditorApplication.timeSinceStartup - lastYieldTime < 0.1)
                return;

            if (OnStatusLog != null)
                OnStatusLog?.Invoke("Still working on it (duration " + (int)(EditorApplication.timeSinceStartup - startTime)  + " seconds so far).");

            lastYieldTime = EditorApplication.timeSinceStartup;
            await Task.Yield();
        }

        /// <summary>
        /// Depends on tmpSource and tmpTarget.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prop"></param>
        static void replaceGameObjectReferenceInProperty(Component parent, SerializedProperty prop)
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference)
                return;

            // Skip if the reference is not a GameObject
            if (!(prop.objectReferenceValue is GameObject))
                return;

            if (prop.objectReferenceValue != tmpSource.gameObject)
                return;

            // Skip source as don't want source to revert to target.
            if (parent.transform.IsChildOf(tmpSourceRoot.transform))
                return;

            // Found a matching GameObject
            prop.objectReferenceValue = tmpTarget.gameObject;
        }

        /// <summary>
        /// Depends on AllComponentsInScene.
        /// </summary>
        /// <param name="sourceComp"></param>
        /// <param name="targetComp"></param>
        static void replaceComponentReferences(Component sourceComp, Component targetComp)
        {
            tmpSourceComp = sourceComp;
            tmpTargetComp = targetComp;

            // Replace references to "sourceComp" with "targetComp" in ALL the components in the scene (that's a lot).
            HierarchyUtils.ForEveryComponentProperty(AllComponentsInScene, replaceComponentReferenceInProperty);
        }

        static void replaceComponentReferenceInProperty(Component parent, SerializedProperty prop)
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference)
                return;

            // Skip if the reference is not a component
            if (!(prop.objectReferenceValue is Component))
                return;

            // Skip source as don't want source to revert to target.
            if (parent.transform.IsChildOf(tmpSourceRoot.transform))
                return;

            if (prop.objectReferenceValue != tmpSourceComp)
                return;

            // Found a matching GameObject
            prop.objectReferenceValue = tmpTargetComp;
        }
    }
}