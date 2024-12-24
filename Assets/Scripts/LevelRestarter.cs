using UnityEngine;

public class LevelRestart : MonoBehaviour
{
    public static LevelRestart local;

    public void Restart() 
    {
        foreach (var o in FindObjectsOfType<Object>())
        {
            if (o is IRestartable restartable)
            {
                restartable.RestartState();
            }
        }
    }

    private void Awake()
    {
        InitSingleton();
    }
    
    private void InitSingleton()
    {
        if (local == null) local = this;
        else if (local != this) Destroy(gameObject);
    }
}
