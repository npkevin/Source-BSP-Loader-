using UnityEngine;

namespace NPKEVIN.Utils
{
    class env_sprite : MonoBehaviour
    {
        public static Transform cameraTransform { get; private set; } = null;
        public VALVE.Entity entity;


        void Start()
        {
            if (cameraTransform == null)
            {
                cameraTransform = GameObject.Find("PlayerController").GetComponent<CPMPlayer>().playerView;
            }
        }

        void Update()
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
            // flip vertically (textutures are imported upside-down)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + 180);
        }
    }
}