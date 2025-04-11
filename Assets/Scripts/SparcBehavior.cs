using UnityEngine;
[RequireComponent(typeof(Sparc))]

public abstract class SparcBehavior : MonoBehaviour
{

    public Sparc sparc { get; private set; }
    public float duration;

    private void Awake()
    {
        sparc = GetComponent<Sparc>();
        this.enabled = false; // Disable this script by default
    }

    public void Enable()
    {
        Enable(this.duration);
    }

    public virtual void Enable(float duration)
    {
       this.enabled = true; // Enable this script

        Invoke(nameof(Disable), duration); 
    }
    public virtual void Disable()
    {
        this.enabled = false; // Disable this script
        CancelInvoke(); // Cancel any pending invocations of Disable
    }

}
