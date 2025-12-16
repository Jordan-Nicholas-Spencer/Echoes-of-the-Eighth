using UnityEngine;
using UnityEngine.Events;

namespace Unity_Store_Imports.Ilumisoft.Health_System.Scripts.Base
{
    /// <summary>
    /// Abstract base class of the health component
    /// </summary>
    public abstract class HealthComponent : MonoBehaviour
    {
        public UnityAction<float> OnHealthChanged { get; set; }
        public UnityAction OnHealthEmpty { get; set; }
        public abstract float MaxHealth { get; set; }
        public abstract float CurrentHealth { get; set; }
        public abstract bool IsAlive { get; }

        public abstract void AddHealth(float amount);
        public abstract void ApplyDamage(float damage);
        public abstract void SetHealth(float health);
    }
}