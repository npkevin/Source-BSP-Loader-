using UnityEngine;

namespace NPKEVIN.Utils
{
    public class SkyCamera : MonoBehaviour
    {
        public float Scale = 16.0f;

        public Camera Camera { get; set; }
        public Vector3 Origin { get; set; }

        private Transform player;

        void Start()
        {
            Origin = gameObject.transform.position;
            Camera = gameObject.AddComponent<Camera>();

            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Color.black;
            // SkyCamera.clearFlags = CameraClearFlags.Skybox;
            Camera.nearClipPlane = 0.001f;
            Camera.depth = -1;
        }

        void LateUpdate()
        {
            if (player == null)
                player = GameObject.Find("PlayerController").GetComponent<CPMPlayer>().playerView;
            else
                SetTransform(player);
        }

        public void SetTransform(Transform tf)
        {
            transform.rotation = tf.rotation;
            transform.position = Origin + (tf.position / Scale);
        }
    }
}

