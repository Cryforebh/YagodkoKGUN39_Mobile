using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private const string GameSceneName = "Game";

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        ApplyScene(SceneManager.GetActiveScene());
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene currentScene)
    {
        ApplyScene(currentScene);
    }

    private void ApplyScene(Scene scene)
    {
        if (scene.name == GameSceneName)
        {
            PlayGameMusic();
            return;
        }

        StopMusic();
    }

    private void PlayGameMusic()
    {
        // При Restart источник уже играет,
        // поэтому композиция не запускается повторно.
        if (!_audioSource.isPlaying)
            _audioSource.Play();
    }

    private void StopMusic()
    {
        if (_audioSource.isPlaying)
            _audioSource.Stop();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
