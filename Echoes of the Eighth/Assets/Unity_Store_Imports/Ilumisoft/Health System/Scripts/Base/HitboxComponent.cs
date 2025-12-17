using UnityEngine;

namespace Unity_Store_Imports.Ilumisoft.Health_System.Scripts.Base
{
    /// <summary>
    /// Abstract base class for the hitbox component
    /// </summary>
    public abstract class HitboxComponent : MonoBehaviour
    {
        public abstract HealthComponent Health { get; protected set; }

        public abstract void ApplyDamage(float damageAmount);
    }
}