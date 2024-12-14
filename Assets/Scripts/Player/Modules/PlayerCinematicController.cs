using System;
using System.Collections;
using UnityEngine;

public class PlayerCinematicController : PlayerModule
{
    /*
    public bool playOpening = false;
    public Vector3 openingPos = Vector3.zero;
    public Vector3 openingSpawnPos = Vector3.zero;
    public float openingSpawnYaw = 0;
    */

    public bool isPlaying { get; private set; } = false;
    /*
    [Header("Cinematics")]
    [SerializeField] PlayerCinematicSeqauence wormJumpinCinematic;
    [SerializeField] PlayerCinematicSeqauence wormJumpoutCinematic;
    [SerializeField] PlayerCinematicSeqauence openingCinematic;
    [SerializeField] PlayerCinematicSeqauence openingSpawnCinematic;
    */
    [Header("Tweaks")]
    [SerializeField] AnimationCurve adjustPosAndVeCurve;
    [Header("Components")]
    [SerializeField] AudioSource audioSource;

    public event Action onWormholeTeleport;

    /*
    private IEnumerator Start()
    {
        if (!playOpening) yield break;
        isPlaying = true;
        parent.SetDuringCinematic(true);


        GameObject sequence = Instantiate(openingCinematic.sequence, openingPos, Quaternion.identity);
        sequence.transform.localScale = Vector3.one * 0.5f;

        yield return null;

        audioSource.Stop();
        audioSource.time = 0;
        audioSource.clip = openingCinematic.audio;
        audioSource.Play();


        Transform camera_anchor = sequence.transform.GetChild(0).GetChild(0);
        float time = 0f;
        while (time <= openingCinematic.duration)
        {
            parent.usedCamera.SetPosition(camera_anchor.position);
            parent.usedCamera.SetViewAngles((Quaternion.Euler(camera_anchor.eulerAngles) * Quaternion.Euler(-60, 0, -180)).eulerAngles);
            time += Time.deltaTime;
            yield return null;
        }
        parent.usedCamera.SetPosition(camera_anchor.position);
        parent.usedCamera.SetViewAngles((Quaternion.Euler(camera_anchor.eulerAngles) * Quaternion.Euler(-60, 0, -180)).eulerAngles);


        GameObject sequence2 = Instantiate(openingSpawnCinematic.sequence, openingSpawnPos, Quaternion.identity * Quaternion.Euler(0, openingSpawnYaw - 180, 0));
        sequence2.transform.localScale = Vector3.one;

        yield return new WaitForFixedUpdate();

        camera_anchor = sequence2.transform.GetChild(0).GetChild(0);
        time = 0f;
        while (time <= openingSpawnCinematic.duration)
        {
            parent.usedCamera.SetPosition(camera_anchor.position - new Vector3(0, 0.95f, 0));
            parent.usedCamera.SetViewAngles((Quaternion.Euler(camera_anchor.eulerAngles) * Quaternion.Euler(-90, 0, -180)).eulerAngles);
            time += Time.deltaTime;
            yield return null;
        }
        Transform player_end_anchor = sequence2.transform.GetChild(0).GetChild(1);
        parent.SetDuringCinematic(false);
        parent.Teleport(player_end_anchor.position - new Vector3(0, 0.0f, 0));
        parent.usedCamera.SetPosition(camera_anchor.position);
        Debug.Log(player_end_anchor.position.y);
        parent.GetModule<PlayerCameraController>().viewAngles = new Vector2(0, openingSpawnYaw);
        yield return null;
        Destroy(sequence);
        Destroy(sequence2);
        Debug.Log(parent.transform.position.y);
        isPlaying = false;
    }
    public void PlayWormholeJumpin(Wormbox box)
    {
        StartCoroutine(WormboxSequence(box));
    }
    */

