using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle
{
    private int submeshIndex;

    public List<Vector3> Vertices { get; set; } = new();
    public List<Vector3> Normals { get; set; } = new();
    public List<Vector2> UVs { get; set; } = new();
    public int SubmeshIndex { get => submeshIndex; private set => SubmeshIndex = value; }

    public MeshTriangle() { }
    public void Set(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int submeshIndex)
    {
        Clear();

        Vertices.Capacity = Vertices.Count + vertices.Length;
        Normals.Capacity = Normals.Count + normals.Length;
        UVs.Capacity = UVs.Count + uvs.Length;

        Vertices.AddRange(vertices);
        Normals.AddRange(normals);
        UVs.AddRange(uvs);

        this.submeshIndex = submeshIndex;
    }

    public void Clear()
    {
        Vertices.Clear();
        Normals.Clear();
        UVs.Clear();
        
        submeshIndex = 0;
    }
}
