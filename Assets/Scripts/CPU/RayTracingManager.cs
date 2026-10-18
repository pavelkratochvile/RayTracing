using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

struct Sphere
{
    public Vector3 center;
    public float radius;
    public Vector3 albedo;
    public Vector3 emissionColor;
    public Vector3 emissionStrength;
}

public class RayTracingManager : MonoBehaviour
{
    public RenderTexture renderTexture;
    public ComputeShader computeShader;
    private Camera _camera;
    private List<Sphere> spheres = new List<Sphere>();
    
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        InitRenderTexture();
        SetShaderParams();

        int numThreadGroupX = Mathf.CeilToInt(Screen.width / 8.0f);
        int numThreadGroupY = Mathf.CeilToInt(Screen.height / 8.0f);
        computeShader.Dispatch(0, numThreadGroupX, numThreadGroupY, 1);

        Graphics.Blit(renderTexture, destination);
    }

    private void InitRenderTexture()
    {
        if(renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        {
            if(renderTexture != null)
            {
                renderTexture.Release();
            }
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
    }

    private void SetShaderParams()
    {   
        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Sphere));
        int sphereCount = spheres.Count;
        ComputeBuffer sphereBuffer = new ComputeBuffer(sphereCount, stride);
        sphereBuffer.SetData(spheres.ToArray());
        sphereBuffer.Release();

        computeShader.SetBuffer(0, "Spheres", sphereBuffer);
        computeShader.SetInt("NumSpheres", sphereCount);
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetMatrix("CameraToWorld", _camera.cameraToWorldMatrix);
        computeShader.SetMatrix("CameraInverseProjection", _camera.projectionMatrix.inverse);
    }
}
