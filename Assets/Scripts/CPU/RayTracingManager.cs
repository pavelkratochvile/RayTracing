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
    public float smoothness;
}

public class RayTracingManager : MonoBehaviour
{
    public RenderTexture renderTexture;
    public ComputeShader computeShader;
    private Camera _camera;
    private Vector3 _cameraOldPos;
    private Vector3 _cameraOldRot;
    private ComputeBuffer sphereBuffer;
    private int frameCount;
    private List<Sphere> spheres = new List<Sphere>();

    [Header("Ray Tracing Settings")]
    [Range(1, 30)]
    [SerializeField] private int bounceCount = 8;
    private int lastBounceCount;
    
    void Start()
    {
        setCamera();
        setSpheres();
        setSphereBuffer();
        setFrameCount();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        checkCameraMove();
        checkBounceCountChange();
        incrementFrameCount();
        InitRenderTexture();
        setSphereBuffer();
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
            renderTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
    }

    private void setSpheres()
    {
        spheres.Clear();

        // ==========================================
        // 1. SVĚTLA (Visí nahoře ve vzduchu)
        // ==========================================
        spheres.Add(new Sphere { center = new Vector3( 0.0f, 10.0f,  6.0f), radius = 2.5f, albedo = Vector3.zero, emissionColor = new Vector3(1.0f, 0.95f, 0.85f), emissionStrength = new Vector3(12f, 12f, 12f), smoothness = 0.0f });
        spheres.Add(new Sphere { center = new Vector3(-7.0f,  6.0f,  3.0f), radius = 1.2f, albedo = Vector3.zero, emissionColor = new Vector3(0.4f, 0.70f, 1.00f), emissionStrength = new Vector3( 8f,  8f,  8f), smoothness = 0.0f });
        spheres.Add(new Sphere { center = new Vector3( 7.0f,  5.0f,  8.0f), radius = 1.0f, albedo = Vector3.zero, emissionColor = new Vector3(1.0f, 0.40f, 0.20f), emissionStrength = new Vector3(10f, 10f, 10f), smoothness = 0.0f });
        spheres.Add(new Sphere { center = new Vector3(-2.0f,  4.5f, 12.0f), radius = 0.8f, albedo = Vector3.zero, emissionColor = new Vector3(0.9f, 0.20f, 0.90f), emissionStrength = new Vector3( 9f,  9f,  9f), smoothness = 0.0f });

        // ==========================================
        // 2. ZRCADLOVÉ KOULE (Smoothness = 1.0f, Y = radius)
        // ==========================================
        spheres.Add(new Sphere { center = new Vector3( 0.0f,  1.5f,  6.0f), radius = 1.50f, albedo = new Vector3(0.95f, 0.95f, 0.95f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3( 2.4f,  0.9f,  4.2f), radius = 0.90f, albedo = new Vector3(1.00f, 0.82f, 0.35f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3(-2.6f,  0.8f,  4.5f), radius = 0.80f, albedo = new Vector3(0.95f, 0.55f, 0.40f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3(-4.5f,  1.2f,  7.0f), radius = 1.20f, albedo = new Vector3(0.20f, 0.40f, 0.95f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3( 4.8f,  1.1f,  7.5f), radius = 1.10f, albedo = new Vector3(0.20f, 0.90f, 0.45f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3(-1.4f,  0.5f,  3.0f), radius = 0.50f, albedo = new Vector3(0.60f, 0.60f, 0.65f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3( 1.1f,  0.4f,  2.8f), radius = 0.40f, albedo = new Vector3(0.95f, 0.40f, 0.70f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3(-3.2f,  0.6f,  9.5f), radius = 0.60f, albedo = new Vector3(0.90f, 0.75f, 0.30f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3( 2.8f,  0.7f, 10.0f), radius = 0.70f, albedo = new Vector3(0.92f, 0.92f, 0.95f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });
        spheres.Add(new Sphere { center = new Vector3( 0.2f,  0.7f, 11.0f), radius = 0.70f, albedo = new Vector3(0.65f, 0.20f, 0.85f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 1.0f });

        // ==========================================
        // 3. MATNÉ A POLOMATNÉ KOULE (Y = radius)
        // ==========================================
        spheres.Add(new Sphere { center = new Vector3(-0.3f, 0.35f,  2.2f), radius = 0.35f, albedo = new Vector3(0.85f, 0.15f, 0.15f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.00f });
        spheres.Add(new Sphere { center = new Vector3( 0.6f, 0.30f,  2.1f), radius = 0.30f, albedo = new Vector3(0.15f, 0.75f, 0.85f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.10f });
        spheres.Add(new Sphere { center = new Vector3(-1.8f, 0.45f,  3.6f), radius = 0.45f, albedo = new Vector3(0.95f, 0.85f, 0.10f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.00f });
        spheres.Add(new Sphere { center = new Vector3(-1.9f,  1.0f,  6.2f), radius = 1.00f, albedo = new Vector3(0.88f, 0.25f, 0.20f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.15f });
        spheres.Add(new Sphere { center = new Vector3( 2.1f,  0.9f,  6.5f), radius = 0.90f, albedo = new Vector3(0.20f, 0.65f, 0.30f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.05f });
        spheres.Add(new Sphere { center = new Vector3( 3.7f,  0.6f,  5.2f), radius = 0.60f, albedo = new Vector3(0.92f, 0.45f, 0.15f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.20f });
        spheres.Add(new Sphere { center = new Vector3(-3.8f, 0.55f,  5.5f), radius = 0.55f, albedo = new Vector3(0.35f, 0.25f, 0.75f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.00f });
        spheres.Add(new Sphere { center = new Vector3(-0.8f,  0.5f,  4.5f), radius = 0.50f, albedo = new Vector3(0.95f, 0.95f, 0.90f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.00f });
        spheres.Add(new Sphere { center = new Vector3(-5.8f,  0.9f,  8.5f), radius = 0.90f, albedo = new Vector3(0.85f, 0.35f, 0.60f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.25f });
        spheres.Add(new Sphere { center = new Vector3( 5.5f,  0.8f,  9.0f), radius = 0.80f, albedo = new Vector3(0.15f, 0.50f, 0.75f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.10f });
        spheres.Add(new Sphere { center = new Vector3(-1.5f,  0.8f,  8.8f), radius = 0.80f, albedo = new Vector3(0.80f, 0.80f, 0.20f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.00f });
        spheres.Add(new Sphere { center = new Vector3( 1.5f, 0.65f,  8.5f), radius = 0.65f, albedo = new Vector3(0.70f, 0.15f, 0.30f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.30f });
        spheres.Add(new Sphere { center = new Vector3(-4.0f, 0.75f, 11.5f), radius = 0.75f, albedo = new Vector3(0.30f, 0.70f, 0.50f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.05f });
        spheres.Add(new Sphere { center = new Vector3( 4.0f, 0.75f, 12.0f), radius = 0.75f, albedo = new Vector3(0.90f, 0.60f, 0.35f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.10f });
        spheres.Add(new Sphere { center = new Vector3(-1.8f,  0.6f, 13.5f), radius = 0.60f, albedo = new Vector3(0.20f, 0.30f, 0.60f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.00f });
        spheres.Add(new Sphere { center = new Vector3( 2.2f,  0.6f, 13.0f), radius = 0.60f, albedo = new Vector3(0.85f, 0.85f, 0.85f), emissionColor = Vector3.zero, emissionStrength = Vector3.zero, smoothness = 0.40f });
    }

    private void SetShaderParams()
    {   
        computeShader.SetInt("frameCount", frameCount);
        computeShader.SetInt("bounceCount", bounceCount);
        computeShader.SetBuffer(0, "Spheres", sphereBuffer);
        computeShader.SetInt("NumSpheres", spheres.Count);
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetMatrix("CameraToWorld", _camera.cameraToWorldMatrix);
        computeShader.SetMatrix("CameraInverseProjection", _camera.projectionMatrix.inverse);
    }

    private void OnDestroy()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
        }
    }

    private void setCamera()
    {
        this._camera = GetComponent<Camera>();
        this._cameraOldPos = this._camera.transform.position;
        this._cameraOldRot = this._camera.transform.rotation.eulerAngles;
    }

    private void checkCameraMove()
    {
        Transform ct = _camera.transform;
        if(ct.position != _cameraOldPos || ct.rotation.eulerAngles != _cameraOldRot)
        {
            renderTexture = null;
            InitRenderTexture();

            frameCount = 0;
            _cameraOldPos = ct.position;
            _cameraOldRot = ct.rotation.eulerAngles;
        }
    }

    private void checkBounceCountChange()
    {
        if (bounceCount != lastBounceCount)
        {
            renderTexture = null;
            InitRenderTexture();

            frameCount = 0;
            lastBounceCount = bounceCount;
        }
    }

    private void setSphereBuffer()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
        }

        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Sphere));
        int sphereCount = spheres.Count;
        sphereBuffer = new ComputeBuffer(sphereCount, stride);
        sphereBuffer.SetData(spheres.ToArray());
    }

    private void setFrameCount()
    {
        frameCount = 0;
    }

    private void incrementFrameCount()
    {
        frameCount++;
    }
}
