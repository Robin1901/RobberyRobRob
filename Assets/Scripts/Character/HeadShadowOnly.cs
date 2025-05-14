using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class HeadShadowOnly : MonoBehaviour
{
    public bool enabled = false;

    void Start()
    {
        if (enabled)
        {
            var rend = GetComponent<Renderer>();

            rend.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }

    }
}
