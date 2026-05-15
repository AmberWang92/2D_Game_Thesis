using UnityEngine;

namespace TopDownShooter.Data
{
    /// <summary>
    /// Tunable player stats. Designers edit the asset, no code change required.
    /// </summary>
    [CreateAssetMenu(menuName = "TopDownShooter/Data/Player Stats", fileName = "PlayerStats")]
    public class PlayerStatsSO : ScriptableObject
    {
        [Min(0.1f)] public float moveSpeed = 6f;
        [Min(1)] public int maxHP = 10;

        [Tooltip("Default weapon assigned to the player on spawn.")]
        public WeaponDefinitionSO primaryWeapon;
    }
}
