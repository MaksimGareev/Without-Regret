using UnityEngine;

/// <summary>
/// Acts as a mediator between the Journal and the journal's animation events to notify the journal when animations end
/// </summary>
public class JournalAnimationCallback : MonoBehaviour
{
    private bool animationFinished = false;

    public void SetAnimationFinishedTrue() => animationFinished = true;
    public void SetAnimationFinishedFalse() => animationFinished = false;
    /// <summary>
    /// Returns the value of animationFinished, which is true when an animation calls <c>SetAnimationFinishedTrue()</c>. 
    /// Should be used in combination with <c>WaitUntil</c> in coroutines.
    /// </summary>
    /// <returns></returns>
    public bool AnimationFinished() => animationFinished;
}
