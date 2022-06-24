using UnityEngine;

namespace NPKEVIN.Utils
{
    public interface IInteractable
    {
        public void interact();
    }

    public abstract class button : MonoBehaviour, IInteractable
    {
        public string targetName = "";
        public abstract void interact();
    }

    public abstract class door : MonoBehaviour, IInteractable
    {
        public string targetName = "";
        public abstract void interact();
    }






    public class func_rotating : MonoBehaviour
    {
        public float maxSpeed = 100;

        public void Update()
        {
            transform.eulerAngles = new Vector3(0, Time.deltaTime * maxSpeed, 0);
        }
    }

    public class func_door : door
    {
        public override void interact()
        {
        }
    }

    public class func_door_rotating : door
    {
        public string linkedDoor = "";
        public float rotationSpeed = 150.0f;
        public float openAngle = 90.0f;
        public float closedAngle = 0.0f;

        private bool opening = false;
        private float rotationAngle = 0.0f;

        private float prevRotationAngle = 0.0f;

        public void Update()
        {
            prevRotationAngle = rotationAngle;

            // Add rotation to the door
            if (opening && rotationAngle < openAngle)
                rotationAngle += rotationSpeed * Time.deltaTime;
            else if (!opening && rotationAngle > closedAngle)
                rotationAngle -= rotationSpeed * Time.deltaTime;

            // Animation finished
            if (rotationAngle > openAngle || rotationAngle < closedAngle)
            {
                rotationAngle = Mathf.Clamp(rotationAngle, closedAngle, openAngle);
                animationFinished();
            }

            if (prevRotationAngle != rotationAngle)
                transform.eulerAngles = new Vector3(0, rotationAngle, 0);
        }

        public override void interact()
        {
            toggleDoor();
            if (linkedDoor != "")
            {
                // door names are unique to eachother for every map
                func_door_rotating otherDoor = transform.parent.Find(linkedDoor).GetComponent<func_door_rotating>();
                otherDoor.toggleDoor();
            }
        }

        private void animationFinished()
        {
            if (rotationAngle == closedAngle)
            {
                // Usefull functions here
            }
            else if (rotationAngle == openAngle)
            {
                // Usefull functions here
            }

        }

        private void toggleDoor() => opening = !opening;
    }

}