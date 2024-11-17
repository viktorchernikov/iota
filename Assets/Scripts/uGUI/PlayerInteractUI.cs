using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] GameObject activePoint;
    [SerializeField] GameObject inactivePoint;

    [SerializeField] InteractableHoverResponse[] responseKeys;
    [SerializeField] GameObject[] responseValues;


    private void FixedUpdate()
    {
        PlayerInteractionSeeker seeker = Player.local.GetModule<PlayerInteractionSeeker>();

        activePoint.SetActive(false);
        inactivePoint.SetActive(false);
        DisableResponseTextes();

        if (seeker.hoveredObject != null)
        {
            InteractableHoverResponse res = seeker.hoveredObject.GetHoverResponse(Player.local);

            if (res != InteractableHoverResponse.None)
            {
                bool canInteract = seeker.hoveredObject.CanInteract(Player.local);

                if (canInteract)
                {
                    activePoint.SetActive(true);
                    for (int i = 0; i < responseKeys.Length; i++)
                    {
                        if (responseKeys[i] == res)
                        {
                            responseValues[i].SetActive(true);
                        }
                    }
                }
                else
                {
                    inactivePoint.SetActive(true);
                }
            }
        }
    }
    void DisableResponseTextes()
    {
        foreach (var resText in responseValues)
        {
            resText.SetActive(false);
        }
    }
}
