namespace Capybrawlers.Core
{
    public enum NatureType   { Flame, Storm, Plant, Rock, Water, Moon }
    public enum ElementType  { Flame, Storm, Plant, Rock, Water, Moon }
    public enum EquipSlot    { Helm, Armor, Weapon }
    public enum RankTier     { Pup, Sprout, Brawler, StoneFang, Tempest, GrandCapy }
    public enum BattleResult { Win, Loss }
    public enum SceneId      { Bootstrap = 0, MainMenu = 1, Battle = 2, Collection = 3 }
    public enum FormationSlot { Frontline, BacklineLeft, BacklineRight }

    public enum MechanicType
    {
        Burn, Rage, Execute, Pierce, Berserker,
        Haste, Slow, Stun, FullStun,
        Reflect, Shield, StaminaSteal,
        Poison, Lifesteal,
        Heal, Purge, StaminaRegen, Cleanse
    }
}
