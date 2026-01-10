using Godot;
using System;

public partial class LevelUpOverlay : CanvasLayer
{
    [Signal]
    public delegate void PickedEventHandler(LevelUpOffer picked);

    // ===== Scene references (NodePaths) =====

    [ExportGroup("Header")]
    [Export] public NodePath HeaderLabelPath = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer/Label";

    [ExportGroup("Buttons")]
    [Export] public NodePath Button1Path = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button1";
    [Export] public NodePath Button2Path = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button2";
    [Export] public NodePath Button3Path = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button3";

    // Dzieci wewnątrz przycisku: MarginContainer -> VBoxContainer -> Title/Description
    [ExportGroup("Button 1 Labels")]
    [Export] public NodePath Button1TitlePath = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button1/MarginContainer/VBoxContainer/Title";
    [Export] public NodePath Button1DescPath  = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button1/MarginContainer/VBoxContainer/Description";

    [ExportGroup("Button 2 Labels")]
    [Export] public NodePath Button2TitlePath = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button2/MarginContainer5/VBoxContainer/Title";
    [Export] public NodePath Button2DescPath  = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button2/MarginContainer5/VBoxContainer/Description";

    [ExportGroup("Button 3 Labels")]
    [Export] public NodePath Button3TitlePath = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button3/MarginContainer6/VBoxContainer/Title";
    [Export] public NodePath Button3DescPath  = "MarginContainer/Panel/MarginContainer/VBoxContainer/MarginContainer2/VBoxContainer/Button3/MarginContainer6/VBoxContainer/Description";

    private Label _header;

    private Button _button1;
    private Button _button2;
    private Button _button3;

    private Label _title1;
    private Label _desc1;
    private Label _title2;
    private Label _desc2;
    private Label _title3;
    private Label _desc3;

    private Godot.Collections.Array<LevelUpOffer> _options = new();
    private GameEvents _events;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");

        _header = GetNodeOrNull<Label>(HeaderLabelPath);

        _button1 = GetNodeOrNull<Button>(Button1Path);
        _button2 = GetNodeOrNull<Button>(Button2Path);
        _button3 = GetNodeOrNull<Button>(Button3Path);

        _title1 = GetNodeOrNull<Label>(Button1TitlePath);
        _desc1 = GetNodeOrNull<Label>(Button1DescPath);

        _title2 = GetNodeOrNull<Label>(Button2TitlePath);
        _desc2 = GetNodeOrNull<Label>(Button2DescPath);

        _title3 = GetNodeOrNull<Label>(Button3TitlePath);
        _desc3 = GetNodeOrNull<Label>(Button3DescPath);

        if (_button1 != null) _button1.Pressed += () => PickIndex(0);
        if (_button2 != null) _button2.Pressed += () => PickIndex(1);
        if (_button3 != null) _button3.Pressed += () => PickIndex(2);

        // Na start ukryj / wyczyść
        SetOptions(new Godot.Collections.Array<LevelUpOffer>());
    }

    public void SetOptions(Godot.Collections.Array<LevelUpOffer> options)
    {
        _options = options ?? new Godot.Collections.Array<LevelUpOffer>();

        if (_header != null)
            _header.Text = "Choose an Upgrade";

        ApplySlot(0, _button1, _title1, _desc1);
        ApplySlot(1, _button2, _title2, _desc2);
        ApplySlot(2, _button3, _title3, _desc3);
    }

    private void PickIndex(int idx)
    {
        if (idx < 0 || idx >= _options.Count)
            return;

        var picked = _options[idx];
        if (picked == null || picked.Upgrade == null)
            return;

        EmitSignal(nameof(Picked), picked);

        if (_events != null)
            _events.ResolveLevelUpChoice(picked);
    }

    private void ApplySlot(int idx, Button btn, Label title, Label desc)
    {
        bool has = idx < _options.Count && _options[idx] != null && _options[idx].Upgrade != null;

        if (btn == null) return;

        btn.Disabled = !has;
        btn.Visible = has;

        if (!has)
            return;

        var offer = _options[idx];
        var up = offer.Upgrade;

        if (title != null)
            title.Text = $"{FormatRarity(offer.Rarity)} {up.DisplayName}";

        if (desc != null)
            desc.Text = BuildDescriptionText(offer);
    }

    private string FormatRarity(UpgradeRarity rarity)
    {
        // Bez kolorów, bo nie prosiłeś. Same teksty.
        return rarity switch
        {
            UpgradeRarity.Common => "[Common]",
            UpgradeRarity.Uncommon => "[Uncommon]",
            UpgradeRarity.Epic => "[Epic]",
            UpgradeRarity.Legendary => "[Legendary]",
            UpgradeRarity.Mythic => "[Mythic]",
            _ => "[Common]"
        };
    }

    private string BuildDescriptionText(LevelUpOffer offer)
    {
        var up = offer.Upgrade;
        float rm = offer.RarityMult;

        // bazowy opis (jeśli jest)
        string baseDesc = string.IsNullOrWhiteSpace(up.Description)
            ? ""
            : up.Description.Trim();

        if (up.StatMods == null || up.StatMods.Count == 0)
            return baseDesc;

        var parts = new System.Collections.Generic.List<string>();

        foreach (var m in up.StatMods)
        {
            float add = m.Add * rm;
            float mult = ScaleMult(m.Mult, rm);

            string part = BuildEffectPair(m.Stat, add, mult); // <-- nowa funkcja
            if (!string.IsNullOrEmpty(part))
                parts.Add(part);
        }

        string effects = string.Join(", ", parts);

        if (string.IsNullOrWhiteSpace(baseDesc))
            return effects;

        if (string.IsNullOrWhiteSpace(effects))
            return baseDesc;

        // Jedna linia: opis + efekty (bez '\n')
        return $"{baseDesc} {effects}";
    }

    private string BuildEffectPair(StatId stat, float add, float mult)
    {
        bool hasAdd = !Mathf.IsEqualApprox(add, 0f);
        bool hasMult = !Mathf.IsEqualApprox(mult, 1f);

        if (!hasAdd && !hasMult)
            return "";

        string statName = stat.ToString(); // jak chcesz ładne nazwy, zmapujesz sobie enum -> string

        // "opis: wartość"
        if (hasAdd && hasMult)
            return $": +{FormatNumber(add)} / x{FormatNumber(mult)}";

        if (hasAdd)
            return $"+{FormatNumber(add)}";

        return $": x{FormatNumber(mult)}";
    }


    private string FormatNumber(float v)
    {
        // prosto: 2 miejsca po przecinku, bez kombinowania
        return v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    private float ScaleMult(float baseMult, float rarityMult) => 1f + (baseMult - 1f) * rarityMult;
}
