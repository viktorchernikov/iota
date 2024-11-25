using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public Mesh GetMesh()
    {
        // TODO: Rewrite!!
        return GetComponent<MeshFilter>().mesh;
    }
    public Renderer GetRenderer()
    {
        return GetComponent<MeshRenderer>();
    }
    public void UpdateMesh(Mesh newMesh)
    {
        GetComponent<MeshFilter>().mesh = newMesh;
    }
    public void SetColision(Mesh collisionMesh)
    {
        foreach (var col in GetComponents<Collider>())
            Destroy(col);
        var newCollider = gameObject.AddComponent<MeshCollider>();
        newCollider.sharedMesh = collisionMesh;
        newCollider.convex = true;
    }
}
