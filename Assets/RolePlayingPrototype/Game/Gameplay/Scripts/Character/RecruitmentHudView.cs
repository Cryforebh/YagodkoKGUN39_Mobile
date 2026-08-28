using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SampleProject
{
    public sealed class RecruitmentHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RecruitmentSlotView _slotTemplate;
        [SerializeField] private Button _rallyPointButton;
        [SerializeField] private TextMeshProUGUI _rallyPointLabel;

        public GameObject Panel => _panel;
        public RectTransform Content => _content;
        public RecruitmentSlotView SlotTemplate => _slotTemplate;
        public Button RallyPointButton => _rallyPointButton;
        public TextMeshProUGUI RallyPointLabel => _rallyPointLabel;
    }
}
