using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

public struct SphereDataGPU
{
    public Vector3 center;
    public float radius;
    public Material material;
}

public class Sphere : MonoBehaviour
{
    public Vector3 color;
    public Vector3 emissionColor;
    public Vector3 emissionStrength;
    public float smoothness;
    public bool didChange = false;
    Vector3 lastPosition;
    float lastScale;

    private void Start()
    {
        lastPosition = transform.position;
        lastScale = transform.localScale.x;
    }

    private void Update()
    {
        if (transform.position != lastPosition || transform.localScale.x != lastScale)
        {
            didChange = true;
            lastPosition = transform.position;
            lastScale = transform.localScale.x;
        }
    }

    public SphereDataGPU ExportGPUData()
    {
        Vector3 center = this.transform.position;
        float radius = this.transform.localScale.x * 0.5f;
        Vector3 albedo = this.color;
        Vector3 emissionColor = this.emissionColor;
        Vector3 emissionStrength = this.emissionStrength;
        float smoothness = this.smoothness;

        return new SphereDataGPU
        {
            center = center,
            radius = radius,
            material = new Material
            {
                color = albedo,
                emissionColor = emissionColor,
                emissionStrength = emissionStrength,
                smoothness = smoothness
            }
        };
    }
}