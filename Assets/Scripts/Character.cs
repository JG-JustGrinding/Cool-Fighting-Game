using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

// Basic character class to be extended for specific character behaviors
// This class serves as a foundation for character-related functionality.
// Handles logic, hitboxes/hurtboxes, actions, and calls animations
public class Character : MonoBehaviour
{

    [Header("Movement")]
    public float movementSpeed = 5.0f;
    protected float currentMovementSpeed = 5.0f;
    protected float jumpBoostMovementSpeedMultiplier = 1.0f;

    [Header("Dash Settings")]
    public float dashSpeed = 12.5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f;
    public float dashEndSmoothing = 0.1f;
    protected bool isDashing;
    protected bool hasDashed;
    protected bool isInDashCooldown;
    protected float dashDirection;
    protected float dashInputTiming = 0.102f; // max time between inputs for dash detection

    [Header("Jumping")]
    public float jumpForce = 23f;
    public float gravityScale = 7f;
    public float gravityScaleWhileFalling = 12f;
    public float airGravityIncrement = 0.5f;
    public float airGravityIncrementGrowth = 1.1f;
    public float jumpBurstSpeedMultiplier = 1.5f;
    public float jumpBurstDuration = 0.1f;
    public float jumpBurstVerticalBoost = 2.0f;
    float currentAirGravityIncrement = 0.5f;
    protected float jumpDirection;
    protected bool isGrounded;
    protected bool hasJumped;

    protected Rigidbody2D rb;

    private List<InputEntry> moveHistory = new List<InputEntry>(); // to track input history for inputs like dashing and special moves
    private readonly int moveHistoryLimit = 20;

    private readonly float groundCheckRadius = 0.1f;


    [Header("Components")]
    public Transform groundCheckTransform;

