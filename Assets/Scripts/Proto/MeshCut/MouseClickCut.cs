using UnityEngine;

public class MouseClickCut : MonoBehaviour
{
	public PlayerMovement player;

    void Update(){
		if(Input.GetMouseButtonDown(0) && player.ninjaMode){
			RaycastHit hit;

			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 4f)){
				GameObject victim = hit.collider.gameObject;
				if(victim.tag == "Cuttable")
                {
					Debug.Log("Cut");
                    Cutter.Cut(victim, hit.point, Camera.main.transform.right);
                }
			}

		}
	}
}
