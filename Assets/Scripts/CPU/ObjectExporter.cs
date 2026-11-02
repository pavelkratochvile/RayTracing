using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectExporter : MonoBehaviour
{
    [SerializeField] private List<Sphere> spheres = new List<Sphere>();
    public int spheresCount => spheres.Count;
    public bool didChange;

    public ComputeBuffer ExportSpheres()
    {
        int spheresCount = spheres.Count;
        if (spheresCount == 0) return null;

        List<SphereDataGPU> spheresDataGPU = new List<SphereDataGPU>();

        for(int i = 0; i < spheresCount; i++)
        {
            spheresDataGPU.Add(this.spheres[i].ExportGPUData());
            if (this.spheres[i].didChange)
            {
                this.didChange = true;
                this.spheres[i].didChange = false;
            }
        }

        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(SphereDataGPU));
        ComputeBuffer sphereBuffer = new ComputeBuffer(spheresCount, stride);
        sphereBuffer.SetData(spheresDataGPU.ToArray());
        return sphereBuffer;
    }
}