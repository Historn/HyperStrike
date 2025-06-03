using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class Crowd : MonoBehaviour
{
    [Range(0, 5)] public float defaultSpeed;
    [Range(0, 5)] public float cheeringSpeed;
    [Range(0, 5)] public float superCheeringSpeed;
    [Range(0, 1)] public float cheerRandomFactor;

    [Range(0, 1)] public float maxHeight;

    [HideInInspector] public float currentSpeedFactor;

    public GameObject cheerTrigger;
    public GameObject superCheerTrigger;

    private void Awake()
    {
        currentSpeedFactor = defaultSpeed;
    }

    private void Update()
    {
        if (cheerTrigger.activeSelf)
        {
            UpdateState("Cheer");
        }
        else if (superCheerTrigger.activeSelf)
        {
            UpdateState("SuperCheer");
        }
        else
        {
            UpdateState("Idle");
        }

    }

    private void UpdateState(string state)
    {
        switch (state)
        {
            case "Idle":
                currentSpeedFactor = defaultSpeed;
                break;

            case "Cheer":
                currentSpeedFactor = cheeringSpeed;
                break;

            case "SuperCheer":
                currentSpeedFactor = superCheeringSpeed;
                break;
        }
    }


}
