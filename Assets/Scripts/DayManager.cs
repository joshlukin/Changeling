using System.Collections.Generic;
using UnityEngine;
//NOTE: 
//Using DayManager for future days: every new day gets its own sequence script — Day2Sequence, Day3Sequence etc.
//At the end of Day1Sequence.EndOfDay(), after DayManager.Instance.AdvanceDay() is called, you load the next scene or activate Day2Sequence.
//Each day sequence only needs to know its own flags.
//The flags dictionary in DayManager persists across scenes since it uses DontDestroyOnLoad, so anything set in Day 1 is readable in Day 2.
//For example Day 2 can check DayManager.Instance.GetFlag("homework_placed") to branch dialogue, or read DayManager.Instance.daughterHealth to determine how Siofra is doing.
//New flags follow the same naming convention — lowercase with underscores, descriptive, set once.

/// <summary>
/// Central game state manager.
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