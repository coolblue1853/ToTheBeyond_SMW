using System;

public static class WeaponEvents
{
    public static event Action<SkillSlot, float> OnSkillUsed;
    public static event Action<Weapon, int> OnWeaponEquipped;
    public static event Action<Armor, int> OnArmorEquipped;

    public static void RaiseSkillUsed(SkillSlot slot, float cooldown)
    {
        OnSkillUsed?.Invoke(slot, cooldown);
    }

    public static void RaiseWeaponEquipped(Weapon weapon, int upgradeLevel)
    {
        OnWeaponEquipped?.Invoke(weapon, upgradeLevel);
    }

    public static void RaiseArmorEquipped(Armor armor, int upgradeLevel)
    {
        OnArmorEquipped?.Invoke(armor, upgradeLevel);
    }
} 