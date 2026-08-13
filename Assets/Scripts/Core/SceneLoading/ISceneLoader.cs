using Cysharp.Threading.Tasks;

public interface ISceneLoader
{
    UniTask LoadMain();
    UniTask LoadGame();
    UniTask RestartGame();
}
