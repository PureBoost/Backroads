using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    private Rigidbody heldObject;

    private void DropObject(){
    heldObject.isKinematic = false;
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

                // Drops heldObject if it is not null, otherwise it will continue to try to pick up an object.
                if (heldObject != null){
                    DropObject();
                    return;
                } else {
                
                // transform.position = where the ray starts.
                // transform.forward = the direction of the ray.
                // Added Camera.main before the method because we want it to be from the player's camera, and this works beacuse the tag on the players camera is MainCamera but the    actuial camera can be called whatever we want.
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

                // Stores information about what the raycast hit. ex: Distance, What it hit, Hit position, Normal of the hit surface. (A normal is simply a vector that points  straight out from a surface at the point you hit it. so basically the normal is the direction the surface is facing at the exact point where the ray hit it.)
                RaycastHit hit;

                // Casts the ray and checks if it hit something within 10 units. (1 unit = 1 meter)
                // The ~LayerMask.GetMask("Player") is used to ignore the player layer when performing the raycast. This means that the raycast will not detect any objects that are    on the "Player" layer, allowing the player to pick up objects without accidentally hitting themselves.
                if (Physics.Raycast(ray, out hit, 10f, ~LayerMask.GetMask("Player"))){

                    if (hit.collider.CompareTag("Pickupable")){
                        Debug.Log("Pickupable object hit");

                        heldObject = hit.collider.GetComponent<Rigidbody>();

                        if (heldObject != null){
                            heldObject.isKinematic = true;
                        }

                    }

                }
            }
        }
        // If we are holding an object, move it in front of the player
        // this has to be outside of the if mouseclick statement or else the object will only move when the mouse is clicked, and we want it to still move after the click.
            if (heldObject != null)
            {
                 Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;

                 heldObject.MovePosition(targetPosition);
            }
    }
}
