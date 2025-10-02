using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControler : MonoBehaviour
{
    public InputActionReference MoveAction;
    //public InputActionReference ShootAction;

    private Vector3 direction = Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * Time.deltaTime * 2f;
    }

    void OnEnable()
    {
        MoveAction.action.performed += OnMoveActionPerformed;
        MoveAction.action.canceled += OnMoveActionCanceled;
        MoveAction.action.Enable();

        //ShootAction.action.started += OnShootActionStarted;
        //ShootAction.action.Enable();
    }
    
    void OnDisable()
    {
        MoveAction.action.performed -= OnMoveActionPerformed;
        MoveAction.action.canceled -= OnMoveActionCanceled;
        MoveAction.action.Disable();

        //ShootAction.action.started -= OnShootActionStarted;
        //ShootAction.action.Disable();
    }

    private void OnMoveActionPerformed(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector3>();
    }

    private void OnMoveActionCanceled(InputAction.CallbackContext context)
    {
        direction = Vector3.zero;
    }

    //private void OnShootActionStarted(InputAction.CallbackContext context)
    //{
        //throw new NotImplementedException();
    //}
}
