using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for the kill / let go choice.
/// Triggered directly by Day6Sequence.
/// </summary>
public class KillChoicePanel : MonoBehaviour
{
    [Header("Art")]
    public Sprite chokingArt;

    [Header("UI")]
    public GameObject choiceRoot;
    public Button killButton;
    public Button letGoButton;
    public TextMeshProUGUI killText;
    public TextMeshProUGUI letGoText;

    [Header("Sequence")]
    public Day6Sequence day6Sequence;

    private bool choiceLocked = false;

    private void Awake()
    {
        choiceRoot.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Show()
    {
        choiceLocked = false;

        killText.text = "Kill";
        letGoText.text = "Let Go";

        killButton.onClick.RemoveAllListeners();
        letGoButton.onClick.RemoveAllListeners();

        killButton.onClick.AddListener(ChooseKill);
        letGoButton.onClick.AddListener(ChooseLetGo);

        ScenePanelManager.Instance.SetCanCloseWithKey(false);

        gameObject.SetActive(true);

        ScenePanelManager.Instance.OpenPanelWithCallback(
            chokingArt,
            "Choking Art",
            onClose: null,
            onPanelReady: () =>
            {
                Debug.Log("[KillChoicePanel] onPanelReady callback triggered! Activating choiceRoot.");
                choiceRoot.SetActive(true);
            }
        );
    }

    private void ChooseKill()
    {
        if (choiceLocked) return;
        choiceLocked = true;

        Cleanup();
        day6Sequence.OnKillChosen();
    }

    private void ChooseLetGo()
    {
        if (choiceLocked) return;
        choiceLocked = true;

        Cleanup();
        day6Sequence.OnLetGoChosen();
    }

    private void Cleanup()
    {
        choiceRoot.SetActive(false);
        ScenePanelManager.Instance.SetCanCloseWithKey(true);
        ScenePanelManager.Instance.ClosePanel();
        gameObject.SetActive(false);
    }
}