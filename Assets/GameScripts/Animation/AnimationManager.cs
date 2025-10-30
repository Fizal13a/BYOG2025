using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private static readonly int moveHash = Animator.StringToHash("move");
    private static readonly int passHash = Animator.StringToHash("pass");
    private static readonly int tackleHash = Animator.StringToHash("tackle");
    private static readonly int shootHash = Animator.StringToHash("shoot");

    private bool isMoving;


    public static AnimationManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    public void MoveAnim(Animator animator, bool moving)
    {
        if (isMoving == moving) return;
        isMoving = moving;
        animator.SetBool(moveHash, moving);
    }

    public void PassAnim(Animator animator)
    {
        animator.SetTrigger(passHash);
    }

    public void TackleAnim(Animator animator)
    {
        animator.SetTrigger(tackleHash);
    }

    public void ShootAnim(Animator animator)
    {
        animator.SetTrigger(shootHash);
    }
    public void ResetAllTriggers(Animator animator)
    {
        animator.ResetTrigger(passHash);
        animator.ResetTrigger(shootHash);
        animator.ResetTrigger(tackleHash);
    }
}
