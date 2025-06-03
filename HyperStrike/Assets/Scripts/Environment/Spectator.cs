using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    private Crowd crowd;

    private float angle;
    private float startingYPos;
    private float yOffset;
    private float randomSpeed;

    void Start()
    {
        crowd = FindAnyObjectByType<Crowd>();

        startingYPos = transform.position.y;
        randomSpeed = Random.Range(crowd.defaultSpeed - crowd.cheerRandomFactor, crowd.defaultSpeed + crowd.cheerRandomFactor);

        ChooseRandomColor();
    }

    private void FixedUpdate()
    {
        yOffset = startingYPos + crowd.maxHeight;
        angle += crowd.currentSpeedFactor * 0.1f * randomSpeed;

        Vector3 newPos = new Vector3(transform.position.x, yOffset + Mathf.Sin(angle) * crowd.maxHeight, transform.position.z);
        transform.position = newPos;
    }

    private void ChooseRandomColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material newMaterial = renderer.material;

        newMaterial.color = new Color(Random.Range(0, 256) / 255f, Random.Range(0, 256) / 255f, Random.Range(0, 256) / 255f);

        renderer.material = newMaterial;
    }
}
