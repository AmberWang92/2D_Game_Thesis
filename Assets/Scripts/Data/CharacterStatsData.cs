using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "TopDownShooter/Data/Character Stats")]
    public class CharacterStatsData : ScriptableObject
    {
        [Header("Health")]
        public float maxHealth = 100f;
        
        [Header("Movement")]
        public float moveSpeed = 5f;
    }
}
