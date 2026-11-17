using UnityEngine;
using System;

public struct Material
{
    public Vector3 color;
    public Vector3 emissionColor;
    public Vector3 emissionStrength;
    public float smoothness;
}

public struct Triangle
{
    public int id0, id1, id2;
    public int materialIndex;
}

public class TriangleMesh
{
    public Triangle[] triangles;
    public Vector3[] vertices;
    public Material[] materials;

    public TriangleMesh(Triangle[] triangles, Vector3[] vertices, Material[] materials)
    {
        this.triangles = triangles;
        this.vertices = vertices;
        this.materials = materials;
    }
}