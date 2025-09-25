using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PrisonController
{
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
}
