using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour {

    public static GameMaster gm;
    private MenuSystem menuSystem;
    public GameObject mask;

    #region Difficulty Settings
    public enum GameDifficulty { littlegirl, shepherdess, goddess };
    public GameDifficulty goalDifficulty;
    public GameDifficulty gameDifficulty;
    #endregion

    #region Key Codes
    public KeyCode left { get; set; }
    public KeyCode right { get; set; }
    public KeyCode interact { get; set; }
    public KeyCode useItem { get; set; }
    public KeyCode jump { get; set; }
    public KeyCode pause { get; set; }
    public KeyCode fireDash { get; set; }
    #endregion

    #region Loading Levels

    public int levelPaddingBesidesMain;

    public int levelToLoad;
    public bool displayLevelOpenText;
    #endregion

    #region Spawns
    public Transform playerPrefab;
    public Vector3 spawnPoint;
    public float spawnDelay = 2;
    public bool spawnReached;

    public enum RespawnState { respawn, reset };
    public RespawnState respawnState;
    #endregion

    #region Misc GM Tracking
    public bool loadSplashScreen = true;
    #endregion

    // public CameraShake cameraShake;

    #region Start, Update, OnSceneLoaded, OnEnable, OnDisable
    void Start() {
        Application.targetFrameRate = 60;
        displayLevelOpenText = true;

        if (gm != null) {
            if (gm != this) {
                Destroy(this.gameObject);
            }
        } else {
            gm = this;
            DontDestroyOnLoad(this);
        }

        left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("leftKey", "A"));
        right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("rightKey", "D"));
        interact = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("interactKey", "W"));
        useItem = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("useItemKey", "S"));
        jump = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("jumpKey", "Mouse1"));
        pause = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("pauseKey", "Space"));
        fireDash = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("fireDashKey", "Mouse0"));
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {

        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
        spawnReached = false;

    }
    #endregion

    #region Loading Levels and Respawn Player

    public void LoadOrRespawn() {
        if (gm.respawnState == GameMaster.RespawnState.respawn) {
            gm.RespawnPlayer();
            menuSystem.fadeMaskController.LightenMask();
        } else if (gm.respawnState == GameMaster.RespawnState.reset) {
            LoadLevel();
        }
    }

    #region Loading Levels
    //Main
    public void SetLoadLevel(int levelInt) {
        levelToLoad = levelInt;
        menuSystem.FadeInEffect();
        respawnState = RespawnState.reset;
    }
    public void LoadLevel() {
        if (!displayLevelOpenText || levelToLoad == 0) {
            SceneManager.LoadScene(levelToLoad);
            displayLevelOpenText = true;
        } else {
            SceneManager.LoadScene(1);
        }
    }
    public void SetDisplayLevelOpenText(bool setTo) {
        displayLevelOpenText = setTo;
    }

    //Convenience Roll-Ups
    public void ReplayLevel() {
        displayLevelOpenText = false;
        gm.SetLoadLevel(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextLevel() {
        if (SceneManager.sceneCountInBuildSettings == SceneManager.GetActiveScene().buildIndex + 1) {
            gm.SetLoadLevel(0);
        } else {
            gm.SetLoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    #endregion

    #region Respawns
    //Main
    public void SetRespawnPlayer() {
        menuSystem.FadeInEffect();
        respawnState = RespawnState.respawn;
    }
    public void RespawnPlayer() {
        Transform playerClone = Instantiate(playerPrefab, spawnPoint, new Quaternion(0f, 0f, 0f, 0f));
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            playerClone.gameObject.GetComponent<Player>().muteSound = true;
        }
    }

    //Updating Spawns
    public void UpdateSpawn(Vector3 newSpawn) {
        spawnReached = true;
        spawnPoint = newSpawn;
    }
    public void ResetSpawn() {
        spawnReached = false;
    }
    #endregion

    #endregion

    #region Killing Things

    public void KillPlayer(Player player) {
        Destroy(player.gameObject);
        displayLevelOpenText = false;
        if (spawnReached) {
            SetRespawnPlayer();
        } else {
            gm.SetLoadLevel(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public static void KillEnemy(GameObject enemy) {
        gm._KillEnemy(enemy);
    }
    public void _KillEnemy(GameObject _enemy) {

        // GameObject _clone = Instantiate(_deer.deathParticles, _deer.transform.position, Quaternion.identity) as GameObject;
        // Destroy(_clone, 5f);
        // cameraShake.Shake(_deer.shakeAmt, _deer.shakeLength);
        Destroy(_enemy);
    }

    public void DestroyCharacter(GameObject character) {
        if (character.tag == "Deer") {
            Deer deer = character.GetComponent<Deer>();
            //KillDeer(deer);
        } else if (character.tag == "Player") {
            Debug.Log("Destroy Player - Player Identified");
            Player player = character.GetComponent<Player>();
            KillPlayer(player);
        } else if (character.tag == "Arrow") {
            Destroy(character);
        } else if (character.tag == "ChargerEnemy" || character.tag == "RangedEnemy") {
            Destroy(character);
        }
    }

    #endregion

    public void QuitGame() {
        print("this will quit");
        Application.Quit();
    }

    #region Relevent Text

    public string ReturnSceneNames(int sceneNumber) {
        sceneNumber = sceneNumber - levelPaddingBesidesMain;
        switch (sceneNumber) {
            case 1: return "Chapter " + sceneNumber.ToString() + ":\n" + "A Stroll Through Olympia";
            case 2: return "Chapter " + sceneNumber.ToString() + ":\n" + "The Climb";
            case 3: return "Chapter " + sceneNumber.ToString() + ":\n" + "The Knot";
            case 4: return "Chapter " + sceneNumber.ToString() + ":\n" + "Labyrinth";
            case 5: return "Chapter " + sceneNumber.ToString() + ":\n" + "The Sisyphean Squeeze";
            case 6: return "Chapter " + sceneNumber.ToString() + ":\n" + "The Underroad";
            case 7: return "Chapter " + sceneNumber.ToString() + ":\n" + "Path to Oblivion";
            case 8: return "Chapter " + sceneNumber.ToString() + ":\n" + "Circuitous Route";
            case 9: return "Chapter " + sceneNumber.ToString() + ":\n" + "Briar Isles";
            case 10: return "Chapter " + sceneNumber.ToString() + ":\n" + "Olympian Idyll";
            default: return "Need to Add more to Return Scene Names";
        }
    }
    public string ReturnSceneIntroText(int sceneNumber) {
        sceneNumber = sceneNumber - levelPaddingBesidesMain;
        switch (sceneNumber) {
            case 0:
                return "Young girls should never argue," + "\n" +
                        "Raise their voice, or cry." + "\n" +
                        "If you disagree, just smile -" + "\n" +
                        "And put an arrow through their eye.";
            case 1: return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 2:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 3:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 4:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 5:
                return "Young girls should never argue," + "\n" +
                 "Raise their voice, or cry." + "\n" +
                 "If you disagree, just smile -" + "\n" +
                 "And put an arrow through their eye.";
            case 6:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 7:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 8:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";
            case 9:
                return "Young girls should never argue," + "\n" +
                "Raise their voice, or cry." + "\n" +
                "If you disagree, just smile -" + "\n" +
                "And put an arrow through their eye.";

            default:
                return "Young girls should never argue," + "\n" +
           "Raise their voice, or cry." + "\n" +
           "If you disagree, just smile -" + "\n" +
           "And put an arrow through their eye.";
        }
    }
    #endregion


}
