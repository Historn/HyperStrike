using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EmissiveColorCycler : MonoBehaviour
{
    public float colorCycleSpeed = 1.0f; // How fast to cycle through colors
    public float emissionIntensity = 1.0f; // HDR intensity multiplier

    private Material material;
    private static readonly string emissionColorProperty = "_EmissionColor";

    void Start()
    {
        // Use a unique material instance to avoid affecting shared materials
        material = GetComponent<Renderer>().material;

        // Ensure emission is enabled on the material
        material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Generate a cycling RGB color using HSV
        Color color = Color.HSVToRGB(Mathf.PingPong(Time.time * colorCycleSpeed, 1f), 1f, 1f);

        // Multiply by intensity for HDR effect
        Color hdrColor = color * emissionIntensity;

        // Set the emission color
        material.SetColor(emissionColorProperty, hdrColor);
    }
}
