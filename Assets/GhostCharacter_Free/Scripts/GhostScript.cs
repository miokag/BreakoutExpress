using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");

        [SerializeField] private float Speed = 4;
        [SerializeField] private float jumpHeight = 2.0f; // Height of the jump
        private bool isJumping = false; // To track if the character is jumping

        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();
        }

        void Update()
        {
            STATUS();
            GRAVITY();
            MOVE();
            HandleJump();
            HandleScale();
        }

        private void STATUS()
        {
            // Status logic can be added here if needed
        }

        //---------------------------------------------------------------------
        // gravity for fall of this character
        //---------------------------------------------------------------------
        private void GRAVITY()
        {
            if (Ctrl.enabled)
            {
                if (CheckGrounded())
                {
                    if (MoveDirection.y < -0.1f)
                    {
                        MoveDirection.y = -0.1f;
                    }
                }
                else
                {
                    MoveDirection.y -= 0.1f; // Apply gravity
                }
                Ctrl.Move(MoveDirection * Time.deltaTime);
            }
        }

        //---------------------------------------------------------------------
        // whether it is grounded
        //---------------------------------------------------------------------
        private bool CheckGrounded()
        {
            if (Ctrl.isGrounded && Ctrl.enabled)
            {
                return true;
            }
            Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = 0.2f;
            return Physics.Raycast(ray, range);
        }

        //---------------------------------------------------------------------
        // for moving
        //---------------------------------------------------------------------
        private void MOVE()
        {
            // velocity
            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == MoveState)
            {
                if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    MOVE_Velocity(new Vector3(0, 0, -Speed), new Vector3(0, 180, 0));
                }
                else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    MOVE_Velocity(new Vector3(0, 0, Speed), new Vector3(0, 0, 0));
                }
                else if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                {
                    MOVE_Velocity(new Vector3(Speed, 0, 0), new Vector3(0, 90, 0));
                }
                else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A))
                {
                    MOVE_Velocity(new Vector3(-Speed, 0, 0), new Vector3(0, 270, 0));
                }
            }
            KEY_DOWN();
            KEY_UP();
        }

        //---------------------------------------------------------------------
        // value for moving
        //---------------------------------------------------------------------
        private void MOVE_Velocity(Vector3 velocity, Vector3 rot)
        {
            MoveDirection = new Vector3(velocity.x, MoveDirection.y, velocity.z);
            if (Ctrl.enabled)
            {
                Ctrl.Move(MoveDirection * Time.deltaTime);
            }
            MoveDirection.x = 0;
            MoveDirection.z = 0;
            this.transform.rotation = Quaternion.Euler(rot);
        }

        //---------------------------------------------------------------------
        // whether WASD key is key down
        //---------------------------------------------------------------------
        private void KEY_DOWN()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Anim.CrossFade(MoveState, 0.1f, 0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Anim.CrossFade(MoveState, 0.1f, 0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                Anim.CrossFade(MoveState, 0.1f, 0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Anim.CrossFade(MoveState, 0.1f, 0, 0);
            }
        }

        //---------------------------------------------------------------------
        // whether WASD key is key up
        //---------------------------------------------------------------------
        private void KEY_UP()
        {
            if (Input.GetKeyUp(KeyCode.W))
            {
                if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
            else if (Input.GetKeyUp(KeyCode.A))
            {
                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                {
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A))
                {
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
        }

        //---------------------------------------------------------------------
        // handle jumping
        //---------------------------------------------------------------------
        private void HandleJump()
        {
            if (CheckGrounded() && Input.GetKeyDown(KeyCode.Space))
            {
                MoveDirection.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
        }

        //---------------------------------------------------------------------
        // handle scaling
        //---------------------------------------------------------------------
        private void HandleScale()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                this.transform.localScale = new Vector3(.5f, .5f, .5f); // Scale down to .5
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.C))
                {
                    this.transform.localScale = new Vector3(1, 1, 1); // Scale up to 1
                }
            }
        }
    }
}