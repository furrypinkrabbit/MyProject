namespace GameplayFramework.Combat
{
    public enum CombatInputAction
    {
        PrimaryFire,   // 左键
        SecondaryFire, // 右键
        Skill1,        // E
        Skill2,        // LShift
        Ultimate,      // Q
        Reload,        // R
        Melee          // F
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Explosion,
        Heal // 负伤害即治疗
    }
}