    private CharacterAnimator characterAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveHistory = new List<InputEntry>();
        currentMovementSpeed = movementSpeed;
        rb = GetComponent<Rigidbody2D>();
        characterAnimator = GetComponent<CharacterAnimator>();
    }

    public virtual void Move(float rawInputX, float rawInputY)
    {
        string inputType = "neutral";
        if (rawInputX >= 0.5)
        {
            rawInputX = 1.0f;
            inputType = "forward";

            if (rawInputX != dashDirection)
            {
                hasDashed = false;
            }
        } else if (rawInputX <= -0.5)
        {
            rawInputX = -1.0f;
            inputType = "backward";

            if (rawInputX != dashDirection)
            {
                hasDashed = false;
            }
        }
        else
        {
            rawInputX = 0.0f;
            hasDashed = false;
        }

        if (isDashing)
        {
            rawInputX = dashDirection;
        }

        if (!isGrounded)
        {
            rawInputX = jumpDirection;
        }

        Vector3 movement = new Vector3(rawInputX, 0) * GetMovementSpeed() * jumpBoostMovementSpeedMultiplier * Time.deltaTime;
        transform.Translate(movement, Space.World);
        AnimateMovement(rawInputX);

        AddNewMoveToHistory(inputType, true);

        if (rawInputX != 0f)
        {
            CheckForDash(rawInputX);
        }

        // crouch & jump logic
        if (rawInputY >= 0.65f)
        {
            Jump(rawInputX);
        }
        else if (rawInputY <= -0.5f)
        {
            Crouch();
        }
    }

    void CheckForDash(float direction)
    {
        if (isDashing || hasDashed || isInDashCooldown || Time.time - moveHistory[^1].time > dashInputTiming || !isGrounded)
        {
            return;
        }

        if (CheckLastMoveCombination("forward", "neutral", "forward") || CheckLastMoveCombination("backward", "neutral", "backward"))
        {
            if (LastMovesWithinThreshold(dashInputTiming, 3))
            {
                Dash(direction);
            }
        }
    }

    public virtual void ReceiveButtonInput(InputEntry input)
    {
        // Logic to receive and process input
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, LayerMask.GetMask("Ground"));

        if (!isGrounded)
        {
            jumpBoostMovementSpeedMultiplier = 1.5f; // give burst of speed while in air
            hasJumped = false; // this is mainly in case there's a weird case where the player is jumping but they're still considered grounded

            if (rb.linearVelocityY > 0)
            {
                AnimatePlayer("jump_rising");
            }
            else
            {
                AnimatePlayer("jump_falling");
            }
        } else
        {
            jumpBoostMovementSpeedMultiplier = 1.0f;
            AnimatePlayer("land");
        }
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocityY < 0)
        {
            rb.gravityScale = gravityScaleWhileFalling;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        if (!isGrounded)
        {
            if (rb.gravityScale > 5000)
            {
                return; // prevent overflow
            }

            rb.gravityScale += currentAirGravityIncrement * Time.fixedDeltaTime;
            currentAirGravityIncrement *= airGravityIncrementGrowth;
        } else
        {
            currentAirGravityIncrement = airGravityIncrement;
        }
    }

    public virtual void Jump(float direction)
    {
        if (!isGrounded || hasJumped || isDashing)
        {
            return;
        }

        jumpDirection = direction;
        rb.linearVelocityY = jumpForce; // only use y because x is controlled by other methods
        AddNewMoveToHistory("jump");
        hasJumped = true;
    }

    protected IEnumerator JumpBurst()
    {
        // If the user jumps forward or backward, give a small burst of horizontal speed
        currentMovementSpeed = movementSpeed * jumpBurstSpeedMultiplier;

        rb.linearVelocityY += jumpBurstVerticalBoost;

        yield return new WaitForSeconds(jumpBurstDuration);

        currentMovementSpeed = movementSpeed;
    }

    public virtual void Crouch()
    {
        // Custom crouch logic can be implemented here
        AddNewMoveToHistory("crouch", true);
    }

    public virtual void Block()
    {
        // Custom block logic can be implemented here
    }

    public virtual void Dash(float direction)
    {
        if (isDashing)
        {
            return;
        }

        dashDirection = direction;

        StartCoroutine(DashCoroutine());
    }

    protected IEnumerator DashCoroutine()
    {
        isDashing = true;

        currentMovementSpeed = dashSpeed;

        AnimatePlayer(dashDirection > 0 ? "dash" : "backwards_dash");

        yield return new WaitForSeconds(dashDuration);

        float initialSpeed = movementSpeed;
        float elapsedTime = 0f;
        while (elapsedTime < dashEndSmoothing)
        {
            currentMovementSpeed = Mathf.Lerp(initialSpeed, currentMovementSpeed, elapsedTime / dashEndSmoothing);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        hasDashed = true;
        isInDashCooldown = true;
        isDashing = false;

        AnimatePlayer("stop_dash");

        yield return new WaitForSeconds(dashCooldown);

        isInDashCooldown = false;
    }

    public virtual void Attack()
    {
        // Custom attack logic can be implemented here
    }

    public virtual void SpecialMove()
    {
        // Custom special move logic can be implemented here
    }

    protected virtual float GetMovementSpeed()
    {
        return currentMovementSpeed; // affected by blocking, dashing, crouching, etc.
    }

    public virtual void AddNewMoveToHistory(string inputName, bool noRepeats = false)
    {
        if (noRepeats && moveHistory.Count > 0)
        {
            InputEntry lastInput = moveHistory[moveHistory.Count - 1];
            if (lastInput.name == inputName)
            {
                return; // Do not add duplicate input
            }
        }
        InputEntry newEntry = new() { name = inputName, timingFromLastInput = GetLastMoveTime(), time = Time.time };
        moveHistory.Add(newEntry);
        if (moveHistory.Count > moveHistoryLimit)
        {
            moveHistory.RemoveAt(0); // Remove oldest entry
        }
    }

    protected virtual float GetLastMoveTime()
    {
        if (moveHistory.Count == 0)
        {
            return 0.0f;
        }

        return Time.time - moveHistory[moveHistory.Count - 1].time;
    }

    protected virtual bool CheckLastMoveCombination(params string[] combination)
    {
        if (moveHistory.Count < combination.Length)
        {
            return false;
        }

        if (moveHistory.Count == 0)
        {
            return false;
        }

        for (int i = 1; i < combination.Length + 1; i++)
        {
            var move = moveHistory[moveHistory.Count - i];
            var expectedMove = combination[combination.Length - i];

            if (move.name != expectedMove)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks the last number of moves to see if they have been inputted within a certain threshold.
    /// </summary>
    /// <param name="threshold">The time frame within each input</param>
    /// <param name="moves">How many inputs the move combination is for. For example, if the combination is 3 moves long, it will check the last 2.</param>
    /// <returns>bool input succedded</returns>
    protected virtual bool LastMovesWithinThreshold(float threshold, int moves)
    {
        if (moves <= 1)
        {
            return true; // no timing window, so return true
        }

        if (moveHistory.Count < moves)
        {
            return false;
        }

        if (moveHistory.Count == 0)
        {
            return false;
        }

        for (int i = 1; i < moves; i++)
        {
            if (moveHistory[^i].timingFromLastInput > threshold)
            {
                return false;
            }
        }

        return true;
    }

    // I have no idea if this works the way I want it to
    protected virtual bool LastMovesWithinThreshold(List<float> thresholds, int moves)
    {
        if (moves <= 1)
        {
            return true; // no timing window, so return true
        }

        if (moveHistory.Count < moves)
        {
            return false;
        }

        if (moveHistory.Count == 0)
        {
            return false;
        }

        if (thresholds.Count != moves - 1)
        {
            throw new System.Exception("Thresholds list length does not match moves count.");
        }

        for (int i = 1; i < moves; i++)
        {
            // ts should work from first move to last move I think
            var move = moveHistory[(moveHistory.Count - 1) - i];
            var threshold = thresholds[i - 1];
            if (move.timingFromLastInput > threshold)
            {
                return false;
            }
        }

        return true;
    }

    protected float GetSnappedInput(float rawInput)
    {
        if (rawInput >= 0.5)
        {
            return 1.0f;
        }
        else if (rawInput <= -0.5)
        {
            return -1.0f;
        }

        return 0f;
    }

    protected void AnimatePlayer(string animation)
    {
        if (characterAnimator != null)
        {
            characterAnimator.AnimatePlayer(animation);
        }
    }

    protected void AnimateMovement(float movementDirection)
    {
        if (characterAnimator != null)
        {
            characterAnimator.AnimateMovement(movementDirection);
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }
}

[System.Serializable]
public struct InputEntry
{
    public string name; // light, medium, heavy, special, etc. whatever we decide
    public float timingFromLastInput;
    public float time;
}
