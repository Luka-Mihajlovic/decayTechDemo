using Unity.VisualScripting;
using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly move their rig continuously over time.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public abstract class forkedContinuousMoveProviderBase : LocomotionProvider
    {
        /// <summary>
        /// Defines when gravity begins to take effect.
        /// </summary>
        /// <seealso cref="gravityApplicationMode"/>
        public enum GravityApplicationMode
        {
            /// <summary>
            /// Only begin to apply gravity and apply locomotion when a move input occurs.
            /// When using gravity, continues applying each frame, even if input is stopped, until touching ground.
            /// </summary>
            /// <remarks>
            /// Use this style when you don't want gravity to apply when the player physically walks away and off a ground surface.
            /// Gravity will only begin to move the player back down to the ground when they try to use input to move.
            /// </remarks>
            AttemptingMove,

            /// <summary>
            /// Apply gravity and apply locomotion every frame, even without move input.
            /// </summary>
            /// <remarks>
            /// Use this style when you want gravity to apply when the player physically walks away and off a ground surface,
            /// even when there is no input to move.
            /// </remarks>
            Immediately,
        }

        [SerializeField]
        [Tooltip("The speed, in units per second, to move forward.")]
        float m_MoveSpeed = 1f;
        /// <summary>
        /// The speed, in units per second, to move forward.
        /// </summary>
        public float moveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }


        [SerializeField]
        [Tooltip("Controls whether to enable strafing (sideways movement).")]
        bool m_EnableStrafe = true;
        /// <summary>
        /// Controls whether to enable strafing (sideways movement).
        /// </summary>
        public bool enableStrafe
        {
            get => m_EnableStrafe;
            set => m_EnableStrafe = value;
        }

        public bool isStrafing;

        [SerializeField]
        [Tooltip("The source Transform to define the forward direction.")]
        Transform m_ForwardSource;
        /// <summary>
        /// The source <see cref="Transform"/> that defines the forward direction.
        /// </summary>
        public Transform forwardSource
        {
            get => m_ForwardSource;
            set => m_ForwardSource = value;
        }

        CharacterController m_CharacterController;

        bool m_AttemptedGetCharacterController;

        bool m_IsMovingXROrigin;

        Vector3 m_VerticalVelocity;

        public AudioSource stepSound;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            m_IsMovingXROrigin = false;

            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            var input = ReadInput();
            var translationInWorldSpace = ComputeDesiredMove(input);

            if (xrOrigin.GetComponent<controllerAnalysis>().isSprinting) //if youre sprinting, you cant strafe
            {
                enableStrafe = false;
            }
            else
            {
                enableStrafe = true;
            }

            if (enableStrafe)
            {
                isStrafing = ((input.y < 0.8 && input.y != 0) ? true : false); //if the input isn't going forwards and they're walking, isStrafe = true
            }
            else
            {
                isStrafing = ((input.y < -0.4) ? true : false);
            }

            if (translationInWorldSpace != new Vector3(0, 0, 0))
            {
                if (isStrafing) //strafe/backwards slowdown
                {
                    translationInWorldSpace *= 0.2f;
                }

                var stepRange = (Random.Range(0.25f, 1.5f) * translationInWorldSpace.magnitude);
                stepSound.pitch = Mathf.Clamp((stepRange * 15), 0.5f, 0.7f);
                stepSound.volume = Mathf.Clamp((stepRange * 30), 0f, 1f);

                stepSound.enabled = true;
                MoveRig(translationInWorldSpace);
            }
            else
            {
                stepSound.enabled = false;
            }
            

            switch (locomotionPhase)
            {
                case LocomotionPhase.Idle:
                case LocomotionPhase.Started:
                    if (m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Moving;
                    break;
                case LocomotionPhase.Moving:
                    if (!m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Done;
                    break;
                case LocomotionPhase.Done:
                    locomotionPhase = m_IsMovingXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={locomotionPhase}");
                    break;
            }
        }

        /// <summary>
        /// Reads the current value of the move input.
        /// </summary>
        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
        protected abstract Vector2 ReadInput();

        /// <summary>
        /// Determines how much to slide the rig due to <paramref name="input"/> vector.
        /// </summary>
        /// <param name="input">Input vector, such as from a thumbstick.</param>
        /// <returns>Returns the translation amount in world space to move the rig.</returns>
        protected virtual Vector3 ComputeDesiredMove(Vector2 input)
        {
            if (input == Vector2.zero)
                return Vector3.zero;

            var xrOrigin = system.xrOrigin;
            if (xrOrigin == null)
                return Vector3.zero;

            // Assumes that the input axes are in the range [-1, 1].
            // Clamps the magnitude of the input direction to prevent faster speed when moving diagonally,
            // while still allowing for analog input to move slower (which would be lost if simply normalizing).
            var inputMove = Vector3.ClampMagnitude(new Vector3(m_EnableStrafe ? input.x : 0f, 0f, input.y), 1f);

            // Determine frame of reference for what the input direction is relative to
            var forwardSourceTransform = m_ForwardSource == null ? xrOrigin.Camera.transform : m_ForwardSource;
            var inputForwardInWorldSpace = forwardSourceTransform.forward;

            var originTransform = xrOrigin.Origin.transform;
            var speedFactor = m_MoveSpeed * Time.deltaTime * originTransform.localScale.x; // Adjust speed with user scale

            var originUp = originTransform.up;

            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, originUp)), 1f))
            {
                // When the input forward direction is parallel with the rig normal,
                // it will probably feel better for the player to move along the same direction
                // as if they tilted forward or up some rather than moving in the rig forward direction.
                // It also will probably be a better experience to at least move in a direction
                // rather than stopping if the head/controller is oriented such that it is perpendicular with the rig.
                inputForwardInWorldSpace = -forwardSourceTransform.up;
            }

            var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, originUp);
            var forwardRotation = Quaternion.FromToRotation(originTransform.forward, inputForwardProjectedInWorldSpace);

            var translationInRigSpace = forwardRotation * inputMove * speedFactor;
            var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);

            return translationInWorldSpace;
        }

        /// <summary>
        /// Creates a locomotion event to move the rig by <paramref name="translationInWorldSpace"/>,
        /// and optionally applies gravity.
        /// </summary>
        /// <param name="translationInWorldSpace">The translation amount in world space to move the rig (pre-gravity).</param>
        protected virtual void MoveRig(Vector3 translationInWorldSpace)
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            FindCharacterController();

            var motion = translationInWorldSpace;

            if (m_CharacterController != null && m_CharacterController.enabled)
            {
                // Step vertical velocity from gravity
                if (m_CharacterController.isGrounded)
                {
                    m_VerticalVelocity = Vector3.zero;
                }
                else
                {
                    m_VerticalVelocity += Physics.gravity * Time.deltaTime;
                }

                motion += m_VerticalVelocity * Time.deltaTime;

                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                    m_IsMovingXROrigin = true;
                    m_CharacterController.Move(motion);
                    EndLocomotion();
                }
            }
            else
            {
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    m_IsMovingXROrigin = true;
                    xrOrigin.transform.position += motion;
                    EndLocomotion();
                }
            }
        }

        void FindCharacterController()
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            // Save a reference to the optional CharacterController on the rig GameObject
            // that will be used to move instead of modifying the Transform directly.
            if (m_CharacterController == null && !m_AttemptedGetCharacterController)
            {
                // Try on the Origin GameObject first, and then fallback to the XR Origin GameObject (if different)
                if (!xrOrigin.TryGetComponent(out m_CharacterController) && xrOrigin != system.xrOrigin.gameObject)
                    system.xrOrigin.TryGetComponent(out m_CharacterController);

                m_AttemptedGetCharacterController = true;
            }
        }
    }
}
