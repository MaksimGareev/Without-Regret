using UnityEngine;
using UnityEngine.Events;

public enum BossActionType
{
    VoidProjectile,
    ArmSweep, 
    DropPillars,
    Random,
    Idle
}
[System.Serializable]
public class BossAction
{
    public string Name = "New Boss Action";
    public BossActionType ActionType;
    public float DelayUntilNextAction;
    [Tooltip("Invoked when the action starts, after the boss starts performing the action designated by ActionType.")]
    public UnityEvent OnActionStart;
    [Tooltip("Invoked when the action finishes, after the boss finishes performing the action designated by ActionType.")]
    public UnityEvent OnActionEnd;

    public virtual void Initiate(BossEnemyController boss, bool showDebugLogs)
    {
        if (showDebugLogs) Debug.Log("Boss Action Activated: " + Name);
        boss.PerformAction(ActionType);
        OnActionStart.Invoke();
    }

    public virtual void FinishAction(bool showDebugLogs)
    {
        if (showDebugLogs) Debug.Log("Boss Action Finished: " + Name);
        OnActionEnd.Invoke();
    }
}