    #region Sequences
    /*
    IEnumerator WormboxSequence(Wormbox box)
    {
        isPlaying = true;
        parent.SetDuringCinematic(true);

        Vector3 playerToBoxDir = box.sitPoint.position - parent.usedRigidbody.position;
        Quaternion playerToBoxAng = Quaternion.LookRotation(playerToBoxDir) * Quaternion.Euler(wormJumpinCinematic.cameraStartViewAngles);
        float playerToBoxYaw = playerToBoxAng.eulerAngles.y;
        Vector3 playerToBoxPos = (Quaternion.Euler(0, playerToBoxYaw, 0) * Vector3.back * 3 * parent.currentScale) + box.sitPoint.position;
        Vector3 cameraToBoxPos = playerToBoxPos + Vector3.up * parent.cameraAnchor.localPosition.y;

        yield return AdjustCameraPositionAndViewAngles(cameraToBoxPos, playerToBoxAng.eulerAngles, 1f, 30f);

        float flapsCloseDelay = 1.5f;
        StartCoroutine(PlaySequence(wormJumpinCinematic, playerToBoxPos, Quaternion.identity * Quaternion.Euler(0, playerToBoxYaw, 0), Vector3.one * parent.currentScale, false, true));
        yield return new WaitForSeconds(flapsCloseDelay);
        box.CloseFlaps();
        box.linkedBox.CloseFlaps();
        yield return new WaitForSeconds(wormJumpinCinematic.duration - flapsCloseDelay);

        onWormholeTeleport?.Invoke();

        box.OpenFlaps();
        box.linkedBox.OpenFlaps();
        yield return PlaySequence(wormJumpoutCinematic, box.linkedBox.sitPoint.position, Quaternion.identity * Quaternion.Euler(0, box.linkedBox.transform.eulerAngles.y, 0), Vector3.one * box.linkedBox.playerScale, true);

        parent.SetDuringCinematic(false);
        isPlaying = false;
    }
    */
    #endregion

    #region Helpers
    /*
    IEnumerator PlaySequence(PlayerCinematicSeqauence cinematic, Vector3 pos, Quaternion rot, Vector3 scale, bool teleportEndPlayer = false, bool playAudio = false)
    {
        GameObject sequence = Instantiate(cinematic.sequence, pos, rot);
        sequence.transform.localScale = scale;

        yield return null;

        if (playAudio)
        {
            audioSource.Stop();
            audioSource.time = 0;
            audioSource.clip = cinematic.audio;
            audioSource.Play();
        }

        Transform camera_anchor = sequence.transform.GetChild(0).GetChild(0);
        float time = 0f;
        while (time <= cinematic.duration)
        {
            parent.usedCamera.SetPosition(camera_anchor.position);
            parent.usedCamera.SetViewAngles(camera_anchor.eulerAngles);
            time += Time.deltaTime;
            yield return null;
        }
        if (teleportEndPlayer)
        {
            Transform player_end_anchor = sequence.transform.GetChild(0).GetChild(1);
            parent.Teleport(player_end_anchor.position);
            parent.usedCamera.SetPosition(camera_anchor.position);
        }
        parent.usedCamera.SetViewAngles(camera_anchor.eulerAngles);
        Destroy(sequence);
    }
    */
    public IEnumerator AdjustCameraPositionAndViewAngles(Vector3 desiredPosition, Vector3 desiredViewAngles, float moveStep = 1f, float lookStep = 10f)
    {
        Quaternion desiredRotation = Quaternion.Euler(desiredViewAngles);
        float initialDistance = Vector3.Distance(parent.usedCamera.position, desiredPosition);
        float initialAngleDifference = Quaternion.Angle(Quaternion.Euler(parent.usedCamera.viewAngles), desiredRotation);


        while ((Vector3.Distance(parent.usedCamera.position, desiredPosition) >= float.Epsilon) || (Quaternion.Angle(Quaternion.Euler(parent.usedCamera.viewAngles), desiredRotation) >= 0.05f))
        {
            float posCompletePercentage = 1f - (Vector3.Distance(parent.usedCamera.position, desiredPosition)) / initialDistance;
            float rotCompletePercentage = 1f - (Quaternion.Angle(Quaternion.Euler(parent.usedCamera.viewAngles), desiredRotation) / initialAngleDifference);

            parent.usedCamera.SetPosition(Vector3.MoveTowards(parent.usedCamera.position, desiredPosition, moveStep * adjustPosAndVeCurve.Evaluate(posCompletePercentage) * Time.deltaTime));
            parent.usedCamera.SetViewAngles(Quaternion.RotateTowards(Quaternion.Euler(parent.usedCamera.viewAngles), desiredRotation, lookStep * adjustPosAndVeCurve.Evaluate(rotCompletePercentage) * Time.deltaTime).eulerAngles);
            
            Debug.Log($"{parent.usedCamera.position}, {parent.usedCamera.viewAngles}");
            
            yield return null;
        }
        parent.usedCamera.SetPosition(desiredPosition);
        parent.usedCamera.SetViewAngles(desiredViewAngles);
    }
    #endregion
}
