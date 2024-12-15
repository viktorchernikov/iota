using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaterSurface : MonoBehaviour
{
   [SerializeField] private List<float> waterLevels;
   [SerializeField] private int currentWaterLevelIndex = 0;
   [SerializeField] private float timeToChangeLevelInSeconds = 8;
   [SerializeField] private GameObject teleportPoint;
   [SerializeField] private bool isDeadly;
      
   public List<float> WaterLevels => waterLevels.GetRange(0, waterLevels.Count);
   public int CurrentWaterLevelIndex => currentWaterLevelIndex;
   public bool IsBusy => _isBusy;
   public bool IsDeadly { get; set; }
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
      if (_isBusy) throw new Exception("Cannot change water level while already changing water level");
      if (waterLevel < 0 || waterLevel >= waterLevels.Count) throw new Exception("Water Level Index out of bounds");
      if (currentWaterLevelIndex == waterLevel) return;

      _isBusy = true;

      StartCoroutine(ChangeLevelCo(waterLevel));
   }

   private void Update()
   {
      if (Application.IsPlaying(gameObject)) return;

      if (!waterLevels.Contains(0)) waterLevels.Add(0);
      
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
   
   private void Awake()
   {
      if (!Application.isPlaying) return;
      
      _levelPivotValue = transform.position.y;
   }

   private void OnTriggerEnter(Collider other)
   {
      if (!other.gameObject.transform.parent.TryGetComponent<Player>(out var player)) return;

      player.PlayerEnterWater(teleportPoint.transform.position, isDeadly);
   }

   private void OnTriggerStay(Collider other)
   {
      if (!other.gameObject.transform.parent.TryGetComponent<Player>(out var player)) return;

      var playerDivingDepth = _levelPivotValue - other.bounds.min.y;
      
      player.PlayerDiving(playerDivingDepth, teleportPoint.transform.position, isDeadly);
   }

   private void OnTriggerExit(Collider other)
   {
      if (!other.gameObject.transform.parent.TryGetComponent<Player>(out var player)) return;

      player.PlayerExitWater();
   }

   private void OnDrawGizmosSelected()
   {
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