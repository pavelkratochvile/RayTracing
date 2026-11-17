using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class ObjectExporter : MonoBehaviour
{
    [SerializeField] private List<Sphere> spheres = new List<Sphere>();
    [SerializeField] private List<TriangleMesh> meshes = new List<TriangleMesh>();
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

    public (ComputeBuffer vertexBuffer, ComputeBuffer triangleBuffer, ComputeBuffer materialBuffer) ExportMeshes()
    {
        int vertexOffset = 0;
        int materialOffset = 0;
        List<Vector3> globalVertices = new List<Vector3>();
        List<Triangle> globalTriangles = new List<Triangle>();
        List<Material> globalMaterial = new List<Material>();

        foreach(TriangleMesh mesh in meshes)
        {
            // Přidání vertexů za sebe
            globalVertices.AddRange(mesh.vertices);

            // Přidání materiálů
            globalMaterial.AddRange(mesh.materials);
            
            // Přidání indexů (ukazují na vertexy) za sebe s tím že vždy měníme jejich indexy
            foreach(Triangle triangle in mesh.triangles)
            {
                Triangle adjustedTriangle = new Triangle
                {
                    id0 = triangle.id0 + vertexOffset,
                    id1 = triangle.id1 + vertexOffset,
                    id2 = triangle.id2 + vertexOffset,
                    materialIndex = triangle.materialIndex + materialOffset
                };
                globalTriangles.Add(adjustedTriangle);
            }
            vertexOffset += mesh.vertices.Length;
            materialOffset += mesh.materials.Length;
        }

        // Vytvoření a přiřazení compute bufferů
        int vertexStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3));
        int triangleStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Triangle));
        int materialStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Material));

        ComputeBuffer vertexBuffer = new ComputeBuffer(globalVertices.Count, vertexStride);
        ComputeBuffer triangleBuffer = new ComputeBuffer(globalTriangles.Count, triangleStride);
        ComputeBuffer materialBuffer = new ComputeBuffer(globalMaterial.Count, materialStride);

        vertexBuffer.SetData(globalVertices.ToArray());
        triangleBuffer.SetData(globalTriangles.ToArray());
        materialBuffer.SetData(globalMaterial.ToArray());

        return (vertexBuffer, triangleBuffer, materialBuffer);
    }
}