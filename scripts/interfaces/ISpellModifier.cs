public interface ISpellModifier
{
    string Id { get; }
    int Priority { get; }
    bool IsExpired { get; }

    SpellCastStats Modify(SpellCastStats stats, SpellInstance instance, PlayerStatsManager playerStats);
}
