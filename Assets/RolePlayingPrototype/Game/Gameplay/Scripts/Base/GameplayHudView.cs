using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SampleProject.Base
{
    public sealed class GameplayHudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _mineralsLabel;
        [SerializeField] private TextMeshProUGUI _woodLabel;
        [SerializeField] private TextMeshProUGUI _crystalsLabel;
        [SerializeField] private Button _patrolButton;
        [SerializeField] private GameObject _editButtons;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _clearButton;

        public TextMeshProUGUI MineralsLabel => _mineralsLabel;
        public TextMeshProUGUI WoodLabel => _woodLabel;
        public TextMeshProUGUI CrystalsLabel => _crystalsLabel;
        public Button PatrolButton => _patrolButton;
        public GameObject EditButtons => _editButtons;
        public Button UndoButton => _undoButton;
        public Button ApplyButton => _applyButton;
        public Button ClearButton => _clearButton;

        public void Setup(TextMeshProUGUI mineralsLabel, TextMeshProUGUI woodLabel, TextMeshProUGUI crystalsLabel, Button patrolButton, GameObject editButtons, Button undoButton, Button applyButton, Button clearButton)
        {
            _mineralsLabel = mineralsLabel;
            _woodLabel = woodLabel;
            _crystalsLabel = crystalsLabel;
            _patrolButton = patrolButton;
            _editButtons = editButtons;
            _undoButton = undoButton;
            _applyButton = applyButton;
            _clearButton = clearButton;
        }
    }
}
