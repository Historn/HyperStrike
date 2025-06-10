using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RGBMaterialCycler : MonoBehaviour
{
    public float cycleSpeed = 1.0f;
    public float emissionIntensity = 1.0f;

    private Material material;
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        // Get a unique instance of the material
        material = GetComponent<Renderer>().material;

        // Enable emission keyword (required for URP)
        material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Generate a cycling RGB color
        Color rgbColor = Color.HSVToRGB(Mathf.PingPong(Time.time * cycleSpeed, 1f), 1f, 1f);

        // Set Base Map color
        material.SetColor(BaseColorID, rgbColor);

        // Set Emission color with intensity
        material.SetColor(EmissionColorID, rgbColor * emissionIntensity);
    }
}
