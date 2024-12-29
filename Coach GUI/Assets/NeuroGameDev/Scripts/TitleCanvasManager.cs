using UnityEngine;
using UnityEngine.UI;
using Interaxon.Libmuse;


public class TitleCanvasManager : MonoBehaviour
{
    public string nextSceneName;
    public Button startButton;
    public RectTransform landingPanel;
    public RectTransform settingsPanel;
    public RectTransform creditsPanel;

    private enum PanelState
    {
        LANDING,
        SETTINGS,
        CREDITS
    }

    private PanelState currentPanelState;

    void Start()
    {
        SwitchPanel(PanelState.LANDING);
    }

    private void Update()
    {
        if (InteraxonInterfacer.Instance.currentConnectionState == ConnectionState.CONNECTED &&
            startButton.interactable == false)
        {
            startButton.interactable = true;
        }
        else if (InteraxonInterfacer.Instance.currentConnectionState != ConnectionState.CONNECTED &&
                 startButton.interactable)
        {
            startButton.interactable = false;
        }
    }

    private void SwitchPanel(PanelState panelState)
    {
        switch (panelState)
        {
            case PanelState.LANDING:
                landingPanel.gameObject.SetActive(true);
                settingsPanel.gameObject.SetActive(false);
                break;
            case PanelState.SETTINGS:
                landingPanel.gameObject.SetActive(false);
                settingsPanel.gameObject.SetActive(true);
                break;
            case PanelState.CREDITS:
                landingPanel.gameObject.SetActive(false);
                settingsPanel.gameObject.SetActive(false);
                break;
        }
    }

    public void OnClickPlay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    public void OnClickSettings()
    {
        SwitchPanel(PanelState.SETTINGS);
    }


    public void OnClickBack()
    {
        SwitchPanel(PanelState.LANDING);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}