using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField]
    private AudioClip _playButtonSound;

    [SerializeField]
    private AudioSource _menuMusicSource;

    private ISceneLoader _sceneLoader;
    private SfxPlayer _sfxPlayer;

    private bool _isStartingGame;

    [Inject]
    private void Construct(
        ISceneLoader sceneLoader,
        SfxPlayer sfxPlayer)
    {
        _sceneLoader = sceneLoader;
        _sfxPlayer = sfxPlayer;
    }

    public void Play()
    {
        if (_isStartingGame)
            return;

        _isStartingGame = true;

        StopMenuMusic();

        _sfxPlayer.PlayOneShot(_playButtonSound);

        _sceneLoader.LoadGame().Forget();
    }

    private void StopMenuMusic()
    {
        if (_menuMusicSource != null && _menuMusicSource.isPlaying)
        {
            _menuMusicSource.Stop();
        }
    }
}
