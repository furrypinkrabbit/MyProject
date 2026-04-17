using UnityEngine;
using UIFramework.Events;

namespace GameplayFramework.Combat
{
    /// <summary>
    /// 肉体的血条与 Buff 管理器（独立组件，像挂件一样挂在主角、甚至靶子、木箱子上）
    /// </summary>
    public class HealthComponent : MonoBehaviour, ITargetable
    {
        public float maxHealth = 200f;
        public float currentHealth;
        public bool isDead = false;
        
        // 留给 GameRule 的接口，让上帝可以挂载回调
        public System.Action<DamageInfo, HealthComponent> OnTakeDamageEvent;
        public System.Action<DamageInfo, HealthComponent> OnDeathEvent;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(DamageInfo info)
        {
            if (isDead) return;

            // TODO: 这里未来会遍历身上的所有 Buff，比如“减伤50% Buff”会对 info.Amount 进行削减

            currentHealth -= info.Amount;
            
            // 触发受击表现（特效、受伤声）
            OnTakeDamageEvent?.Invoke(info, this);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
                OnDeathEvent?.Invoke(info, this);
            }
        }
    }
}
