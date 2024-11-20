using UnityEngine;

public class ConditionTestButton : MonoBehaviour
{
    [SerializeField] private KeyCode keyToActivate;
    [SerializeField] private PuzzleCondition condition;
    private Renderer _rend;
    
    private void Awake()
    {
        _rend = GetComponentInChildren<Renderer>();
        _rend.material.color = Color.red;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(keyToActivate)) return;

        condition.InvertFulfillment();
        UpdateRenderer();
    }

    private void UpdateRenderer()
    {
        _rend.material.color = condition.wasFulfilled ? Color.green : Color.red;
    }
}
