using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CageColorCycler : MonoBehaviour
{
    public float colorCycleSpeed = 1.0f;
    public float rimIntensity = 1.0f;

    private Material material;
    private static readonly int RimColorID = Shader.PropertyToID("_RimColor");

    void Start()
    {
        // Get a unique material instance
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Cycle HSV color
        Color color = Color.HSVToRGB(Mathf.PingPong(Time.time * colorCycleSpeed, 1f), 1f, 1f);

        // Apply optional intensity (optional — depends on how Shader Graph handles RimColor)
        color *= rimIntensity;

        // Update the shader graph property
        material.SetColor(RimColorID, color);
    }
}
