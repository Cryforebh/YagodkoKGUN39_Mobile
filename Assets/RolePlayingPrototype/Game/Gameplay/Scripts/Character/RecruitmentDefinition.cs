using Entities;
using UnityEngine;

namespace SampleProject
{
    [CreateAssetMenu(fileName = "New Recruitment Unit", menuName = "Game/Recruitment/Unit")]
    public sealed class RecruitmentDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Unit";
        [SerializeField] private CharacterEntity _prefab;
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(0)] private int _crystalCost = 10;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public CharacterEntity Prefab => _prefab;
        public Sprite Icon => _icon;
        public int CrystalCost => Mathf.Max(0, _crystalCost);
    }
}
