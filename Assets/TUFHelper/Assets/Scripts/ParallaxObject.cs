using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [SerializeField] 
    private Transform cameraTransform;
    [SerializeField] 
    private Vector2 parallaxEffectMultiplier;

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, 
                                          deltaMovement.y * parallaxEffectMultiplier.y, 
                                          0f);
        lastCameraPosition = cameraTransform.position;
    }
}
