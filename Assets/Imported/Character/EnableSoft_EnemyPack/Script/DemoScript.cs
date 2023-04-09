
using UnityEngine;

namespace Enemy
{
    public class DemoScript : MonoBehaviour
    {

        public Transform target;
        public Animator ani;
        public float smoothSpeed = 15f;
        public Vector3 offset;
        [SerializeField]
        private bool _aniReady = true;

        private void OnGUI()
        {
            if (_aniReady)
            {
                if (GUI.Button(new Rect(50, 50, 100, 30), "Attack"))
                    SetTrigger("Attack");

                if (GUI.Button(new Rect(50, 90, 100, 30), "Die"))
                    SetTrigger("Die");

                if (GUI.Button(new Rect(50, 130, 100, 30), "Fear"))
                    SetTrigger("Fear");

                if (GUI.Button(new Rect(50, 170, 100, 30), "KnockDown"))
                    SetTrigger("KnockDown");

                if (GUI.Button(new Rect(50, 210, 100, 30), "Move"))
                    SetTrigger("Move");

                if (GUI.Button(new Rect(50, 250, 100, 30), "Reaction"))
                    SetTrigger("Reaction");

                if (GUI.Button(new Rect(50, 290, 100, 30), "Stun"))
                    SetTrigger("Stun");
            }
            else
            {
                if (ani.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    _aniReady = true;
                }
            }
        }

        void LateUpdate()
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

        void SetTrigger(string param)
        {
            _aniReady = false;
            ani.SetTrigger(param);
        }
    }
}