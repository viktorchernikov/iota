using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSounds : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] AudioSource footstepSource;
    [Header("Sounds")]
    [SerializeField] AudioClip[] footsteps;

    public void PlayFootstep()
    {
        footstepSource.Stop();
        footstepSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footstepSource.time = 0;
        footstepSource.Play();
    }
}
