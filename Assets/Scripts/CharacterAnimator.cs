using UnityEngine;
using UnityEngine.Animations;

// Handles character animations
public class CharacterAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void AnimatePlayer(string animation)
    {
        // will probably treat the string more like an enum than a raw string in the future (like I will different things depending on the value passed not just passing a string to the animator)

        if (animation == "dash")
        {
            animator.SetInteger("dash_direction", 1);
        }

        if (animation == "backwards_dash")
        {
            animator.SetInteger("dash_direction", -1);
        }

        if (animation == "stop_dash")
        {
            animator.SetInteger("dash_direction", 0);
        }

        if (animation == "jump_rising")
        {
            animator.SetInteger("fall_direction", 1);
        }

        if (animation == "jump_falling")
        {
            animator.SetInteger("fall_direction", -1);
        }

        if (animation == "land")
        {
            animator.SetInteger("fall_direction", 0);
        }

        if (animation == "light_attack_1")
        {
            animator.SetTrigger("light_attack_1");
        }

        if (animation == "special_attack_1")
        {
            animator.SetTrigger("special_attack_1");
        }
    }

    public void AnimateMovement(float movementDirection)
    {
        if (animator != null)
        {
            animator.SetFloat("movement", movementDirection);
        }
    }
}
