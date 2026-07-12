using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    // Stores the Rigidbody of the object currently being held.
    // If this is null, the player is not holding anything.
    private Rigidbody heldObject;

    // Stores how far away from the camera the object was when picked up.
    // This keeps the object at the same distance instead of snapping closer/farther away.
    private float holdDistance;

    // Stores the position offset between the camera and the point where the object was grabbed.
    // This allows us to grab objects from different points instead of always grabbing the center.
    private Vector3 holdOffset;

    // Stores the object's rotation relative to the camera when picked up.
    // This allows the object to keep its original rotation while being moved.
    private Quaternion holdRotation;

    // Stores the camera's previous position.
    // Used to calculate how fast the camera moved when dropping an object,
    // preventing objects from being launched when quickly moving the camera.
    private Vector3 lastCameraPosition;

    // Maximum distance the player can pick up objects from.
    private const float PickupRange = 3f;

    // Multiplier for how much camera movement affects the object's release velocity.
    private const float ReleaseMultiplier = 0.5f;

    // Maximum speed an object can be given when released.
    private const float MaxReleaseSpeed = 2f;

    // Strength of the throw when using right click.
    private const float ThrowForce = 3f;


    // Releases the currently held object.
    // Requires the camera transform because the object needs to be placed back
    // where it currently appears to be before physics takes over.
    private void DropObject(Transform cameraTransform)
    {
        // Safety check in case this function is called without an object being held.
        if (heldObject == null)
        {
            return;
        }

        // Calculates where the object should be positioned when dropped.
        // TransformPoint converts the camera-relative offset back into world space.
        Vector3 targetPosition = cameraTransform.TransformPoint(holdOffset);

        // Calculates the rotation the object should have when dropped.
        Quaternion targetRotation = cameraTransform.rotation * holdRotation;

        // Calculates a small release velocity based on how much the camera moved.
        // This stops objects from flying away if the player flicks the camera.
        Vector3 releaseVelocity = Vector3.ClampMagnitude(
            (cameraTransform.position - lastCameraPosition) / Time.fixedDeltaTime * ReleaseMultiplier,
            MaxReleaseSpeed
        );

        // Manually place the object at its current held position before enabling physics.
        heldObject.position = targetPosition;
        heldObject.rotation = targetRotation;

        // Turns physics back on so gravity and collisions affect the object again.
        heldObject.isKinematic = false;

        // Gives the object the calculated release velocity.
        heldObject.linearVelocity = releaseVelocity;

        // Removes any spinning when dropping.
        heldObject.angularVelocity = Vector3.zero;

        // Keeps Rigidbody interpolation enabled so the object transitions smoothly from being held to being affected by physics after it is dropped.
        heldObject.interpolation = RigidbodyInterpolation.Interpolate;

        // Clears the reference because the player is no longer holding anything.
        heldObject = null;
    }


    void Start()
    {
        
    }


    void Update()
    {
        // Checks if left mouse button was pressed this frame.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Left mouse button was pressed");

            // Makes sure there is a camera tagged MainCamera in the scene.
            if (Camera.main == null)
            {
                return;
            }

            // Stores the camera transform so we don't have to repeatedly call Camera.main.
            Transform cameraTransform = Camera.main.transform;


            // If the player is already holding something,
            // left click will drop it instead of trying to pick something else up.
            if (heldObject != null)
            {
                DropObject(cameraTransform);
                Debug.Log("Object dropped");
                return;
            }
            else
            {
                // Creates a ray starting from the camera position
                // and pointing in the direction the player is looking.
                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);


                // Stores information about whatever the ray hits.
                // Includes the object hit, hit position, distance, and surface normal.
                RaycastHit hit;


                // Sends the ray forward.
                // Ignores objects on the Player layer.
                if (Physics.Raycast(ray, out hit, PickupRange, ~LayerMask.GetMask("Player")))
                {
                    // Checks if the object hit has the Pickupable tag.
                    if (hit.collider.CompareTag("Pickupable"))
                    {
                        Debug.Log("Pickupable object hit");


                        // Gets the Rigidbody from the object we hit.
                        heldObject = hit.collider.GetComponent<Rigidbody>();

                        // If the object has no Rigidbody, stop here.
                        if (heldObject == null)
                        {
                            return;
                        }


                        // Stores where on the object we grabbed it from relative to the camera.
                        // This lets us grab corners/edges instead of always grabbing the center.
                        holdOffset = cameraTransform.InverseTransformPoint(hit.point);


                        // Stores the object's rotation relative to the camera.
                        // This keeps the object's original rotation while holding it.
                        holdRotation = Quaternion.Inverse(cameraTransform.rotation) * heldObject.rotation;


                        // Stops physics from affecting the object while the player is holding it.
                        heldObject.isKinematic = true;


                        // Smooths Rigidbody movement while being held.
                        heldObject.interpolation = RigidbodyInterpolation.Interpolate;


                        // Saves the camera position at the moment of pickup.
                        lastCameraPosition = cameraTransform.position;


                        // Saves the distance between the camera and object.
                        holdDistance = hit.distance;
                    }
                }
            }
        }


        // Checks if right mouse button was pressed.
        // Right click throws the object if the player is holding one.
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("Right mouse button was pressed");


            if (Camera.main == null)
            {
                return;
            }


            Transform cameraTransform = Camera.main.transform;


            // Only throw if an object is currently being held.
            if (heldObject != null)
            {
                ThrowObject(cameraTransform.forward);
                Debug.Log("Object thrown");
            }
        }
    }


    // Runs on the physics update loop.
    // Used for moving Rigidbody objects because physics calculations happen here.
    private void FixedUpdate()
    {
        // Stop if there is no object being held or no camera exists.
        if (heldObject == null || Camera.main == null)
        {
            return;
        }


        Transform cameraTransform = Camera.main.transform;


        // Converts the stored camera-relative position back into world space.
        Vector3 targetPosition = cameraTransform.TransformPoint(holdOffset);


        // Calculates the object's target rotation while keeping the original orientation.
        Quaternion targetRotation = cameraTransform.rotation * holdRotation;


        // Moves the Rigidbody to follow the camera.
        heldObject.MovePosition(targetPosition);

        // Rotates the Rigidbody to follow the camera.
        heldObject.MoveRotation(targetRotation);


        // Updates the last camera position for release velocity calculations.
        lastCameraPosition = cameraTransform.position;
    }


    // Throws the held object forward.
    // throwDirection is the direction the camera is facing.
    private void ThrowObject(Vector3 throwDirection)
    {
        // Gives physics control back to the object.
        heldObject.isKinematic = false;


        // Applies velocity in the direction the player is looking.
        heldObject.linearVelocity = throwDirection * ThrowForce;


        // Stops the object from randomly spinning when thrown.
        heldObject.angularVelocity = Vector3.zero;


        // Keeps movement smooth after throwing.
        heldObject.interpolation = RigidbodyInterpolation.Interpolate;


        // Clears the held object because the player is no longer holding it.
        heldObject = null;
    }
}