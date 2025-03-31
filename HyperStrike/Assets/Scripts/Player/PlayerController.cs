using UnityEngine;
using UnityEngine.InputSystem;
//using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    private Player player;
    private PlayerView view;

    //
    Rigidbody rb;

    //Movement Inputs
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction interactAction;
    InputAction crouchAction;
    InputAction sprintAction;
    InputAction previousAction;
    InputAction nextAction;

    private void Start()
    {
        player = new Player();
        view = GetComponent<PlayerView>();

        rb = GetComponent<Rigidbody>();

        InitInputs();
    }

    // Physics-based + Rigidbody Actions
    private void FixedUpdate()
    {
        
    }

    void InitInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        attackAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Look");
        crouchAction = InputSystem.actions.FindAction("Jump");
    }
}
