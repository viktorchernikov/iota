using System;
using UnityEngine;

public class MouseClickCut : MonoBehaviour
{
	private PlayerKatana _playerKatana = null;

	private void Awake()
	{
		_playerKatana = GetComponent<PlayerKatana>();
	}
		
    private void Update()
    {
	    Debug.Log(_playerKatana);
	    if (_playerKatana.KatanaState != PlayerKatanaState.Holding) return;
	    if (!Input.GetMouseButtonDown(0)) return;
	    if (!Camera.main) throw new Exception("No main camera found");
	    if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 4f)) return;
	    
	    var victim = hit.collider.gameObject;

	    if (!victim.CompareTag("Cuttable")) return;
	    
	    Debug.Log("Cut");
		Cutter.Cut(victim, hit.point, Camera.main.transform.right);
    }
}
