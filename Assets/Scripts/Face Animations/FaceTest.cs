using UnityEngine;

public class FaceTest : MonoBehaviour
{
    public Animator animator;

    private bool Blinking = false;
    private bool Smiling = false;
    private bool Angry = false;


    private void isBlinking()
    {
        Smiling = false;
        Angry = false;
        if (!Blinking)
        {
            resetExpressions();
        }
        Blinking = true;
        animator.SetBool("Blinking", true);
    }
    private void isSmiling()
    {
        Blinking = false;
        Angry = false;
        if (!Smiling)
        {
            resetExpressions();
        }
        Blinking = true;
        animator.SetBool("isSmiling", true);
    }
    private void isAngry()
    {
        Smiling = false;
        Blinking = false;
        if (!Angry)
        {
            resetExpressions();
        }
        Blinking = true;
        animator.SetBool("isAngry", true);
    }
    private void resetExpressions()
    {
        animator.SetBool("Blinking", false);
        animator.SetBool("isAngry", false);
        animator.SetBool("isSmiling", false);
    }

}
