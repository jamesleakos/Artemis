using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawwheel : MonoBehaviour {

    GameMaster gm;
    AudioManager audioManager;
    AudioSource buzzsound;

    public float defaultVolume;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        //buzzsound = gameObject.GetComponent<AudioSource>();
        //buzzsound.volume = defaultVolume * PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume");
    }

    void LateStart() {

    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            Player player = collider.gameObject.GetComponent<Player>();
            if (player.artemisState == Player.ArtemisState.alive) {
                player.HitByEnemy();

                audioManager.PlaySound("Slice");
            }
        } else {
            gm.DestroyCharacter(collider.gameObject);
        }
    }

    //void OnEnable() {
    //    AudioManager.OnSFXSet += SetVolume;
    //}
    //void OnDisable() {
    //    AudioManager.OnSFXSet -= SetVolume;
    //}

    //void SetVolume(float setVolume, float mainVolume) {
    //    print(buzzsound.volume);
    //    print(defaultVolume);
    //    print(setVolume);
    //    print(mainVolume);

    //    buzzsound.volume = defaultVolume * setVolume * mainVolume;
    //}
}
