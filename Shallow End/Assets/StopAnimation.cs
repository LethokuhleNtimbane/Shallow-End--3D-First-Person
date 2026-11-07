using UnityEngine;
using UnityEngine.SceneManagement;

public class StopAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationName;

    [Header("Object To Disable")]
    [SerializeField] private GameObject objectToDisable;

    private static bool animationHasPlayed = false;

    private void Start()
    {
        if (animationHasPlayed)
        {
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }

            return;
        }

        animator.Play(animationName, 0, 0f);
    }

    private void Update()
    {
        if (animationHasPlayed)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1f)
        {
            AnimationFinished();
        }
    }

    private void AnimationFinished()
    {
        animationHasPlayed = true;

        // Put your event here.
        // Example:
        // playerController.enabled = true;

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }
}