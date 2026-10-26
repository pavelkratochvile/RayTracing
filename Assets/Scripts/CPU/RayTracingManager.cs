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
    private Vector3 _cameraOldPos;
    private Vector3 _cameraOldRot;
    private ComputeBuffer sphereBuffer;
    private int frameCount;
    private List<Sphere> spheres = new List<Sphere>();
    
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
        
        // 1. Červená koule (levá)
        // Leží na zemi (y=1) a je posunutá doleva (x=-0.95)
        spheres.Add(new Sphere { 
            center = new Vector3(-0.95f, 1.0f, 5.0f), 
            radius = 1.0f, 
            albedo = new Vector3(1.0f, 0.1f, 0.1f), 
            emissionColor = Vector3.zero, 
            emissionStrength = Vector3.zero 
        });

        // 2. Zelená koule (pravá)
        // Téměř se dotýká červené (x=0.95), světlo se tak bude odrážet mezi nimi
        spheres.Add(new Sphere { 
            center = new Vector3(0.95f, 1.0f, 5.0f), 
            radius = 1.0f, 
            albedo = new Vector3(0.9f, 0.9f, 0.9f), 
            emissionColor = Vector3.zero, 
            emissionStrength = Vector3.zero 
        });

        // 3. Silný zdroj bílého světla nahoře mezi nimi
        // Albedo je nula (černé), ale emise je velmi vysoká (8)
        spheres.Add(new Sphere { 
            center = new Vector3(0.0f, 5.5f, 5.0f), 
            radius = 0.6f, 
            albedo = Vector3.zero, 
            emissionColor = new Vector3(1.0f, 1.0f, 1.0f), 
            emissionStrength = new Vector3(8.0f, 8.0f, 8.0f) 
        });

        // 4. Velká bílá koule vzadu (slouží jako "stěna")
        // Zachytí vržené stíny a krásně na ní uvidíte odraz červené i zelené barvy
        spheres.Add(new Sphere { 
            center = new Vector3(0.0f, 2.0f, 8.5f), 
            radius = 2.5f, 
            albedo = new Vector3(0.1f, 0.9f, 0.1f), 
            emissionColor = Vector3.zero, 
            emissionStrength = Vector3.zero 
        });

        // 5. Menší modrá kulička vepředu
        // Pro dodání hloubky scéně a ukázku odrazu barev z více stran
        spheres.Add(new Sphere { 
            center = new Vector3(0.0f, 0.4f, 3.2f), 
            radius = 0.4f, 
            albedo = new Vector3(0.1f, 0.4f, 1.0f), 
            emissionColor = Vector3.zero, 
            emissionStrength = Vector3.zero 
        });
        
        spheres.Add(new Sphere { 
            center = new Vector3(4.0f, 2.0f, 3.0f), 
            radius = 0.6f, 
            albedo = Vector3.zero, 
            emissionColor = new Vector3(1.0f, 1.0f, 1.0f), 
            emissionStrength = new Vector3(8.0f, 8.0f, 8.0f)
        });
        // (Zde by případně mělo být volání RebuildSphereBuffer(), pokud jste si 
        // předělal skript podle předchozích rad, aby se správně předala nová data do GPU).
    }

    private void SetShaderParams()
    {   
        computeShader.SetInt("frameCount", frameCount);
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
