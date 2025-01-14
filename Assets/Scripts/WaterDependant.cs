using UnityEngine;


public class WaterDependant : MonoBehaviour
{
    public bool inWater = false;

    public void NotifyInsideWater()
    {
        inWater = true;
    }
    public void NotifyOutOfWater()
    {
        inWater = false;
    }
}
