public static class DamageUtility
{
    public static DamagePayload CreatePayloadFrom(this IDamageStatProvider provider, bool isPiercing = false)
    {
        return new DamagePayload
        {
            baseDamage = provider.BaseDamage,
            physicalDamage = provider.PhysicalDamage,
            magicalDamage = provider.MagicalDamage,
            meleeDamage = provider.MeleeDamage,
            rangedDamage = provider.RangedDamage,
            basicAttackDamage = provider.BasicAttackDamage,
            skillDamage = provider.SkillDamage,
            finalDamageMultiplier = provider.FinalDamageMultiplier,
            critChance = provider.CritChance,
            critDamageMultiplier = provider.CritDamageMultiplier,
            isPiercing = isPiercing 
        };
    }
}
