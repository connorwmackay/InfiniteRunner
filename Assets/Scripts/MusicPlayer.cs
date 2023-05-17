using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicPlayer : MonoBehaviour
{
    private AudioSource _audioSource;

    [SerializeField] private List<AudioClip> _audioClips = new List<AudioClip>();

    private SeedManager seedManager;

    AudioClip ChooseRandomClip()
    {
        if (_audioClips.Count > 0)
        {
            int randomIndex = seedManager.RandomRange(0, _audioClips.Count);
            return _audioClips[randomIndex];
        }
        
        return null;
    }
    
    void Start()
    {
        seedManager = GameObject.FindGameObjectWithTag("SeedManager").GetComponent<SeedManager>();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource && _audioClips.Count > 0)
        {
            _audioSource.clip = ChooseRandomClip();
            _audioSource.Play(0);
        }
    }
    
    void Update()
    {
        if (!_audioSource.isPlaying && _audioClips.Count > 0)
        {
            _audioSource.clip = ChooseRandomClip();
            _audioSource.Play(0);
        }
    }
}
