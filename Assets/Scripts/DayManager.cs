using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central game state manager. Singleton — one instance persists across scenes.
/// Tracks the current day, story flags, and player stats.
/// </summary>
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day")]
    public int currentDay = 1;

    [Header("Stats")]
    public int relationshipStat = 0;
    public float daughterHealth = 90f;

    // One-off story flags: e.g. "has_opened_curtains", "has_placed_homework"
    private Dictionary<string, bool> _flags = new Dictionary<string, bool>();
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void AdvanceDay()
    {
        currentDay++;
        Debug.Log($"[DayManager] Day advanced to {currentDay}");
    }


    public void SetFlag(string key, bool value = true)
    {
        _flags[key] = value;
    }

    public bool GetFlag(string key)
    {
        return _flags.TryGetValue(key, out bool value) && value;
    }
    

    public void AddRelationship(int amount)
    {
        relationshipStat += amount;
        Debug.Log($"[DayManager] Relationship: {relationshipStat}");
    }

    public void SetDaughterHealth(float value)
    {
        daughterHealth = Mathf.Clamp(value, 0f, 100f);
        Debug.Log($"[DayManager] Daughter health: {daughterHealth}");
    }

    public void ModifyDaughterHealth(float delta)
    {
        SetDaughterHealth(daughterHealth + delta);
    }
}