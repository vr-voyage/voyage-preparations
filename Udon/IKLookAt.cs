
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class IKLookAt : UdonSharpBehaviour
{
    Animator animator;
    public bool active = false;
    public Transform target;
    public float lookAtWeight = 1;

    public void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"{gameObject.name} IKLookAt : No Animator set on the object !");
            Debug.LogError("Engaging Self Destruct Sequence !");
            Destroy(this);
        }
    }

    public bool EverythingOk()
    {
        return ((animator != null) & (active == true) & (target != null));
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!EverythingOk())
        {
            animator.SetLookAtWeight(0);
            return;
        }

        animator.SetLookAtWeight(lookAtWeight);
        animator.SetLookAtPosition(target.position);
    }
}
