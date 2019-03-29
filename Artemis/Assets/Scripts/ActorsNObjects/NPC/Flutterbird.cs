using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flutterbird : MonoBehaviour
{

    AudioSource chirping;
    public float defaultVolume = 1;
    public float defaultPitch = 1;
    public float randomVolume = 0.3f;
    public float randomPitch = 0.3f;

    float nextTimeToCheckVolume;
    float volumeCheckInterval = 1f;

    #region Player Target 
    // get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 1.2f;
    #endregion

    void Start()
    {
        chirping = gameObject.GetComponent<AudioSource>();
        chirping.volume = defaultVolume * PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume");
    }

    void Update() {
        if (nextTimeToCheckVolume < Time.time && chirping.isPlaying) {
            chirping.volume = defaultVolume * PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume");
            nextTimeToCheckVolume = Time.time + volumeCheckInterval + Random.Range(0,0.3f);
        }
        if (playerTransform == null) {
            FindPlayer();
        }
        if (playerTransform != null) {
            if ((playerTransform.position - transform.position).magnitude < chirping.maxDistance + 5f) {
                if (!chirping.isPlaying) {
                    chirping.volume = defaultVolume * (1 + Random.Range(-randomVolume / 2f, randomVolume / 2f)) * PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume");
                    chirping.pitch = defaultPitch * (1 + Random.Range(-randomPitch / 2f, randomPitch / 2f));
                    chirping.Play();
                }
            } else {
                if (chirping.isPlaying) {
                    chirping.Stop();
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

    //void OnEnable() {
    //    AudioManager.OnSFXSet += SetVolume;
    //}
    //void OnDisable() {
    //    AudioManager.OnSFXSet -= SetVolume;
    //}

    //void SetVolume(float setXMainVoume) {
    //    print(chirping.volume);
    //    print(defaultVolume);
    //    print(setXMainVoume);

    //    chirping.volume = defaultVolume * setXMainVoume;
    //}
}
