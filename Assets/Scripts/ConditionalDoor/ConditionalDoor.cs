using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionalDoor : MonoBehaviour
{
    [Header("State")]
    public ConditionalDoorState state = ConditionalDoorState.Closed;

    [Header("Sounds")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] openSounds;
    [SerializeField] AudioClip[] closeSounds;

    [Header("Parameters")]
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] Vector3 closedLocalPos = Vector3.zero;
    [SerializeField] Vector3 openedLocalPos = Vector3.zero;


    [SerializeReference] private List<PuzzleCondition> activators = new List<PuzzleCondition>();
    
    private void Awake()
    {
        activators.ForEach(doorActivator => doorActivator.onFulfilmentChange.AddListener(ConditionHandler));
    }

    private void ConditionHandler()
    {
        if (activators.All(activator => activator.GetConditionFulfilment())) Open();
        else Close();
    }

    public void Open()
    {
        StartCoroutine(OpenC());
    }
    public void Close()
    {
        StartCoroutine(CloseC());
    }

    public IEnumerator OpenC()
    {
        if (state == ConditionalDoorState.Closed)
        {
            PlayRandom(openSounds);
            yield return MoveGate(true);
        }
    }
    public IEnumerator CloseC()
    {
        if (state == ConditionalDoorState.Opened)
        {
            PlayRandom(closeSounds);
            yield return MoveGate(false);
        }
    }

    IEnumerator MoveGate(bool opened)
    {
        state = ConditionalDoorState.Busy;
        Vector3 destination = opened ? openedLocalPos : closedLocalPos;

        while (Vector3.Distance(transform.localPosition, destination) > 0.05f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = destination;
        state = opened ? ConditionalDoorState.Opened : ConditionalDoorState.Closed;
    }
    void PlayRandom(AudioClip[] collection)
    {
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        int index = UnityEngine.Random.Range(0, collection.Length);

        audioSource.clip = collection[index];
        audioSource.Play();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        closedLocalPos = transform.localPosition;
    }
#endif
}

public enum ConditionalDoorState
{
    Opened,
    Closed,
    Busy
}