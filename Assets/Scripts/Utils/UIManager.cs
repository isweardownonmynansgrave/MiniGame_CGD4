using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    #region Instanzvariablen
    // Singleton
    public static UIManager Instance { get; private set; }

    // Main Menu References
    private Button joinButton;
    private Button settingsButton;
    private Button quitButton;
    private GameObject UIDocMainMenuObj;
    private GameObject UIDocSettingsObj;
    private TextField usernameField;

    // Splash Screen References
    ETeam teamChoice;
    private GameObject UIDocSplashObj;

    // Ingame References
    private GameObject UIDocGUIObj;
    private Label scoreLabel;
    #endregion

    // Accesssor
    public string Username => usernameField.value;
    public ETeam TeamChoice => teamChoice;

    #region MonoBehaviour-Methoden
    private void Awake()
    {
        try
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[UIManagerV2] Fehler in Awake(): {ex.Message}");
        }
    }
    void Start()
    {

    }

    void Update()
    {
        // cheesy-temporary, ig
        if (Input.GetKeyDown(KeyCode.Escape))
            OnSettingsEscPress();
    }
    #endregion

    #region Scenehandling
    private void OnSceneChanged(Scene p, Scene n)
    {
        // Cleanup
        UnsetAllUIRefs();

        // Neues Scenehandling
        switch (n.name)
        {
            case "0_MainMenu":
                UIDocMainMenuObj = GameObject.Find("UIDoc_MainMenu");
                UIDocSettingsObj = GameObject.Find("UIDoc_SettingsMenu");
                SetupMainMenu(UIDocMainMenuObj.GetComponent<UIDocument>().rootVisualElement);
                SetupSettingsMenu(UIDocSettingsObj.GetComponent<UIDocument>().rootVisualElement);
                break;
            case "1_Splash":
                UIDocSplashObj = GameObject.Find("UIDoc_SplashTeamScreen");
                break;
            case "GameScene":
                Debug.Log("");
                UIDocGUIObj = GameObject.Find("UIDoc_GUI");
                break;
        }
    }

    private void SetupMainMenu(VisualElement root)
    {
        try
        {
            joinButton = root.Q<Button>("join-button");
            settingsButton = root.Q<Button>("settings-button");
            quitButton = root.Q<Button>("quit-button");

            // Funktionalität der Buttons
            if (joinButton != null)
            {
                joinButton.clicked += () => SceneManager.LoadScene(1);
            }
            if (settingsButton != null)
            {
                settingsButton.clicked += () => ToggleSettingsMenu();
            }
            if (quitButton != null)
            {
                quitButton.clicked += () => Application.Quit();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UIManager] Fehler beim Setup des Hauptmenüs: {ex.Message}");
        }
    }
    private void SetupSettingsMenu(VisualElement root)
    {

    }

    // Settings Menu anfangs im Hauptmenü zugänglich
    private void ToggleSettingsMenu()
    {
        UIDocSettingsObj.SetActive(!UIDocSettingsObj.activeSelf);
        UIDocMainMenuObj.SetActive(!UIDocMainMenuObj.activeSelf);
    }
    public void OnSettingsEscPress() => ToggleSettingsMenu(); // Öffentliche Methode für externen Zugriff & saubere Struktur
    #endregion

    #region Hilfsmethoden
    private void UnsetAllUIRefs()
    {
        //Main Menu
        joinButton = null;
        settingsButton = null;
        quitButton = null;
        UIDocMainMenuObj = null;
        UIDocSettingsObj = null;

        // Splash
        UIDocSplashObj = null;

        // Ingame
        UIDocGUIObj = null;
    }
    #endregion
}
