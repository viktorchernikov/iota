using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scales : MonoBehaviour
{
    [SerializeField] private MeshCollider scollider;
    [SerializeField] private Transform panTransform;
    
    public float ConditionTopRange = 1;
    public float ConditionBotRange = 0;

    private bool ConditionSatisfied;

    public float panHeight;

    private void Start() {
        scollider = GetComponent<MeshCollider>();
        panTransform = GetComponent<Transform>();
        panHeight = panTransform.position.y;
    }


    void FixedUpdate()
    {
      panHeight = panTransform.position.y;

      ConditionSatisfied = ConditionRangeCheck();
      if (ConditionSatisfied)
        Debug.Log("you passed");

    }


    bool ConditionRangeCheck() {
      return panHeight <= ConditionTopRange && panHeight > ConditionBotRange;

    }


    void OnCollisionEnter(Collision collision) {
        Rigidbody otherBody = collision.collider.attachedRigidbody;
            if (otherBody != null)
                Debug.Log("collided with " + otherBody.name);
    }
    // void OnCollisionStay(Collision collision) {
    //   Debug.Log("Pan height = " +  panHeight);
    // }
}