using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        public float MoveSpeed = 4f;
        public float SprintSpeed = 6f;
        public float RotationSpeed = 1f;
        public float JumpHeight = 1.2f;
        public float Gravity = -15f;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;
        public float MaxLookAngle = 90f;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private float _verticalVelocity;
        private bool _grounded;
        private float _pitch;
        private const float _threshold = 0.01f;

        private bool IsMouse =>
#if ENABLE_INPUT_SYSTEM
            _playerInput.currentControlScheme == "KeyboardMouse";
#else
			false;
#endif

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
            _pitch = transform.eulerAngles.x;
        }

        private void Update()
        {
            if (Time.timeScale > 0)
            {
                GroundedCheck();
                Move();
                JumpAndGravity();
                Look();
            }
        }

        private void GroundedCheck()
        {
            Vector3 pos = transform.position + Vector3.up * GroundedOffset;
            _grounded = Physics.CheckSphere(pos, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void Move()
        {
            float speed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) speed = 0f;
            Vector3 dir = transform.right * _input.move.x + transform.forward * _input.move.y;
            _controller.Move(dir.normalized * speed * Time.deltaTime + Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (_grounded)
            {
                if (_verticalVelocity < 0f) _verticalVelocity = -2f;
                if (_input.jump) _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }
            else _input.jump = false;

            if (_verticalVelocity < 53f) _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void Look()
        {
            if (_input.look.sqrMagnitude < _threshold) return;

            float delta = IsMouse ? 1f : Time.deltaTime;

            _pitch += _input.look.y * RotationSpeed * delta;
            _pitch = Mathf.Clamp(_pitch, -MaxLookAngle, MaxLookAngle);

            transform.Rotate(Vector3.up * _input.look.x * RotationSpeed * delta);

            Camera.main.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _grounded ? new Color(0, 1, 0, 0.35f) : new Color(1, 0, 0, 0.35f);
            Gizmos.DrawSphere(transform.position - Vector3.up * GroundedOffset, GroundedRadius);
        }
    }
}