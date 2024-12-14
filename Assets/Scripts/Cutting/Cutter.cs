using System;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    private static CuttingWorker worker = new CuttingWorker();

    public static void Queue(CuttableObject target, Vector3 contactPoint, Vector3 cutNormal)
    {
        worker.Occupy(target, contactPoint, cutNormal);
        worker.Proceed();
        worker.Release();
    }
    [Obsolete("Use Cutter.Queue() instead!")]
    public static void Cut(GameObject originalGameObject, Vector3 contactPoint, Vector3 cutNormal)
    {
        CuttableObject cuttableObject;
        if (!originalGameObject.TryGetComponent(out cuttableObject))
        {
            cuttableObject = originalGameObject.AddComponent<CuttableObject>();
        }
        Queue(cuttableObject, contactPoint, cutNormal);
    }
}
    
