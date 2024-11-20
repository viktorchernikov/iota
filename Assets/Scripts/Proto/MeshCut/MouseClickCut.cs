using UnityEngine;

public class MouseClickCut : MonoBehaviour
{
	public Player player;

    void Update(){
		if(Input.GetMouseButtonDown(0)){
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
