using UnityEngine;

namespace Player
{
    public class DamageableScriptableObject : ScriptableObject
    {
        public int maxHealth = 100;
        public int currentHealth;
        public bool isDead = false;

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            isDead = true;
        }

        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }
}
