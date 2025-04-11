using UnityEngine;

public class Sparc : MonoBehaviour
{
    public SparcMovement movement { get; private set; }
    public SparcScatter scatter { get; private set; }

    private void Awake()
    {
        movement = GetComponent<SparcMovement>();
        scatter = GetComponent<SparcScatter>();
    }

    private void Start()
    {
        ResetState(); // Call this AFTER Awake runs and initializes everything
    }

    public void ResetState()
    {
        if (movement != null)
            movement.enabled = false;

        if (scatter != null)
            scatter.Enable(); // or scatter.enabled = true, depending on your logic
    }
}
