using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private AudioSource _atmosphereSource;
    [SerializeField] private AudioClip _atmosphereClip;

    private const string GameSceneName = "Game";

    private AudioSource _audioSource;
    private int _currentClipIndex = -1;
    private bool _musicEnabled;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _audioSource.loop = false;
        _audioSource.playOnAwake = false;

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        ApplyScene(SceneManager.GetActiveScene());
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene currentScene)
    {
        ApplyScene(currentScene);
    }

    private void Update()
    {
        //if (!_musicEnabled)
        //    return;

        // Текущая композиция закончилась.
        if (_musicEnabled && !_audioSource.isPlaying)
            PlayNextTrack();

        PlayGameAtmosphereSound();
    }

    private void ApplyScene(Scene scene)
    {
        if (scene.name == GameSceneName)
        {
            PlayGameAtmosphereSound();
            PlayGameMusic();
            return;
        }

        StopAtmosphereSound();
        StopMusic();
    }

    private void PlayGameMusic()
    {
        // При Restart музыка уже играет,
        // поэтому не запускается заново.
        if (_audioSource.isPlaying)
            return;

        PlayNextTrack();
    }

    private void PlayGameAtmosphereSound()
    {
        if (_atmosphereSource.isPlaying)
            return;

        _atmosphereSource.clip = _atmosphereClip;
        _atmosphereSource.Play();
    }

    private void PlayNextTrack()
    {
        if (_clips == null || _clips.Length == 0)
            return;

        int nextClipIndex = GetRandomClipIndex();

        if (nextClipIndex < 0)
            return;

        _currentClipIndex = nextClipIndex;
        _audioSource.clip = _clips[_currentClipIndex];
        _audioSource.Play();
    }

    private int GetRandomClipIndex()
    {
        if (_clips.Length == 1)
            return _clips[0] != null ? 0 : -1;

        int validCandidatesCount = 0;

        for (int i = 0; i < _clips.Length; i++)
        {
            if (i != _currentClipIndex && _clips[i] != null)
                validCandidatesCount++;
        }

        if (validCandidatesCount == 0)
            return -1;

        int randomCandidate =
            Random.Range(0, validCandidatesCount);

        for (int i = 0; i < _clips.Length; i++)
        {
            if (i == _currentClipIndex || _clips[i] == null)
                continue;

            if (randomCandidate == 0)
                return i;

            randomCandidate--;
        }

        return -1;
    }

    private void StopMusic()
    {
        _musicEnabled = false;
        _audioSource.Stop();
    }

    private void StopAtmosphereSound()
    {
        _atmosphereSource.Stop();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
