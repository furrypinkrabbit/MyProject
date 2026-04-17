using GameplayFramework.Core;
using UnityEngine;

namespace GameplayFramework.Combat
{
    // 所有的伤害/受击/回血，都必须通过这个快递包裹传递！绝对不能直接 player.hp -= 10！
    public struct DamageInfo
    {
        public PlayerController Attacker; // 谁打的
        public float Amount;              // 打了多少
        public DamageType Type;           // 是什么子弹
        public Vector3 HitPoint;          // 打在身上哪里（为了爆血特效）
        
        public DamageInfo(PlayerController attacker, float amount, DamageType type, Vector3 hitPoint)
        {
            Attacker = attacker;
            Amount = amount;
            Type = type;
            HitPoint = hitPoint;
        }
    }

    public interface ITargetable
    {
        void TakeDamage(DamageInfo info);
    }
}
