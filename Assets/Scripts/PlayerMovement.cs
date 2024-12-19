using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    bool isFacingRight = true;
    public Animator animator;
    public ParticleSystem smokeFX;
    BoxCollider2D playerCollider;

    [Header("Movement")]
    public float moveSpeed = 5;
    float horizontalMovement;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.1f;
    bool isDashing;
    bool canDash = true;
    TrailRenderer trailRenderer;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;
    bool isOnPlatform;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("Wall Check")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("Wall Movement")]
    public float wallSlideSpeed = 2f;
    bool isWallSliding;
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
    }


    void Update()
    {
        animator.SetBool("isWalking", Mathf.Abs(horizontalMovement) > 0);
        smokeFX.Play();

        if (isDashing)
        {
            return;
        }

        GroundCheck();
        Gravity();
        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }   
    }

    private void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if(context.performed && canDash)
        {
            StartCoroutine(DashCouroutine());
        }
    }

    private IEnumerator DashCouroutine()
    {
        Physics2D.IgnoreLayerCollision(7, 8, true);
        canDash = false;
        isDashing = true;
        trailRenderer.emitting = true;

        float dashDirection = isFacingRight ? 1f : -1f;

        rb.velocity = new Vector2(dashDirection * dashSpeed, rb.velocity.y);

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = new Vector2(0f, rb.velocity.y);

        isDashing = false;
        trailRenderer.emitting = false;
        Physics2D.IgnoreLayerCollision(7, 8, false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
            smokeFX.Play();
        }
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && isOnPlatform && playerCollider.enabled)
        {
            StartCoroutine(DisablePlayerCollider(0.25f));
        }
    }

    private IEnumerator DisablePlayerCollider(float disableTime)
    {
        playerCollider.enabled = false;
        yield return new WaitForSeconds(disableTime);
        playerCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0 && context.performed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            jumpsRemaining--;
        }


        //WallJump
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;

            //Force Flip
            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime +0.1f);
        }
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
        else
        {
            isGrounded = false;
            animator.SetBool("isJumping", true);
            smokeFX.Play();
        }
    }

    private void WallSlide()
    {
        if(!isGrounded & WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
            animator.SetBool("isWallSliding", true);
            smokeFX.Play();
            animator.SetBool("isJumping", false);
        }
        else
        {
            isWallSliding = false;
            animator.SetBool("isWallSliding", false);
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}
