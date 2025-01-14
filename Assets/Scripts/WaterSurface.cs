using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaterSurface : MonoBehaviour
{
    public WaterSurfaceMode mode = WaterSurfaceMode.LevelRelative;

    public float MinimumHeight = 0f;
    public float MaximumHeight = 1f;

   [SerializeField] private List<float> waterLevels;
   [SerializeField] private int currentWaterLevelIndex = 0;
   [SerializeField] private float timeToChangeLevelInSeconds = 8;
   [SerializeField] private GameObject teleportPoint;
      
   public List<float> WaterLevels => waterLevels.GetRange(0, waterLevels.Count);
   public int CurrentWaterLevelIndex => currentWaterLevelIndex;
   public bool IsBusy => _isBusy;
    public bool IsDeadly { get; set; } = false;
   public event Action<int> OnWaterLevelChanged;

   private BoxCollider _collider;
   private float _levelPivotValue;
   private bool _isBusy = false;

   public void ToNextLevel()
   {
      ChangeWaterLevel(currentWaterLevelIndex + 1);
   }

   public bool ExistsNextLevel()
   {
      return currentWaterLevelIndex + 1 < waterLevels.Count;
   }

   public void ToPreviousLevel()
   {
      ChangeWaterLevel(currentWaterLevelIndex - 1);
   }

   public bool ExistsPreviousLevel()
   {
      return currentWaterLevelIndex - 1 > 0;
   }

   public void ChangeWaterLevel(int waterLevel)
   {
      if (_isBusy) return;
      if (mode != WaterSurfaceMode.LevelRelative) return;
      if (waterLevel < 0 || waterLevel >= waterLevels.Count) throw new Exception("Water Level Index out of bounds");
      if (currentWaterLevelIndex == waterLevel) return;

      _isBusy = true;

      StartCoroutine(ChangeLevelCo(waterLevel));
   }

   private void Update()
   {
       Monster monster = GameObject.FindAnyObjectByType<Monster>();
       if (monster == null) return;
       if (IsDeadly && monster.transform.position.y < transform.position.y)
       {
           Destroy(monster.gameObject);
       }
       if (Application.IsPlaying(gameObject)) return;

        if (mode != WaterSurfaceMode.LevelRelative) return;
      waterLevels.Sort((a, b) => b.CompareTo(a));
      currentWaterLevelIndex = waterLevels.FindIndex(val => val == 0);

      
   }

   private IEnumerator ChangeLevelCo(int toLevel)
   {
      var wayBetweenLevels = waterLevels[toLevel] - waterLevels[currentWaterLevelIndex];
      var timePassed = 0f;
      
      while(timePassed < timeToChangeLevelInSeconds)
      {
         var delta = Time.deltaTime;
         
         AddY(delta / timeToChangeLevelInSeconds * wayBetweenLevels);
         
         timePassed += delta;
         yield return null;
      }
      
      SetY(_levelPivotValue + waterLevels[toLevel]);
      
      currentWaterLevelIndex = toLevel;
      _isBusy = false;
      OnWaterLevelChanged?.Invoke(currentWaterLevelIndex);
   }

   private void AddY(float value)
   {
      var position = transform.position;
      position.y += value;
      transform.position = position;
   }

   private void SetY(float value)
   {
      var position = transform.position;
      position.y = value;
      transform.position = position;
   }
   public void SetY(Valve valve)
   {
        var position = transform.position;
        position.y = Mathf.Lerp(MinimumHeight, MaximumHeight, valve.value);
        transform.position = position;
    }
   
   private void Awake()
   {
      if (!Application.isPlaying) return;
      
      _levelPivotValue = transform.position.y;
   }


    private void OnTriggerEnter(Collider other)
    {
        if (!other) return;

        WaterDependant wd;

        if (!other.gameObject.TryGetComponent<WaterDependant>(out wd)) return;

        wd.NotifyInsideWater();
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other) return;

        WaterDependant wd;

        if (!other.gameObject.TryGetComponent<WaterDependant>(out wd)) return;

        wd.NotifyOutOfWater();
    }
    private void OnTriggerStay(Collider other)
   {
      if (!other) return;
      if (!other.gameObject.CompareTag("Player")) return;
      if (!other.gameObject.transform.parent.TryGetComponent<Player>(out var player)) return;

      var playerDivingDepth = _levelPivotValue - other.bounds.min.y;
      
      player.PlayerDiving(playerDivingDepth, Vector3.zero, IsDeadly);
   }

   private void OnDrawGizmosSelected()
   {
      if (mode != WaterSurfaceMode.LevelRelative) return;
      Gizmos.color = Color.green;
      var size = GetComponent<BoxCollider>().bounds.size;
      var p = transform.position;

      foreach (var level in waterLevels)
      {
         var y = Application.isPlaying ? _levelPivotValue + level : p.y + level;
         
         Gizmos.DrawLine(new Vector3(p.x - size.x / 2, y, p.z - size.z / 2), new Vector3(p.x + size.x / 2, y, p.z - size.z / 2));
         Gizmos.DrawLine(new Vector3(p.x + size.x / 2, y, p.z - size.z / 2), new Vector3(p.x + size.x / 2, y, p.z + size.z / 2));
         Gizmos.DrawLine(new Vector3(p.x + size.x / 2, y, p.z + size.z / 2), new Vector3(p.x - size.x / 2, y, p.z + size.z / 2));
         Gizmos.DrawLine(new Vector3(p.x - size.x / 2, y, p.z + size.z / 2), new Vector3(p.x - size.x / 2, y, p.z - size.z / 2));
      }
   }
}
public enum WaterSurfaceMode
{
    LevelRelative,
    ValueRelative
}