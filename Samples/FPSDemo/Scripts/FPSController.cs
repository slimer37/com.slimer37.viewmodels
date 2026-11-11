using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private float _sensitivity = 1f;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _jumpHeight = 0.5f;
    [SerializeField] private float _clampAngle = 80f;
    [SerializeField] private float _gravity = 9.81f;
    [SerializeField] private CharacterController _controller;

    [Header("Input")]
    [SerializeField] private InputAction _moveAction;
    [SerializeField] private InputAction _lookAction;
    [SerializeField] private InputAction _jumpAction;

    private Vector3 _velocity;
    private Vector3 _eulerAngles;

    private void Reset()
    {
        _moveAction = new InputAction("Move");
        _lookAction = new InputAction("Look");
        _jumpAction = new InputAction("Jump");

        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        _moveAction.ChangeBinding("2DVector").WithName("WASD");

        _lookAction.AddBinding("<Mouse>/delta");

        _jumpAction.AddBinding("<Keyboard>/space");
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();

        _eulerAngles = _camera.localEulerAngles;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();

        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDestroy()
    {
        _moveAction.Dispose();
        _lookAction.Dispose();
        _jumpAction.Dispose();
    }

    private static float GetInitialJumpVelocity(float maxHeight, float gravity) =>
        Mathf.Sqrt(2f * maxHeight * gravity);

    private void Update()
    {
        if (_controller.isGrounded)
        {
            _velocity.y = -1f;

            if (_jumpAction.WasPerformedThisFrame())
            {
                _velocity.y = GetInitialJumpVelocity(_jumpHeight, _gravity);
            }
        }

        _velocity.y -= _gravity * Time.deltaTime;

        Vector2 moveInput = _moveAction.ReadValue<Vector2>().normalized * _speed;

        _velocity.x = moveInput.x;
        _velocity.z = moveInput.y;

        _velocity = Quaternion.Euler(0f, _eulerAngles.y, 0f) * _velocity;

        _controller.Move(_velocity * Time.deltaTime);

        Vector2 lookInput = _lookAction.ReadValue<Vector2>() * _sensitivity;

        _eulerAngles.x -= lookInput.y;
        _eulerAngles.y += lookInput.x;

        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -_clampAngle, _clampAngle);

        _camera.localEulerAngles = _eulerAngles;
    }
}