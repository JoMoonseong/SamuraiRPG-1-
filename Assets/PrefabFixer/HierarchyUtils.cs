using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    public static class HierarchyUtils
    {
        /// <summary>
        /// Copies "source" into "parentOfCopy" and a reference to the copied game object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parentOfCopy"></param>
        /// <returns></returns>
        public static GameObject CopyHierarchyTo(GameObject source, Transform parentOfCopy)
        {
            // Sadly this does UNPACK all Prefabs in the hierarchy, thus it's only of limited use.
            // To fix this we would have to iterate through the whole hierarchy after instantiation
            // and use PrefabUtility.InstantiatePrefab() followed by PrefabUtility.GetPropertyModifications()
            // AND we would still have to copy over the objects which were not part of the prefab.
            // For now we use the hacky way.
            // See: https://forum.unity.com/threads/duplicating-a-hierarchy-of-gameobjects-containing-prefabs-without-losing-prefab-links.296108/
            /*
            var copy = GameObject.Instantiate(source);
            copy.transform.SetParent(parentOfCopy);
            copy.name = source.name;
            return copy;
            */

            // The hacky solution
            Object recordSelected = Selection.activeObject;
            Selection.activeObject = source;
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            Selection.activeObject = recordSelected;

            // Fetching the copy relies on the the fact that DuplicateGameObjectsUsingPasteboard()
            // seems to reliably paste the object at the END of the parent transform.
            GameObject target;
            if (source.transform.parent != null)
            {
                target = source.transform.parent.GetChild(source.transform.parent.childCount - 1).gameObject;
            }
            else
            {
                var roots = source.scene.GetRootGameObjects();
                target = roots[source.scene.rootCount - 1];
            }
            target.name = source.name;
            target.transform.SetParent(parentOfCopy);
            return target;
        }

        public static void ForEveryComponentProperty(List<Component> components, System.Action<Component, SerializedProperty> func)
        {
            if (func == null)
                return;

            foreach (Component sceneComp in components)
            {
                if (sceneComp == null)
                    continue;

                SerializedObject serObj = new SerializedObject(sceneComp);
                SerializedProperty prop = serObj.GetIterator();
                while (prop.NextVisible(true))
                {
                    func(sceneComp, prop);
                }
                serObj.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Iterates over two hierarchies recursively and in parallel.<br />
        /// If the childCount in a and b differ then one of the two
        /// parameters will be null in the call to "func(a,b)".<br /><br />
        /// 
        /// Recurses into child objects.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="func">The function that will be called on the parallel transforms. A or B may be null.</param>
        public static void WalkHierarchiesInParallel(Transform a, Transform b, System.Action<Transform, Transform> func)
        {
            func(a, b);

            int max = Mathf.Max(
                a != null ? a.childCount : 0,
                b != null ? b.childCount : 0
                );
            Transform newA, newB;
            for (int i = 0; i < max; i++)
            {
                if (a != null && i < a.childCount)
                    newA = a.GetChild(i);
                else
                    newA = null;

                if (b != null && i < b.childCount)
                    newB = b.GetChild(i);
                else
                    newB = null;

                WalkHierarchiesInParallel(newA, newB, func);
            }
        }

        /// <summary>
        /// Iterates over two hierarchies recursively and in parallel.<br />
        /// Every object in A is compared (by name) to every object in B.
        /// If no match for A is found then func(null, B) will be called.<br />
        /// If no match for B is found then func(A, null) will be called.<br />
        /// <br />
        /// Recurses into child objects.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="func">The function that will be called on the parallel transforms. A or B may be null.</param>
        public static async Task WalkHierarchiesInParallelByNameAsync(
            Transform a, Transform b,
            System.Func<Transform, Transform, CancellationToken, Task> func,
            CancellationToken ct)
        {
            if (a == null && b == null)
            {
                return;
            }

            await func(a, b, ct);

            // a is empty
            bool aIsEmpty = a == null || a.childCount == 0;
            if (aIsEmpty)
            {
                for (int i = 0; i < b.childCount; i++)
                {
                    await func(null, b.GetChild(i), ct);
                }
            }

            // b is empty
            bool bIsEmpty = b == null || b.childCount == 0;
            if (bIsEmpty)
            {
                for (int i = 0; i < b.childCount; i++)
                {
                    await func(a.GetChild(i), null, ct);
                }
            }
            
            List<int> handledIndizesInB = new List<int>(b.childCount);

            // Go through A and find matches by name in B.
            if (!aIsEmpty)
            {
                bool foundMatchForA;
                for (int ai = 0; ai < a.childCount; ai++)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    foundMatchForA = false;
                    for (int bi = 0; bi < b.childCount; bi++)
                    {
                        if (handledIndizesInB.Contains(bi))
                            continue;

                        if (a.GetChild(ai).name == b.GetChild(bi).name)
                        {
                            handledIndizesInB.Add(bi);
                            foundMatchForA = true;
                            await WalkHierarchiesInParallelByNameAsync(a.GetChild(ai), b.GetChild(bi), func, ct);
                        }
                    }

                    if (!foundMatchForA)
                        await WalkHierarchiesInParallelByNameAsync(a.GetChild(ai), null, func, ct);
                }
            }

            // Handle all unmatched objects in b.
            if (!bIsEmpty)
            {
                for (int bi = 0; bi < b.childCount; bi++)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    if (!handledIndizesInB.Contains(bi))
                    {
                        await WalkHierarchiesInParallelByNameAsync(null, b.GetChild(bi), func, ct);
                    }
                }
            }
        }

        /// <summary>
        /// Iterates over the components of two objects in parallel.<br />
        /// If the number of components in a and b differ then one of the two
        /// parameters will be null in the call to "func(a,b)".<br />
        /// <br />
        /// It matches the components by type. Therefore func() will always
        /// get components of the same type.
        /// 
        /// This only goes through the given transform (not the children).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="func">The function that will be called on the parallel components. A or B may be null.</param>
        public static void IterateOverComponentsInParallel(Transform a, Transform b, System.Action<Component, Component> func)
        {
            List<Component> handledComponents = new List<Component>();

            // Shortcut if a is null
            if (a == null && b != null)
            {
                var comps = b.GetComponents<Component>();
                foreach (var comp in comps)
                {
                    func(null, comp);
                }
            }

            // Shortcut if b is null
            if (a != null && b == null)
            {
                var comps = a.GetComponents<Component>();
                foreach (var comp in comps)
                {
                    func(comp, null);
                }
            }

            var aComps = a.GetComponents<Component>();
            var bComps = b.GetComponents<Component>();

            // a to b
            foreach (var comp in aComps)
            {
                Component matchedComp = null;
                foreach (var bComp in bComps)
                {
                    if (handledComponents.Contains(bComp))
                        continue;

                    if (bComp.GetType() != comp.GetType())
                        continue;

                    matchedComp = bComp;
                    break;
                }
                
                handledComponents.Add(comp);
                handledComponents.Add(matchedComp);

                func(comp, matchedComp);
            }

            // b to a
            foreach (var comp in bComps)
            {
                if (handledComponents.Contains(comp))
                    continue;

                Component matchedComp = null;
                foreach (var aComp in aComps)
                {
                    if (handledComponents.Contains(aComp))
                        continue;

                    if (aComp.GetType() != comp.GetType())
                        continue;

                    matchedComp = aComp;
                    break;
                }

                handledComponents.Add(comp);
                handledComponents.Add(matchedComp);

                func(comp, matchedComp);
            }
        }

        /// <summary>
        /// Returns the transform path with name and sibling index.<br />
        /// 
        /// The separator is the TAB character as that one is unlikely
        /// to be used in a GameObject name.<br />
        /// 
        /// The sibling index is added int square brackets after the name
        /// and before the TAB resulting in a sequence like this: "[0]\t".<br />
        /// 
        /// Example:
        ///   Char/LeftArm/LeftHand/Finger (assume there are multiple "Finger"s)<br />
        ///   becomes<br />
        ///   Char[0]\tLeftArm[0]\tLeftHand\tFinger[2]<br />
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="rootToStopAt">The root to stop at. It's NULL by default meaning it stops at the scene.</param>
        /// <param name="addRoot">Add the root to the path (null roots will never be added)</param>
        /// <param name="separator">Separation character. TAB (\t) by default</param>
        /// <returns></returns>
        public static string GetPathAsString(Transform transform, Transform rootToStopAt = null, bool addRoot = true, char separator = '\t')
        {
            if (transform == null)
                return null;

            var stringBuilder = new System.Text.StringBuilder();

            stringBuilder.Append(transform.name);
            stringBuilder.Append("[");
            stringBuilder.Append(transform.GetSiblingIndex());
            stringBuilder.Append("]");

            while (transform != rootToStopAt)
            {
                transform = transform.parent;
                if (transform != null)
                {
                    if (!addRoot && rootToStopAt != null && transform == rootToStopAt)
                        continue;

                    // get sibling index
                    stringBuilder.Insert(0, separator); // TAB is very unlikely to be used in a game object name
                    stringBuilder.Insert(0, "]");
                    stringBuilder.Insert(0, transform.GetSiblingIndex());
                    stringBuilder.Insert(0, "[");
                    stringBuilder.Insert(0,transform.name);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the component path with game object name and sibling index
        /// and component name + type + index-of-components-with-the-same-type.<br /><br />
        /// 
        /// The separator is the TAB character as that one is unlikely
        /// to be used in a Component name.<br /><br />
        /// 
        /// The game object path part is generated via a call to GetPathAsString()
        /// on the game object.<br /><br />
        ///
        /// For each component three things are saved:<br />
        /// 1. The name<br />
        /// 2. The fully qualified type ("UnityEngine.UI.Image" for example)<br />
        /// 3. The index which is defined as the number of preceeding components<br />
        ///    with the same type on the same game object (usually 0).<br /><br />
        ///    
        /// TABs (\t) and square brackets([]) are used to separate the data <br />
        /// resulting in a sequence like this: "Image\t[UnityEngine.UI.Image][3]\t".<br /><br />
        /// 
        /// Example:<br />
        ///   Char/LeftArm/LeftHand/Finger/Image (assume there are multiple "Finger"s and "Image"s)<br />
        ///   becomes<br />
        ///   Char[0]\tLeftArm[0]\tLeftHand\tFinger[2]\tImage[UnityEngine.UI.Image][1]<br />
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="rootToStopAt">The root to stop at. It's NULL by default meaning it stops at the scene.</param>
        /// <param name="addRoot">Add the root to the path (null roots will never be added)</param>
        /// <param name="separator">Separation character. TAB (\t) by default</param>
        /// <returns></returns>
        public static string GetPathAsString(Component component, Transform rootToStopAt = null, bool addRoot = true, char separator = '\t')
        {
            if (component == null)
                return null;

            var stringBuilder = new System.Text.StringBuilder();
            var components = component.transform.GetComponents(component.GetType());
            // loop until we find the component (i = the index)
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == component)
                {
                    stringBuilder.Append(separator); // TAB is very unlikely to be used in a component name
                    stringBuilder.Append("[");
                    stringBuilder.Append(component.GetType().FullName);
                    stringBuilder.Append("]");
                    stringBuilder.Append("[");
                    stringBuilder.Append(i);
                    stringBuilder.Append("]");
                    break;
                }
            }
            string transformPath = GetPathAsString(component.transform, rootToStopAt, addRoot, separator);
            stringBuilder.Insert(0, transformPath);
            return stringBuilder.ToString();
        }
    }
}