using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using SampleProject.Base;
using UniRx;
using UnityEngine;
using Zenject;

namespace SampleProject
{
    public sealed class RecruitmentHudPresenter : IInitializable, IDisposable
    {
        private readonly List<SlotEntry> _slots = new();
        private readonly CompositeDisposable _subscriptions = new();
        private readonly IRecruitmentService _recruitment;
        private readonly IResourceStorage _resources;
        private readonly RecruitmentHudView _view;
        private readonly IGameObjectPool _pool;
        private string _slotPoolId;

        public RecruitmentHudPresenter(IRecruitmentService recruitment, IResourceStorage resources, RecruitmentHudView view, IGameObjectPool pool)
        {
            _recruitment = recruitment;
            _resources = resources;
            _view = view;
            _pool = pool;
        }

        public void Initialize()
        {
            _slotPoolId = "RecruitmentSlot:" + _view.SlotTemplate.GetInstanceID();
            _view.SlotTemplate.gameObject.SetActive(false);
            _view.Panel.SetActive(false);
            _view.RallyPointButton.onClick.AddListener(_recruitment.ToggleRallyPointPlacement);
            _recruitment.SelectedBuilding.Subscribe(ShowBuilding).AddTo(_subscriptions);
            _recruitment.IsSettingRallyPoint.Subscribe(RefreshRallyPointButton).AddTo(_subscriptions);
            _resources.Get(ResourceType.Crystals).Subscribe(_ => RefreshAvailability()).AddTo(_subscriptions);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            if (_view != null && _view.RallyPointButton != null)
            {
                _view.RallyPointButton.onClick.RemoveListener(_recruitment.ToggleRallyPointPlacement);
            }
            ReleaseSlots();
        }

        private void ShowBuilding(RecruitmentBuilding building)
        {
            ReleaseSlots();
            if (building == null || !building.isActiveAndEnabled)
            {
                _view.Panel.SetActive(false);
                return;
            }

            _view.Panel.SetActive(true);
            var options = building.AvailableUnits;
            if (options == null)
            {
                return;
            }

            for (var index = 0; index < options.Length; index++)
            {
                var definition = options[index];
                if (definition == null || definition.Prefab == null)
                {
                    continue;
                }

                var slotObject = _pool.Get(
                    _slotPoolId,
                    () => UnityEngine.Object.Instantiate(_view.SlotTemplate.gameObject),
                    _view.Content,
                    instance => instance.GetComponent<RecruitmentSlotView>().Configure(definition, () => _recruitment.TryRecruit(definition))
                );
                var slot = slotObject.GetComponent<RecruitmentSlotView>();
                _slots.Add(new SlotEntry(slot, definition));
            }

            RefreshAvailability();
            RefreshRallyPointButton(_recruitment.IsSettingRallyPoint.Value);
        }

        private void RefreshRallyPointButton(bool isSetting)
        {
            if (_view == null || _view.RallyPointButton == null)
            {
                return;
            }

            _view.RallyPointButton.interactable = _recruitment.SelectedBuilding.Value != null;
            _view.RallyPointLabel.text = isSetting ? "Отмена" : "Точка сбора";
        }

        private void RefreshAvailability()
        {
            for (var index = 0; index < _slots.Count; index++)
            {
                var slot = _slots[index];
                slot.View.SetAvailable(_recruitment.CanRecruit(slot.Definition));
            }
        }

        private void ReleaseSlots()
        {
            for (var index = 0; index < _slots.Count; index++)
            {
                var slot = _slots[index].View;
                if (slot == null)
                {
                    continue;
                }

                slot.ResetSlot();
                _pool.Release(_slotPoolId, slot.gameObject);
            }

            _slots.Clear();
        }

        private readonly struct SlotEntry
        {
            public readonly RecruitmentSlotView View;
            public readonly RecruitmentDefinition Definition;

            public SlotEntry(RecruitmentSlotView view, RecruitmentDefinition definition)
            {
                View = view;
                Definition = definition;
            }
        }
    }
}
