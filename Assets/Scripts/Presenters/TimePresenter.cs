using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class TimePresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _timeText;

    [Inject] private ITimeModel _timeModel;

    private readonly CompositeDisposable _disposables = new();

    private void Start()
    {
        _timeModel.Time
            .Subscribe(UpdateTime)
            .AddTo(_disposables);

        _timeModel.StartTimer().Forget();
    }

    private void UpdateTime(int time)
    {
        _timeText.text = $"T: {time.ToString()}";
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
