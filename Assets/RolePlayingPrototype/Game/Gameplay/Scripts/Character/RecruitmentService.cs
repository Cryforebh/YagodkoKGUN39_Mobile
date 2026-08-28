using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using SampleProject.Base;
using UniRx;
using UnityEngine;

namespace SampleProject
{
    public interface IRecruitmentService
    {
        IReadOnlyReactiveProperty<RecruitmentBuilding> SelectedBuilding { get; }
        IReadOnlyReactiveProperty<bool> IsSettingRallyPoint { get; }
        IObservable<Unit> RallyPointChanged { get; }
        void Select(RecruitmentBuilding building);
        void Close();
        void ToggleRallyPointPlacement();
        bool TrySetRallyPoint(Vector3 position);
        bool CanRecruit(RecruitmentDefinition definition);
        bool TryRecruit(RecruitmentDefinition definition);
    }

    public sealed class RecruitmentService : IRecruitmentService, IDisposable
    {
        private readonly ReactiveProperty<RecruitmentBuilding> _selectedBuilding = new();
        private readonly ReactiveProperty<bool> _isSettingRallyPoint = new();
        private readonly Subject<Unit> _rallyPointChanged = new();
        private readonly IResourceStorage _resources;
        private readonly IUnitPoolService _unitPool;
        private readonly IEntityCommandService _commands;
        private readonly INavigationPathService _navigation;

        public IReadOnlyReactiveProperty<RecruitmentBuilding> SelectedBuilding => _selectedBuilding;
        public IReadOnlyReactiveProperty<bool> IsSettingRallyPoint => _isSettingRallyPoint;
        public IObservable<Unit> RallyPointChanged => _rallyPointChanged;

        public RecruitmentService(IResourceStorage resources, IUnitPoolService unitPool, IEntityCommandService commands, INavigationPathService navigation)
        {
            _resources = resources;
            _unitPool = unitPool;
            _commands = commands;
            _navigation = navigation;
        }

        public void Select(RecruitmentBuilding building)
        {
            if (building == null || !building.isActiveAndEnabled)
            {
                Close();
                return;
            }

            _isSettingRallyPoint.Value = false;
            _selectedBuilding.Value = building;
            var options = building.AvailableUnits;
            if (options == null)
            {
                return;
            }

            for (var index = 0; index < options.Length; index++)
            {
                var option = options[index];
                if (option != null && option.Prefab != null)
                {
                    _unitPool.Prewarm(option.Prefab, 1);
                }
            }
        }

        public void Close()
        {
            _isSettingRallyPoint.Value = false;
            _selectedBuilding.Value = null;
        }

        public void ToggleRallyPointPlacement()
        {
            if (_selectedBuilding.Value == null)
            {
                _isSettingRallyPoint.Value = false;
                return;
            }

            _isSettingRallyPoint.Value = !_isSettingRallyPoint.Value;
        }

        public bool TrySetRallyPoint(Vector3 position)
        {
            var building = _selectedBuilding.Value;
            if (building == null || !building.isActiveAndEnabled ||
                !building.TryGetSpawnPose(out var spawnPosition, out _) ||
                !_navigation.TryBuildPath(spawnPosition, position, 0.5f, out var path) || !path.IsComplete)
            {
                return false;
            }

            building.SetRallyPoint(path.Destination);
            _isSettingRallyPoint.Value = false;
            _rallyPointChanged.OnNext(Unit.Default);
            return true;
        }

        public bool CanRecruit(RecruitmentDefinition definition)
        {
            var building = _selectedBuilding.Value;
            return building != null && building.isActiveAndEnabled && definition != null &&
                   definition.Prefab != null && building.Offers(definition) &&
                   _resources.CanSpend(ResourceType.Crystals, definition.CrystalCost);
        }

        public bool TryRecruit(RecruitmentDefinition definition)
        {
            var building = _selectedBuilding.Value;
            if (!CanRecruit(definition) || !building.TryGetSpawnPose(out var position, out var rotation))
            {
                return false;
            }

            if (!_resources.TrySpend(ResourceType.Crystals, definition.CrystalCost))
            {
                return false;
            }

            var unit = _unitPool.Spawn(definition.Prefab, position, rotation, building.UnitsParent);
            if (unit == null)
            {
                return false;
            }

            SendToRallyPoint(building, unit, position);
            return true;
        }

        public void Dispose()
        {
            _rallyPointChanged.Dispose();
            _isSettingRallyPoint.Dispose();
            _selectedBuilding.Dispose();
        }

        private void SendToRallyPoint(RecruitmentBuilding building, Entities.CharacterEntity unit, Vector3 start)
        {
            const int maximumDestinationAttempts = 8;
            for (var attempt = 0; attempt < maximumDestinationAttempts; attempt++)
            {
                if (!building.TryGetNextRallyDestination(out var destination) ||
                    !_navigation.TryBuildPath(start, destination, 0.5f, out var path) || !path.IsComplete)
                {
                    continue;
                }

                var corners = path.Corners;
                if (corners.Length <= 2)
                {
                    _commands.Move(unit.Handle, path.Destination);
                    return;
                }

                var waypoints = new List<Vector3>(corners.Length - 2);
                for (var index = 1; index < corners.Length - 1; index++)
                {
                    waypoints.Add(corners[index]);
                }

                _commands.Move(unit.Handle, path.Destination, waypoints);
                return;
            }
        }
    }
}
