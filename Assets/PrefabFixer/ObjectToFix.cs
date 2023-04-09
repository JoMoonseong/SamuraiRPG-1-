using System.Text.RegularExpressions;
using UnityEngine;

namespace Kamgam.PF
{
    public class ObjectToFix
    {
        static Regex regEx = new Regex(@"\(Missing .* guid: [a-z0-9]{30,60}\)");

        protected GameObject gameObject;
        public GameObject GameObject
        {
            get => gameObject;
            set
            {
                if (gameObject == value)
                    return;

                gameObject = value;
                updateSortIndex(gameObject);
            }
        }

        public GameObject Prefab;
        public int SortIndex;

        public ObjectToFix(GameObject gameObject, GameObject prefab)
        {
            GameObject = gameObject;
            Prefab = prefab;
            updateSortIndex(gameObject);
        }

        private void updateSortIndex(GameObject gameObject)
        {
            if (gameObject == null)
                SortIndex = 0;

            SortIndex = 0;
            Transform t = gameObject.transform.parent;
            while (t != null)
            {
                SortIndex++;
                t = t.parent;
            }
            SortIndex += gameObject.transform.GetSiblingIndex();
        }

        public static int CompareAsc(ObjectToFix a, ObjectToFix b)
        {
            return a.SortIndex - b.SortIndex;
        }
    }
}