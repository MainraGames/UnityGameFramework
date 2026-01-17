using System;

namespace _Game.Scripts.Core.Interfaces
{
    /// <summary>
    /// Represents an entity that can receive damage and be healed.
    /// Provides common properties, events, and methods frequently used in gameplay code.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Current health value. Should be clamped between 0 and <see cref="MaxHealth"/> by implementations.
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Maximum health value.
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Indicates whether the entity is currently dead (e.g., <c>CurrentHealth &lt;= 0</c>).
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Indicates whether the entity is currently invulnerable (temporarily or permanently) and should not take damage.
        /// </summary>
        bool IsInvulnerable { get; }

        /// <summary>
        /// Indicates whether the entity is eligible to receive damage at the moment.
        /// Typically equivalent to <c>!IsInvulnerable &amp;&amp; !IsDead</c> but can vary per implementation.
        /// </summary>
        bool CanTakeDamage { get; }

        /// <summary>
        /// Raised when the entity takes damage.
        /// </summary>
        /// <param name="amount">The damage amount actually applied (after mitigation), non-negative.</param>
        /// <param name="source">Optional source object that caused the damage (attacker, hazard, etc.).</param>
        event Action<float, object> OnDamaged;

        /// <summary>
        /// Raised when the entity is healed.
        /// </summary>
        /// <param name="amount">The healing amount actually applied, non-negative.</param>
        event Action<float> OnHealed;

        /// <summary>
        /// Raised when the entity dies.
        /// </summary>
        event Action OnDied;

        /// <summary>
        /// Raised when the entity is revived (comes back from a dead state).
        /// </summary>
        event Action OnRevived;

        /// <summary>
        /// Applies damage to the entity if <see cref="CanTakeDamage"/> is true.
        /// Implementations should handle clamping, mitigation, and event invocation.
        /// </summary>
        /// <param name="amount">The incoming damage amount, non-negative.</param>
        /// <param name="source">Optional source object that caused the damage.</param>
        void TakeDamage(float amount, object source = null);

        /// <summary>
        /// Heals the entity by the specified amount.
        /// Implementations should clamp to <see cref="MaxHealth"/> and invoke events.
        /// </summary>
        /// <param name="amount">The healing amount, non-negative.</param>
        void Heal(float amount);

        /// <summary>
        /// Forces the entity into a dead state, invoking <see cref="OnDied"/> as appropriate.
        /// </summary>
        void Kill();

        /// <summary>
        /// Revives the entity from a dead state.
        /// </summary>
        /// <param name="health">
        /// Optional health to set on revive. If negative, implementations may choose a default (e.g., MaxHealth or a configured value).
        /// </param>
        void Revive(float health = -1f);
    }
}