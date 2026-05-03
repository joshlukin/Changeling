using UnityEngine;
using TMPro;

/// <summary>
/// Displays a small objective prompt on screen.
/// Other scripts call SetObjective() to update it.
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("TextMeshPro element showing the current objective. Place in a corner of the screen.")]
    public TextMeshProUGUI objectiveText;

    [Tooltip("Root panel for the objective UI — hide when no objective is active.")]
    public GameObject objectivePanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ClearObjective();
    }

    public void SetObjective(string text)
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(true);
        if (objectiveText != null)
            objectiveText.text = text;
    }

    public void ClearObjective()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);
        if (objectiveText != null)
            objectiveText.text = "";
    }
}