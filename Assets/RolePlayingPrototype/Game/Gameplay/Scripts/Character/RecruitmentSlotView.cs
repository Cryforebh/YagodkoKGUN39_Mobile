using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SampleProject
{
    public sealed class RecruitmentSlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _costLabel;

        private Action _onClick;

        public void Configure(RecruitmentDefinition definition, Action onClick)
        {
            ResetSlot();
            _onClick = onClick;
            _button.onClick.AddListener(HandleClick);
            _nameLabel.text = definition.DisplayName;
            _costLabel.text = $"{definition.CrystalCost} крист.";
            _icon.sprite = definition.Icon;
            _icon.enabled = definition.Icon != null;
        }

        public void SetAvailable(bool available)
        {
            _button.interactable = available;
            _costLabel.color = available ? Color.white : new Color(1f, 0.45f, 0.45f, 1f);
        }

        public void ResetSlot()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }

            _onClick = null;
        }

        private void HandleClick()
        {
            _onClick?.Invoke();
        }
    }
}
