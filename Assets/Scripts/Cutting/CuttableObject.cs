using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public CuttableObjectDeformation deformation = CuttableObjectDeformation.NonDeformable;

    // Deformable components
    SkinnedMeshRenderer _skinnedrenderer;
    // Non deformable components
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;


    private void Awake()
    {
        if (deformation == CuttableObjectDeformation.NonDeformable)
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        else
        {
            _skinnedrenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }
    }

    public void UpdateMesh(Mesh newMesh)
    {
        if (deformation == CuttableObjectDeformation.NonDeformable)
            _meshFilter.mesh = newMesh;
        else
            _skinnedrenderer.sharedMesh = newMesh;
    }
    public void SetColision(Mesh collisionMesh)
    {
        foreach (var col in GetComponents<Collider>())
            Destroy(col);
        var newCollider = gameObject.AddComponent<MeshCollider>();
        newCollider.sharedMesh = collisionMesh;
        newCollider.convex = true;
    }

    public Mesh GetMesh() => deformation == CuttableObjectDeformation.NonDeformable ? _meshFilter.mesh : _skinnedrenderer.sharedMesh;
    public Renderer GetRenderer() => deformation == CuttableObjectDeformation.NonDeformable ? _meshRenderer : _meshRenderer;
}
public enum CuttableObjectDeformation
{
    /// <summary>
    /// Mesh that is static, doesn't deform. MeshFilter component is used
    /// </summary>
    NonDeformable,
    /// <summary>
    /// Mesh that is deformable. SkinnedMeshRenderer component is used
    /// </summary>
    Deformable
}
