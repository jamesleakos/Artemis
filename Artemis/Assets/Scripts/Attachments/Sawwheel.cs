using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawwheel : MonoBehaviour {

    GameMaster gm;
    AudioManager audioManager;
    AudioSource buzzsound;

    float bufferDistance = 10;

    #region Player Target 
    // get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 0.5f;
    #endregion

    public float defaultVolume;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        buzzsound = gameObject.GetComponent<AudioSource>();
        buzzsound.volume = defaultVolume * PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume");
    }

    void Update() {
        if (playerTransform == null) {
            FindPlayer();
        }
        if (playerTransform != null) {
            if ((playerTransform.position - transform.position).magnitude < buzzsound.maxDistance + bufferDistance) {
                if (!buzzsound.isPlaying) {
                    buzzsound.Play();
                }
            } else {
                if (buzzsound.isPlaying) {
                    buzzsound.Stop();
                }
            }
        }
    }

    void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
            if (searchResult != null) {
                player = searchResult.GetComponent<Player>();
                playerTransform = searchResult.GetComponent<Transform>();
            }
            nextTimeToSearch = Time.time + searchInterval;
        }
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
