using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Cutter : MonoBehaviour
{
    private static bool isBusy;
    private static Mesh originalMesh;
    private static Vector3[] originalMeshVertices;
    private static Vector3[] originalMeshNormals;
    private static Vector2[] originalMeshUv;

    private static ObjectPool<MeshTriangle> meshTrianglePool = new ObjectPool<MeshTriangle>
        (
            createFunc: () =>
            {
                return new MeshTriangle();
            },
            actionOnRelease: (MeshTriangle triangle) =>
            {
                triangle.Clear();
            },
            defaultCapacity: 4,
            maxSize: 8
        );
    private static ObjectPool<GeneratedMesh> generatedMeshPool = new ObjectPool<GeneratedMesh>
        (
            createFunc: () =>
            {
                return new GeneratedMesh();
            },
            actionOnRelease: (GeneratedMesh mesh) =>
            {
                mesh.Clear();
            },
            defaultCapacity: 2,
            maxSize: 4
        );


    public static void Cut(GameObject originalGameObject, Vector3 contactPoint, Vector3 cutNormal)
    {
        if(isBusy)
        {
            Debug.LogWarning("Cutter is busy!");
            return;
        }

        isBusy = true;

        MeshFilter originalMeshFilter = originalGameObject.GetComponent<MeshFilter>();
        MeshRenderer originalMeshRenderer = originalGameObject.GetComponent<MeshRenderer>();

        Plane cutPlane = new Plane(originalGameObject.transform.InverseTransformDirection(-cutNormal), originalGameObject.transform.InverseTransformPoint(contactPoint));
        originalMesh = originalMeshFilter.mesh;
        originalMeshVertices = originalMesh.vertices;
        originalMeshNormals = originalMesh.normals;
        originalMeshUv = originalMesh.uv;

        if (originalMesh == null)
        {
            Debug.LogError("Need mesh to cut");
            return;
        }
        
        List<Vector3> addedVertices = new List<Vector3>();
        GeneratedMesh leftMesh = generatedMeshPool.Get();
        GeneratedMesh rightMesh = generatedMeshPool.Get();
        
        SeparateMeshes(leftMesh,rightMesh,cutPlane,addedVertices);
        FillCut(addedVertices, cutPlane, leftMesh, rightMesh);

        Mesh finishedLeftMesh = leftMesh.GetGeneratedMesh();
        Mesh finishedRightMesh = rightMesh.GetGeneratedMesh();



        //Getting and destroying all original colliders to prevent having multiple colliders
        //of different kinds on one object
        foreach (var col in originalGameObject.GetComponents<Collider>())
            Destroy(col);

        originalMeshFilter.mesh = finishedLeftMesh;

        var originalMeshCollider = originalGameObject.AddComponent<MeshCollider>();
        originalMeshCollider.sharedMesh = finishedLeftMesh;
        originalMeshCollider.convex = true;

        int matsCount = Mathf.Min(finishedLeftMesh.subMeshCount, originalMeshRenderer.materials.Length);
        Material[] mats = new Material[matsCount];
        for (int i = 0; i < matsCount; i++)
		{
            mats[i] = originalMeshRenderer.materials[i];
        }
        originalMeshRenderer.materials = mats;

        GameObject right = new GameObject();
        right.tag = originalGameObject.tag;

        right.transform.position = originalGameObject.transform.position + (Vector3.up * .05f);
        right.transform.rotation = originalGameObject.transform.rotation;
        right.transform.localScale = originalGameObject.transform.localScale;

        matsCount = Mathf.Min(finishedRightMesh.subMeshCount, originalMeshRenderer.materials.Length);
        mats = new Material[matsCount];
        for (int i = 0; i < matsCount; i++)
		{
            mats[i] = originalMeshRenderer.materials[i];
        }

        MeshRenderer rightMeshRenderer = right.AddComponent<MeshRenderer>();
        rightMeshRenderer.materials = mats;

        MeshFilter rightMeshFilter = right.AddComponent<MeshFilter>();
        rightMeshFilter.mesh = finishedRightMesh;
        
        MeshCollider rightMeshCollider = right.AddComponent<MeshCollider>();
        rightMeshCollider.sharedMesh = finishedRightMesh;
        rightMeshCollider.convex = true;
        
        var rightRigidbody = right.AddComponent<Rigidbody>();
        rightRigidbody.AddRelativeForce(-cutPlane.normal * 250f);
        var leftRigidbody = originalGameObject.GetComponent<Rigidbody>();
        leftRigidbody.AddRelativeForce(cutPlane.normal * 250f);

        generatedMeshPool.Release(leftMesh);
        generatedMeshPool.Release(rightMesh);

        isBusy = false;
    }

    /// <summary>
    /// Iterates over all the triangles of all the submeshes of the original mesh to separate the left
    /// and right side of the plane into individual meshes.
    /// </summary>
    /// <param name="leftMesh"></param>
    /// <param name="rightMesh"></param>
    /// <param name="plane"></param>
    /// <param name="addedVertices"></param>
    private static void SeparateMeshes(GeneratedMesh leftMesh,GeneratedMesh rightMesh, Plane plane, List<Vector3> addedVertices)
    {
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            var subMeshIndices = originalMesh.GetTriangles(i);

            //We are now going through the submesh indices as triangles to determine on what side of the mesh they are.
            for (int j = 0; j < subMeshIndices.Length; j+=3)
            {
                var triangleIndexA = subMeshIndices[j];
                var triangleIndexB = subMeshIndices[j + 1];
                var triangleIndexC = subMeshIndices[j + 2];

                MeshTriangle currentTriangle = GetTriangle(triangleIndexA,triangleIndexB,triangleIndexC,i);

                //We are now using the plane.getside function to see on which side of the cut our trianle is situated 
                //or if it might be cut through
                bool triangleALeftSide = plane.GetSide(originalMeshVertices[triangleIndexA]);
                bool triangleBLeftSide = plane.GetSide(originalMeshVertices[triangleIndexB]);
                bool triangleCLeftSide = plane.GetSide(originalMeshVertices[triangleIndexC]);

                switch (triangleALeftSide)
                {
                    //All three vertices are on the left side of the plane, so they need to be added to the left
                    //mesh
                    case true when triangleBLeftSide && triangleCLeftSide:
                        leftMesh.AddTriangle(currentTriangle);
                        break;
                    //All three vertices are on the right side of the mesh.
                    case false when !triangleBLeftSide && !triangleCLeftSide:
                        rightMesh.AddTriangle(currentTriangle);
                        break;
                    default:
                        CutTriangle(plane,currentTriangle, triangleALeftSide, triangleBLeftSide, triangleCLeftSide,leftMesh,rightMesh,addedVertices);
                        break;
                }
                meshTrianglePool.Release(currentTriangle);
            }
        }
    }

    /// <summary>
    /// Returns the tree vertices of a triangle as one MeshTriangle to keep code more readable
    /// </summary>
    /// <param name="_triangleIndexA"></param>
    /// <param name="_triangleIndexB"></param>
    /// <param name="_triangleIndexC"></param>
    /// <param name="_submeshIndex"></param>
    /// <returns></returns>
    private static MeshTriangle GetTriangle(int _triangleIndexA, int _triangleIndexB, int _triangleIndexC, int _submeshIndex)
    {
        var triangle = meshTrianglePool.Get();
        triangle.Vertices.Add(originalMeshVertices[_triangleIndexA]);
        triangle.Vertices.Add(originalMeshVertices[_triangleIndexB]);
        triangle.Vertices.Add(originalMeshVertices[_triangleIndexC]);
        triangle.Normals.Add(originalMeshNormals[_triangleIndexA]);
        triangle.Normals.Add(originalMeshNormals[_triangleIndexB]);
        triangle.Normals.Add(originalMeshNormals[_triangleIndexC]);
        triangle.UVs.Add(originalMeshUv[_triangleIndexA]);
        triangle.UVs.Add(originalMeshUv[_triangleIndexB]);
        triangle.UVs.Add(originalMeshUv[_triangleIndexC]);
        return triangle;
    }

    /// <summary>
    /// Cuts a triangle that exists between both sides of the cut apart adding additional vertices
    /// where needed to create intact triangles on both sides.
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="triangle"></param>
    /// <param name="triangleALeftSide"></param>
    /// <param name="triangleBLeftSide"></param>
    /// <param name="triangleCLeftSide"></param>
    /// <param name="leftMesh"></param>
    /// <param name="rightMesh"></param>
    /// <param name="addedVertices"></param>
    private static void CutTriangle(Plane plane,MeshTriangle triangle, bool triangleALeftSide, bool triangleBLeftSide, bool triangleCLeftSide,
    GeneratedMesh leftMesh, GeneratedMesh rightMesh, List<Vector3> addedVertices)
    {
        List<bool> leftSide = new List<bool>();
        leftSide.Add(triangleALeftSide);
        leftSide.Add(triangleBLeftSide);
        leftSide.Add(triangleCLeftSide);

        MeshTriangle leftMeshTriangle = meshTrianglePool.Get();
        MeshTriangle rightMeshTriangle = meshTrianglePool.Get();

        var emptyV3 = ArrayPool<Vector3>.Shared.Rent(2);
        var emptyV2 = ArrayPool<Vector2>.Shared.Rent(2);

        leftMeshTriangle.Set(emptyV3, emptyV3, emptyV2, triangle.SubmeshIndex);
        rightMeshTriangle.Set(emptyV3, emptyV3, emptyV2, triangle.SubmeshIndex);

        ArrayPool<Vector3>.Shared.Return(emptyV3);
        ArrayPool<Vector2>.Shared.Return(emptyV2);

        bool left = false;
        bool right = false;

        for (int i = 0; i < 3; i++)
        {
            if(leftSide[i])
            {
                if (!left)
                {
                    left = true;

                    leftMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    leftMeshTriangle.Vertices[1] = leftMeshTriangle.Vertices[0];

                    leftMeshTriangle.UVs[0] = triangle.UVs[i];
                    leftMeshTriangle.UVs[1] = leftMeshTriangle.UVs[0];

                    leftMeshTriangle.Normals[0] = triangle.Normals[i];
                    leftMeshTriangle.Normals[1] = leftMeshTriangle.Normals[0];
                }
                else
                {
                    leftMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    leftMeshTriangle.Normals[1] = triangle.Normals[i];
                    leftMeshTriangle.UVs[1] = triangle.UVs[i];
                }
            }
            else
            {
                if(!right)
                {
                    right = true;

                    rightMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    rightMeshTriangle.Vertices[1] = rightMeshTriangle.Vertices[0];

                    rightMeshTriangle.UVs[0] = triangle.UVs[i];
                    rightMeshTriangle.UVs[1] = rightMeshTriangle.UVs[0];

                    rightMeshTriangle.Normals[0] = triangle.Normals[i];
                    rightMeshTriangle.Normals[1] = rightMeshTriangle.Normals[0];

                }
                else
                {
                    rightMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    rightMeshTriangle.Normals[1] = triangle.Normals[i];
                    rightMeshTriangle.UVs[1] = triangle.UVs[i];
                }
            }
        }

        float normalizedDistance;
        float distance;
        plane.Raycast(new Ray(leftMeshTriangle.Vertices[0], (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).normalized), out distance);

        normalizedDistance = distance / (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).magnitude;
        Vector3 vertLeft = Vector3.Lerp(leftMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[0], normalizedDistance);
        addedVertices.Add(vertLeft);

        Vector3 normalLeft = Vector3.Lerp(leftMeshTriangle.Normals[0], rightMeshTriangle.Normals[0], normalizedDistance);
        Vector2 uvLeft = Vector2.Lerp(leftMeshTriangle.UVs[0], rightMeshTriangle.UVs[0], normalizedDistance);
        
        plane.Raycast(new Ray(leftMeshTriangle.Vertices[1], (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).normalized), out distance);

        normalizedDistance = distance / (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).magnitude;
        Vector3 vertRight = Vector3.Lerp(leftMeshTriangle.Vertices[1], rightMeshTriangle.Vertices[1], normalizedDistance);
        addedVertices.Add(vertRight);

        Vector3 normalRight = Vector3.Lerp(leftMeshTriangle.Normals[1], rightMeshTriangle.Normals[1], normalizedDistance);
        Vector2 uvRight = Vector2.Lerp(leftMeshTriangle.UVs[1], rightMeshTriangle.UVs[1], normalizedDistance);

        //TESTING OUR FIRST TRIANGLE
        MeshTriangle currentTriangle = meshTrianglePool.Get();
        Vector3[] updatedVertices = ArrayPool<Vector3>.Shared.Rent(3);
        Vector3[] updatedNormals = ArrayPool<Vector3>.Shared.Rent(3);
        Vector2[] updatedUVs = ArrayPool<Vector2>.Shared.Rent(3);

        updatedVertices[0] = leftMeshTriangle.Vertices[0];
        updatedVertices[1] = vertLeft;
        updatedVertices[2] = vertRight;
        updatedNormals[0] = leftMeshTriangle.Normals[0];
        updatedNormals[1] = normalLeft;
        updatedNormals[2] = normalRight;
        updatedUVs[0] = leftMeshTriangle.UVs[0];
        updatedUVs[1] = uvLeft;
        updatedUVs[2] = uvRight;

        currentTriangle.Set(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);

        //If our vertices ant the same
        if(updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0],updatedVertices[2] - updatedVertices[0]),updatedNormals[0]) < 0) 
            {
                FlipTriangel(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        //SECOND TRIANGLE 
        updatedVertices[0] = leftMeshTriangle.Vertices[0];
        updatedVertices[1] = leftMeshTriangle.Vertices[1];
        updatedVertices[2] = vertRight;
        updatedNormals[0] = leftMeshTriangle.Normals[0];
        updatedNormals[1] = leftMeshTriangle.Normals[1];
        updatedNormals[2] = normalRight;
        updatedUVs[0] = leftMeshTriangle.UVs[0];
        updatedUVs[1] = leftMeshTriangle.UVs[1];
        updatedUVs[2] = uvRight;


        currentTriangle.Clear();
        currentTriangle.Set(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        //If our vertices arent the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0],updatedVertices[2] - updatedVertices[0]),updatedNormals[0]) < 0) 
            {
                FlipTriangel(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        //THIRD TRIANGLE 
        updatedVertices[0] = rightMeshTriangle.Vertices[0];
        updatedVertices[1] = vertLeft;
        updatedVertices[2] = vertRight;
        updatedNormals[0] = rightMeshTriangle.Normals[0];
        updatedNormals[1] = normalRight;
        updatedNormals[2] = normalRight;
        updatedUVs[0] = rightMeshTriangle.UVs[0];
        updatedUVs[1] = uvLeft;
        updatedUVs[2] = uvRight;

        currentTriangle.Clear();
        currentTriangle.Set(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        //If our vertices arent the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0],updatedVertices[2] - updatedVertices[0]),updatedNormals[0]) < 0) 
            {
                FlipTriangel(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }

        //FOURTH TRIANGLE 
        updatedVertices[0] = rightMeshTriangle.Vertices[0];
        updatedVertices[1] = rightMeshTriangle.Vertices[1];
        updatedVertices[2] = vertRight;
        updatedNormals[0] = rightMeshTriangle.Normals[0];
        updatedNormals[1] = rightMeshTriangle.Normals[1];
        updatedNormals[2] = normalRight;
        updatedUVs[0] = rightMeshTriangle.UVs[0];
        updatedUVs[1] = rightMeshTriangle.UVs[1];
        updatedUVs[2] = uvRight;

        currentTriangle.Clear();
        currentTriangle.Set(updatedVertices, updatedNormals, updatedUVs, triangle.SubmeshIndex);
        //If our vertices arent the same
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if(Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0],updatedVertices[2] - updatedVertices[0]),updatedNormals[0]) < 0) 
            {
                FlipTriangel(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }

        meshTrianglePool.Release(currentTriangle);
        meshTrianglePool.Release(leftMeshTriangle);
        meshTrianglePool.Release(rightMeshTriangle);
        ArrayPool<Vector3>.Shared.Return(updatedVertices);
        ArrayPool<Vector3>.Shared.Return(updatedNormals);
        ArrayPool<Vector2>.Shared.Return(updatedUVs);
    }

    private static void FlipTriangel(MeshTriangle _triangle)
    {
        Vector3 temp = _triangle.Vertices[2];
        _triangle.Vertices[2] = _triangle.Vertices[0];
        _triangle.Vertices[0] = temp;

        temp = _triangle.Normals[2];
		_triangle.Normals[2] = _triangle.Normals[0];
		_triangle.Normals[0] = temp;

		(_triangle.UVs[2], _triangle.UVs[0]) = (_triangle.UVs[0], _triangle.UVs[2]);
    }

    public static void FillCut(List<Vector3> _addedVertices, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        HashSet<Vector3> vertices = new HashSet<Vector3>();
        List<Vector3> polygon = new List<Vector3>();

        for (int i = 0; i < _addedVertices.Count; i++)
        {
            if(!vertices.Contains(_addedVertices[i]))
            {
                polygon.Clear();
                polygon.Add(_addedVertices[i]);
                polygon.Add(_addedVertices[i + 1]);

                vertices.Add(_addedVertices[i]);
                vertices.Add(_addedVertices[i + 1]);

                EvaluatePairs(_addedVertices, vertices, polygon);
                Fill(polygon, _plane, _leftMesh, _rightMesh);
            }
        }
    }

    public static void EvaluatePairs(List<Vector3> _addedVertices, HashSet<Vector3> _vertices, List<Vector3> _polygone)
    {
        bool isDone = false;
        while(!isDone)
        {
            isDone = true;
            for (int i = 0; i < _addedVertices.Count; i+=2)
            {
                Vector3 vertexA = _addedVertices[i];
                Vector3 vertexB = _addedVertices[i + 1];
                Vector3 lastPolygoneVertex = _polygone[^1];

                if (vertexA == lastPolygoneVertex && _vertices.Add(vertexB))
                {
                    isDone = false;
                    _polygone.Add(vertexB);
                } 
                else if (vertexB == lastPolygoneVertex && _vertices.Add(vertexA))
                {
                    isDone = false;
                    _polygone.Add(vertexA);
                }
            }
        }
    }

    private static void Fill(List<Vector3> _vertices, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        //Firstly we need the center we do this by adding up all the vertices and then calculating the average
        Vector3 centerPosition = Vector3.zero;
        for (int i = 0; i < _vertices.Count; i++)
        {
            centerPosition += _vertices[i];
        }
        centerPosition /= _vertices.Count;

        //We now need an Upward Axis we use the plane we cut the mesh with for that 
        Vector3 up = new Vector3()
        {
            x = _plane.normal.x,
            y = _plane.normal.y,
            z = _plane.normal.z
        };

        Vector3 left = Vector3.Cross(_plane.normal, up);

        Vector3 displacement = Vector3.zero;
        Vector2 uv1 = Vector2.zero;
        Vector2 uv2 = Vector2.zero;

        Vector3[] vertices = ArrayPool<Vector3>.Shared.Rent(3);
        Vector3[] normals = ArrayPool<Vector3>.Shared.Rent(3);
        Vector2[] uvs = ArrayPool<Vector2>.Shared.Rent(3);

        ArrayPool<Vector3>.Shared.Return(vertices);
        ArrayPool<Vector3>.Shared.Return(normals);
        ArrayPool<Vector2>.Shared.Return(uvs);

        for (int i = 0; i < _vertices.Count; i++)
        {
            displacement = _vertices[i] - centerPosition;
            uv1 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up)
            };

            displacement = _vertices[(i + 1) % _vertices.Count] - centerPosition;
            uv2 = new Vector2()
            { 
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up)
            };

            vertices[0] = _vertices[i];
            vertices[1] = _vertices[(i + 1) % _vertices.Count];
            vertices[2] = centerPosition;

            normals[0] = -_plane.normal;
            normals[1] = -_plane.normal;
            normals[2] = -_plane.normal;
            uvs[0] = uv1;
            uvs[1] = uv2;
            uvs[2] = new(0.5f, 0.5f);

            MeshTriangle currentTriangle = meshTrianglePool.Get();
            currentTriangle.Set(vertices, normals, uvs, originalMesh.subMeshCount + 1);

            if(Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0],vertices[2] - vertices[0]),normals[0]) < 0)
            {
                FlipTriangel(currentTriangle);
            }
            _leftMesh.AddTriangle(currentTriangle);

            normals[0] = -_plane.normal;
            normals[1] = -_plane.normal;
            normals[2] = -_plane.normal;
            currentTriangle.Set(vertices, normals, uvs, originalMesh.subMeshCount + 1);

            if(Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0],vertices[2] - vertices[0]),normals[0]) < 0)
            {
                FlipTriangel(currentTriangle);
            }
            _rightMesh.AddTriangle(currentTriangle);
            meshTrianglePool.Release(currentTriangle);
        
        } 
    }
}
    
