using Godot;

[GlobalClass]
public abstract partial class SpellDefinition : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string DisplayName { get; set; }
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }

    [Export] public Element Element { get; set; }

    [Export] public SpellCastMode CastMode { get; set; }
    [Export] public SpellBehaviourType BehaviourType { get; set; }

    [ExportGroup("Core Stats")]
    [Export] public float DamageModifier { get; set; } = 1f;
    [Export] public float BaseRange { get; set; } = 20f;
    [Export] public float BaseCooldown { get; set; } = 1f;
    [Export] public float BaseManaCost { get; set; } = 0f;

    [ExportGroup("Rehit (DoT)")]
    [Export] public bool EnableRehit { get; set; } = false;
    [Export(PropertyHint.Range, "0.01,2.0,0.01")]
    public float RehitIntervalSeconds { get; set; } = 0.25f;

    [ExportGroup("Status Magnitudes")]
    [Export] public ElementStatusProfile StatusProfile { get; set; } = null;
    [Export] public float BurningDotMultiplier = 1f;
    [Export] public float SlowMultiplierBonus = 0f;
    [Export] public float BleedDotMultiplier = 1f;
    [Export] public float StunBuildupPerHit = 0f;
}
