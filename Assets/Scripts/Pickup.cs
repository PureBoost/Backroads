using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    private Rigidbody heldObject;
    private float holdDistance;
    private Vector3 holdOffset;
    private Quaternion holdRotation;
    private Vector3 lastCameraPosition;
    private const float PickupRange = 3f;
    private const float ReleaseMultiplier = 0.5f;
    private const float MaxReleaseSpeed = 2f;
    private const float ThrowForce = 3f;

    private void DropObject(Transform cameraTransform){
        if (heldObject == null){
            return;
        }

        // Stop the object from being thrown away when flinging camera. This is done by calculating the release velocity based on the camera's movement since the last frame, and clamping it to a maximum speed.
        Vector3 targetPosition = cameraTransform.TransformPoint(holdOffset);
        Quaternion targetRotation = cameraTransform.rotation * holdRotation;
        Vector3 releaseVelocity = Vector3.ClampMagnitude((cameraTransform.position - lastCameraPosition) / Time.fixedDeltaTime * ReleaseMultiplier, MaxReleaseSpeed);

        heldObject.position = targetPosition;
        heldObject.rotation = targetRotation;
        heldObject.isKinematic = false;
        heldObject.linearVelocity = releaseVelocity;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.interpolation = RigidbodyInterpolation.None;
        heldObject = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        
        //Checks if the left mouse button was pressed this frame.
        if (Mouse.current.leftButton.wasPressedThisFrame){
            Debug.Log("Left mouse button was pressed");

            if (Camera.main == null){
                return;
            }

            Transform cameraTransform = Camera.main.transform;

                // Drops heldObject if it is not null, otherwise it will continue to try to pick up an object.
                if (heldObject != null){
                    DropObject(cameraTransform);
                    Debug.Log("Object dropped");
                    return;
                } else {
                
                // transform.position = where the ray starts.
                // transform.forward = the direction of the ray.
                // Added Camera.main before the method because we want it to be from the player's camera, and this works beacuse the tag on the players camera is MainCamera but the    actuial camera can be called whatever we want.
                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

                // Stores information about what the raycast hit. ex: Distance, What it hit, Hit position, Normal of the hit surface. (A normal is simply a vector that points  straight out from a surface at the point you hit it. so basically the normal is the direction the surface is facing at the exact point where the ray hit it.)
                RaycastHit hit;

                // Casts the ray and checks if it hit something within 3 units. (1 unit = 1 meter)
                // The ~LayerMask.GetMask("Player") is used to ignore the player layer when performing the raycast. This means that the raycast will not detect any objects that are    on the "Player" layer, allowing the player to pick up objects without accidentally hitting themselves.
                if (Physics.Raycast(ray, out hit, PickupRange, ~LayerMask.GetMask("Player"))){

                    if (hit.collider.CompareTag("Pickupable")){
                        Debug.Log("Pickupable object hit");

                        heldObject = hit.collider.GetComponent<Rigidbody>();
                        if (heldObject == null){
                            return;
                        }

                        holdOffset = cameraTransform.InverseTransformPoint(hit.point);
                        holdRotation = Quaternion.Inverse(cameraTransform.rotation) * heldObject.rotation;

                        heldObject.isKinematic = true;
                        heldObject.interpolation = RigidbodyInterpolation.Interpolate;
                        lastCameraPosition = cameraTransform.position;
                        // Store the distance from the camera to the object when it is picked up
                        holdDistance = hit.distance;

                    }
                }

                
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame){
            Debug.Log("Right mouse button was pressed");

            if (Camera.main == null){
            return;
            }

            Transform cameraTransform = Camera.main.transform;

            if (heldObject != null){
            ThrowObject(cameraTransform.forward);
            Debug.Log("Object thrown");
            }
        }
    }

    private void FixedUpdate(){
        if (heldObject == null || Camera.main == null){
            return;
        }

        Transform cameraTransform = Camera.main.transform;

        Vector3 targetPosition = cameraTransform.TransformPoint(holdOffset);
        Quaternion targetRotation = cameraTransform.rotation * holdRotation;

        heldObject.MovePosition(targetPosition);
        heldObject.MoveRotation(targetRotation);
        lastCameraPosition = cameraTransform.position;
    }

    private void ThrowObject(Vector3 throwDirection){

        heldObject.isKinematic = false;
        heldObject.linearVelocity = throwDirection * ThrowForce;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.interpolation = RigidbodyInterpolation.Interpolate;

        heldObject = null;
        
    }
}
