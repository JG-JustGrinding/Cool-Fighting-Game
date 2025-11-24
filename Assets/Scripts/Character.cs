using UnityEngine;
using System.Collections.Generic;

// Basic character class to be extended for specific character behaviors
// This class serves as a foundation for character-related functionality.
// Handles logic, hitboxes/hurtboxes, actions, and calls animations
public class Character : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    private List<InputEntry> moveHistory = new List<InputEntry>(); // to track input history for inputs like dashing and special moves
    private int moveHistoryLimit = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveHistory = new List<InputEntry>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Move(float rawInput)
    {
        string inputType = "neutral";

        if (rawInput >= 0.5)
        {
            rawInput = 1.0f;
            inputType = "forward";
        } else if (rawInput <= -0.5)
        {
            rawInput = -1.0f;
            inputType = "backward";
        } 
        else
        {
            rawInput = 0.0f;
        }

        Vector3 movement = new Vector3(rawInput, 0) * GetMovementSpeed() * Time.deltaTime;
        transform.Translate(movement, Space.World);

        CheckForDash();

        AddNewMoveToHistory(inputType, true);
    }

    public virtual void ReceiveVerticalInput(float direction)
    {
        // crouch or jump logic
    }

    void CheckForDash()
    {
        // Logic to check moveHistory for dash input pattern
        if (moveHistory.Count < 3)
        {
            return; // Not enough inputs to dash
        }
        InputEntry lastInput = moveHistory[moveHistory.Count - 1];
        InputEntry secondLastInput = moveHistory[moveHistory.Count - 2];
        InputEntry thirdLastInput = moveHistory[moveHistory.Count - 3];
        if (lastInput.name == "forward" && secondLastInput.name == "neutral" && thirdLastInput.name == "forward")
        {
            // check to see if timing is within dash window
            lastInput = secondLastInput;
            if (lastInput.timingFromLastInput < 0.102f && thirdLastInput.timingFromLastInput < 0.102f)
            {
                Dash();
            }
        }
    }

    public virtual void ReceiveButtonInput(InputEntry input)
    {
        // Logic to receive and process input
    }

    public virtual void Jump()
    {
        // Custom jump logic can be implemented here
    }

    public virtual void Crouch()
    {
        // Custom crouch logic can be implemented here
    }

    public virtual void Block()
    {
        // Custom block logic can be implemented here
    }

    public virtual void Dash()
    {
        // Custom dash logic can be implemented here
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
        return movementSpeed; // affected by blocking, dashing, crouching, etc.
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
        InputEntry newEntry = new InputEntry { name = inputName, timingFromLastInput = GetLastMoveTime(), time = Time.time };
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
}

public struct InputEntry
{
    public string name; // light, medium, heavy, special, etc. whatever we decide
    public float timingFromLastInput;
    public float time;
}
