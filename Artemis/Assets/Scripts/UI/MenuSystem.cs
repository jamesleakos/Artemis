using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour {
    // Menu System Code!

    #region GameObjects
    [Header("Objects Settings")]
    [Space]
    public GameObject fadeMask;
    public MaskController fadeMaskController;
    public GameObject distanceFog;
    public GameObject splashScreen;
    public GameObject mainScreen;
    public GameObject pauseScreen;
    public GameObject levelSelection;
    public GameObject settingsScreen;
    public Transform buttonConfig;
    public Transform volumeConfig;
    public Transform otherSettingsConfig;
    public GameObject timer;

    #endregion

    #region Other Vars

    [Header("Visual Settings")]
    [Space]

    private int levelToLoad;

    public enum CurrentMenuScreen { splash, main, pause, win, levelSelect, settings, none };
    public CurrentMenuScreen currentMenuScreen;

    float nextTimeToSearch;
    #endregion

    #region Vars for Key Binding

    Event keyEvent;
    Text buttonText;
    string keyToName;
    KeyCode newKey;
    bool waitingForKey = false;

    #endregion

    Player targetPlayer;
    CameraZoom camZoom;
    Transform camera;
    AudioManager audioManager;
    public GameMaster gm;

    // Start and Update
    #region Start and Update, On Enable/Diable, On Screen Loaded, etc.

    void Start() {

        camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        camZoom = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraZoom>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        fadeMaskController = fadeMask.GetComponent<MaskController>();

        basicSettings();
        SetPlayerPrefs();
    }

    void SetPlayerPrefs() {
        #region Key Bindings
        for (int i = 0; i < buttonConfig.childCount; i++) {

            if (buttonConfig.GetChild(i).name == "SetRightButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.right.ToString();

            else if (buttonConfig.GetChild(i).name == "SetLeftButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.left.ToString();

            else if (buttonConfig.GetChild(i).name == "SetInteractButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.interact.ToString();

            else if (buttonConfig.GetChild(i).name == "SetUseItemButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.useItem.ToString();

            else if (buttonConfig.GetChild(i).name == "SetDashFireButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.fireDash.ToString();

            else if (buttonConfig.GetChild(i).name == "SetJumpButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.jump.ToString();

            else if (buttonConfig.GetChild(i).name == "SetPauseButton")
                buttonConfig.GetChild(i).GetComponentInChildren<Text>().text = gm.pause.ToString();
        }
        #endregion

        #region Volume
        for (int i = 0; i < volumeConfig.childCount; i++) {

            if (volumeConfig.GetChild(i).name == "MainVolume") {
                volumeConfig.GetChild(i).Find("Slider").GetComponentInChildren<Slider>().value = PlayerPrefs.GetFloat("MainVolume");
            }

            if (volumeConfig.GetChild(i).name == "MusicVolume") {
                volumeConfig.GetChild(i).Find("Slider").GetComponentInChildren<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
            }

            if (volumeConfig.GetChild(i).name == "SFXVolume") {
                volumeConfig.GetChild(i).Find("Slider").GetComponentInChildren<Slider>().value = PlayerPrefs.GetFloat("SFXVolume");
            }
                

        }
        #endregion

        #region Other Settings
        for (int i = 0; i < otherSettingsConfig.childCount; i++) {

            if (otherSettingsConfig.GetChild(i).name == "CameraZoomScale") {
                otherSettingsConfig.GetChild(i).Find("Slider").GetComponentInChildren<Slider>().value = PlayerPrefs.GetFloat("CameraZoomScale",1f);
            }
        }
        #endregion
    }

    private void OnGUI() {
        if (waitingForKey) {
            if (Input.anyKeyDown) {
                if (Event.current.isKey) {
                    newKey = Event.current.keyCode;
                    KeyBindingOff();
                } else if (Event.current.isMouse) {
                    if (Input.GetMouseButtonDown(0))
                        newKey = KeyCode.Mouse0;
                    if (Input.GetMouseButtonDown(1))
                        newKey = KeyCode.Mouse1;
                    if (Input.GetMouseButtonDown(2))
                        newKey = KeyCode.Mouse2;
                    KeyBindingOff();
                }
            }
        }
    }

    void Update() {
        if (targetPlayer == null) {
            FindPlayer();
        }

        StateFunctions();

        quitLevelSelectionScreen();
        openPauseMenu();
        quitSettingsScreen();
    }
    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            currentMenuScreen = CurrentMenuScreen.main;
        } else {
            currentMenuScreen = CurrentMenuScreen.none;
        }
    }
    #endregion

    #region Settings Changes

    public void KeyBindingOn(string keyString) {

        SetAssignKey(keyString);

        MoveSettingsLeft();
        waitingForKey = true;
    }

    public void KeyBindingOff() {

        AssignKey(keyToName);

        MoveSettingsRight();
        waitingForKey = false;
    }

    void MoveSettingsLeft() {
        settingsScreen.GetComponentInChildren<LevelScroller>().MoveLeft();
    }

    void MoveSettingsRight() {
        settingsScreen.GetComponentInChildren<LevelScroller>().MoveRight();
    }

    void SetAssignKey(string keyString) {
        keyToName = keyString;
    }

    void AssignKey(string keyName) {

        switch (keyName) {
            case "left":
                GameMaster.gm.left = newKey; //Set forward to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetLeftButton").GetComponentInChildren<Text>().text = GameMaster.gm.left.ToString(); //Set button text to new key
                PlayerPrefs.SetString("SetLeftButton", GameMaster.gm.left.ToString()); //save new key to PlayerPrefs
                break;

            case "right":
                GameMaster.gm.right = newKey; //set backward to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetRightButton").GetComponentInChildren<Text>().text = GameMaster.gm.right.ToString(); //set button text to new key
                PlayerPrefs.SetString("SetRightButton", GameMaster.gm.right.ToString()); //save new key to PlayerPrefs
                break;

            case "interact":
                GameMaster.gm.interact = newKey; //set left to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetInteractButton").GetComponentInChildren<Text>().text = GameMaster.gm.interact.ToString(); //set button text to new key
                PlayerPrefs.SetString("SetInteractButton", GameMaster.gm.interact.ToString()); //save new key to playerprefs
                break;

            case "useItem":
                GameMaster.gm.useItem = newKey; //set right to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetUseItemButton").GetComponentInChildren<Text>().text = GameMaster.gm.useItem.ToString(); //set button text to new key
                PlayerPrefs.SetString("SetUseItemButton", GameMaster.gm.useItem.ToString()); //save new key to playerprefs
                break;

            case "fireDash":
                GameMaster.gm.fireDash = newKey; //set jump to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetDashFireButton").GetComponentInChildren<Text>().text = GameMaster.gm.fireDash.ToString(); //set button text to new key
                PlayerPrefs.SetString("SetDashFireButton", GameMaster.gm.fireDash.ToString()); //save new key to playerprefs
                break;

            case "jump":
                GameMaster.gm.jump = newKey; //set jump to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetJumpButton").GetComponentInChildren<Text>().text = GameMaster.gm.jump.ToString(); //set button text to new key
                PlayerPrefs.SetString("SetJumpButton", GameMaster.gm.jump.ToString()); //save new key to playerprefs
                break;

            case "pause":
                GameMaster.gm.pause = newKey; //set jump to new keycode
                TransformDeepChildExtension.FindDeepChild(gameObject.transform, "SetPauseButton").GetComponentInChildren<Text>().text = GameMaster.gm.pause.ToString(); //set button text to new key
                PlayerPrefs.SetString("SetPauseButton", GameMaster.gm.pause.ToString()); //save new key to playerprefs
                break;
        }
    }

    public void SetMain(float volumeScale) {
        audioManager.SetMain(volumeScale);
    }

    public void SetMusic(float volumeScale) {
        audioManager.SetMusic(volumeScale);
    }

    public void SetSFX(float volumeScale) {
        audioManager.SetSFX(volumeScale);
    }

    public void SetCameraZoomPref(float CameraZoomScale) {
        camZoom.SetNewZoomSize(CameraZoomScale);
    }

    #endregion

    #region Player Functions
    void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
            if (searchResult != null) {
                targetPlayer = searchResult.GetComponent<Player>();
            }
            nextTimeToSearch = Time.time + 0.5f;
        }
    }
    #endregion

    #region State Functions
    void StateFunctions() {
        if (currentMenuScreen == CurrentMenuScreen.none) {
            if (targetPlayer != null) {
                targetPlayer.inputOnUIScreen = true;
            }
            camZoom.ResetZoom("State Functions - none");

        } else if (currentMenuScreen == CurrentMenuScreen.main) {
            if (targetPlayer != null) {
                targetPlayer.inputOnUIScreen = true;
            }
            camZoom.ResetZoom("State Functions - none");

        } else if (currentMenuScreen == CurrentMenuScreen.pause) {
            if (targetPlayer != null) {
                targetPlayer.inputOnUIScreen = false;
            }
            camZoom.ZoomCameraOut();

        } else if (currentMenuScreen == CurrentMenuScreen.levelSelect) {
            if (targetPlayer != null) {
                targetPlayer.inputOnUIScreen = false;
            }
            camZoom.ZoomCameraOut();

        } else if (currentMenuScreen == CurrentMenuScreen.settings) {
            if (targetPlayer != null) {
                targetPlayer.inputOnUIScreen = false;
            }
            camZoom.ZoomCameraOut();
            SetPlayerPrefs();
        } else if (currentMenuScreen == CurrentMenuScreen.splash) {
            if (targetPlayer != null) {
                targetPlayer.inputOnUIScreen = false;
            }
            camZoom.ZoomCameraOut();
            watchSplashScreen();
        }
    }
    #endregion

    #region Clear Menus
    void basicSettings() {
        ResetMenus();
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            if (gm.loadSplashScreen) {
                setSplashScreen(true);
            } else {
                setMainMenu(true);
            }
        } else {
            currentMenuScreen = CurrentMenuScreen.none;

            fadeMask.SetActive(true);
            fadeMaskController.LightenMask();
        }
    }

    void ResetMenus() {
        fadeMask.SetActive(true);
        levelSelection.SetActive(false);
        pauseScreen.SetActive(false);
        mainScreen.SetActive(false);
        settingsScreen.SetActive(false);
    }
    #endregion

    #region Mask Work
    // Fade Effect
    public void fadeInEffect() {
        fadeMask.SetActive(true);
        fadeMaskController.DarkenMask();
        // animator on fade mask takes over from here
    }
    #endregion

    #region Loading Levels - refs to GM
    public void setLevelToLoad(int levelInt) {
        gm.SetLoadLevel(levelInt);
    }
    public void LoadOrRespawn() {
        gm.LoadOrRespawn();
    }
    #endregion

    // Screen Specific Code
    #region Main Menu
    public void setMainMenu(bool value) {
        if (value) {
            currentMenuScreen = CurrentMenuScreen.main;
            mainScreen.SetActive(true);
        } else {
            mainScreen.SetActive(false);
        }
    }
    #endregion

    #region PauseMenu
    public void setPauseMenu(bool value) {
        if (value) {
            currentMenuScreen = CurrentMenuScreen.pause;
            pauseScreen.SetActive(true);
        } else {
            pauseScreen.SetActive(false);
            currentMenuScreen = CurrentMenuScreen.none;
        }
    }

    void openPauseMenu() {
        if (SceneManager.GetActiveScene().buildIndex > 0) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (currentMenuScreen == CurrentMenuScreen.none) {
                    setPauseMenu(true);
                } else if (currentMenuScreen == CurrentMenuScreen.pause) {
                    setPauseMenu(false);
                }
            }
        }
    }
    #endregion

    #region Splash Screen
    public void setSplashScreen(bool value) {
        if (value) {
            currentMenuScreen = CurrentMenuScreen.splash;
            splashScreen.SetActive(true);
        } else {
            splashScreen.SetActive(false);
            currentMenuScreen = CurrentMenuScreen.none;
        }
    }

    void watchSplashScreen() {
        if (Input.anyKeyDown) {
            if (currentMenuScreen == CurrentMenuScreen.splash) {
                setSplashScreen(false);
                setMainMenu(true);
            }
        }
    }
    #endregion

    #region LevelSelection
    #region Setting Selection Pane
    public void setLevelSelection(bool value) {
        if (value) {
            ResetMenus();
            levelSelection.SetActive(true);
            currentMenuScreen = CurrentMenuScreen.levelSelect;
        } else {
            levelSelection.SetActive(false);
            setMainMenu(true);
        }
    }

    void quitLevelSelectionScreen() {
        if (currentMenuScreen == CurrentMenuScreen.levelSelect) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                setLevelSelection(false);
            }
        }
    }
    #endregion

    
    #endregion

    #region Settings
    public void setSettings(bool value) {
        if (value) {
            ResetMenus();
            settingsScreen.SetActive(true);
            currentMenuScreen = CurrentMenuScreen.settings;
        } else {
            settingsScreen.SetActive(false);
        }
    }

    void quitSettingsScreen() {
        if (currentMenuScreen == CurrentMenuScreen.settings) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (SceneManager.GetActiveScene().buildIndex == 0) {
                    setMainMenu(true);
                    setSettings(false);
                } else {
                    setSettings(false);
                    setPauseMenu(true);
                }
            }
        }
    }
    #endregion

    #region Timer
    public void SetTimer (bool start) {
        if (start) {
            timer.GetComponent<Timer>().StartTimer();
        } else {
            timer.GetComponent<Timer>().EndTimer();
        }
    }
    #endregion

    #region Links to GM
    public void QuitGame() {
        GameMaster.gm.QuitGame();
    }
    #endregion

}