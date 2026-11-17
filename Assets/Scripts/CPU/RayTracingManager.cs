using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class RayTracingManager : MonoBehaviour
{
    public RenderTexture renderTexture;
    public ComputeShader computeShader;
    public ObjectExporter objectExporter;
    private Camera _camera;
    private Vector3 _cameraOldPos;
    private Vector3 _cameraOldRot;
    public int frameCount;
    private ComputeBuffer sphereBuffer;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer materialBuffer;

    [Header("Ray Tracing Settings")]
    [Range(1, 30)]
    [SerializeField] private int bounceCount = 8;
    private int lastBounceCount;
    
    void Start()
    {
        setCamera();
        setFrameCount();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        checkCameraMove();
        checkObjectsMove();
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

    private void setSphereBuffer()
    {
        sphereBuffer = objectExporter.ExportSpheres();
    }

    private void setMeshesBuffer()
    {
        (vertexBuffer, triangleBuffer, materialBuffer) = objectExporter.ExportMeshes();
    }

    private void SetShaderParams()
    {   
        computeShader.SetInt("frameCount", frameCount);
        computeShader.SetInt("bounceCount", bounceCount);
        computeShader.SetBuffer(0, "Spheres", sphereBuffer);
        computeShader.SetBuffer(0, "Vertices", vertexBuffer);
        computeShader.SetBuffer(0, "Triangles", triangleBuffer);
        computeShader.SetBuffer(0, "triangleMaterials", materialBuffer);
        computeShader.SetInt("NumSpheres", objectExporter.spheresCount);
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

    private void checkObjectsMove()
    {
        if (objectExporter.didChange)
        {
            renderTexture = null;
            InitRenderTexture();

            frameCount = 0;
            objectExporter.didChange = false;
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

    private void setFrameCount()
    {
        frameCount = 0;
    }

    private void incrementFrameCount()
    {
        frameCount++;
    }
}
