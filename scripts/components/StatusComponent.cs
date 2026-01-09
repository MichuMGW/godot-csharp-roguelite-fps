using Godot;
using System;

public partial class StatusComponent : Node
{
    [Export] public NodePath HurtboxComponentPath = "../HurtboxComponent";

    // Opcjonalnie: jak chcesz, możesz tu trzymać listę profili dla elementów (np. z kitu),
    // ale w Twoim podejściu profil idzie w HitInfo, więc nie jest to wymagane.

    private HurtboxComponent _hurtbox;

    // ---------------- Burning runtime ----------------
    private bool _burning;
    private float _burningTimeLeft;
    private float _burningTickRate;
    private float _burningTickAcc;
    private float _burningDotPerTick;

    // ---------------- Slow runtime ----------------
    private bool _slowed;
    private float _slowTimeLeft;
    private float _slowPercent;      // 0.3 = -30%
    private float _speedMultiplier;  // 0.7 = 70% speed

    // ---------------- Bleed runtime ----------------
    private bool _bleeding;
    private float _bleedTimeLeft;
    private float _bleedTickRate;
    private float _bleedTickAcc;
    private float _bleedDotPerTick;

    [ExportGroup("Bleed Stacking")]
    [Export] public int BleedMaxStacks { get; set; } = 10;
    [Export] public bool BleedRefreshDurationOnStack { get; set; } = true;
    [Export] public bool BleedUseFastestTickRate { get; set; } = true;

    private int _bleedStacks;
    private float _bleedBaseDotPerTick;

    // ---------------- Earth runtime ----------------
    private float _stunBuildup;
    private float _stunDecayPerSecond;

    private bool _stunned;
    private float _stunTimeLeft;

	// components
	private HealthComponent _health;
	private VelocityComponent _velocity;

    public override void _Ready()
    {
        _hurtbox = GetNodeOrNull<HurtboxComponent>(HurtboxComponentPath);
        if (_hurtbox == null)
        {
            GD.PushError($"{Name}: StatusComponent missing HurtboxComponent at '{HurtboxComponentPath}'.");
            return;
        }

        _hurtbox.Hit += OnHit;

		_health = GetOwner().GetNodeOrNull<HealthComponent>("HealthComponent");
		_velocity = GetOwner().GetNodeOrNull<VelocityComponent>("VelocityComponent");

        SetProcess(false);

        // ApplyMoveSpeedMultiplier(1f);
        // ApplyStunState(false);
    }

    public override void _ExitTree()
    {
        if (_hurtbox != null)
            _hurtbox.Hit -= OnHit;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        TickBurning(dt);
        TickBleed(dt);
        TickSlow(dt);
        TickStunBuildup(dt);
        TickStun(dt);

    }

    private void OnHit(HitInfo hit)
    {
        if (hit == null)
            return;

        var profile = hit.StatusProfile;
        if (profile == null)
            return;

        if (profile.EnableBurning)
        {
            float dot = profile.BurningDotPerTick * Mathf.Max(0f, hit.BurningDotMultiplier);

            ApplyBurning(
                duration: profile.BurningDuration,
                tickRate: Mathf.Max(0.01f, profile.BurningTickRate),
                dotPerTick: dot
            );
        }

        if (profile.EnableSlow)
        {
            float slowPercent = Mathf.Clamp(profile.SlowMultiplier + hit.SlowBonus, 0f, 0.95f);

            ApplySlow(
                duration: profile.SlowDuration,
                slowPercent: slowPercent
            );
        }

        if (profile.EnableBleed)
        {
            float dot = profile.BleedDotPerTick * Mathf.Max(0f, hit.BleedDotMultiplier);

            ApplyBleed(
                duration: profile.BleedDuration,
                tickRate: Mathf.Max(0.01f, profile.BleedTickRate),
                dotPerTick: dot
            );
        }

        if (profile.EnableStunBuildup)
        {
            ApplyStunBuildup(
                add: Mathf.Max(0f, hit.StunBuildupPerHit),
                threshold: Mathf.Max(1f, profile.StunThreshold),
                decayPerSec: Mathf.Max(0f, profile.StunBuildupDecayPerSecond),
                stunDuration: Mathf.Max(0.01f, profile.StunDuration)
            );
        }
    }

    // ====================== Burning ======================

    private void ApplyBurning(float duration, float tickRate, float dotPerTick)
    {
        if (duration <= 0f || dotPerTick <= 0f)
            return;

        SetProcess(true);
        _burning = true;
        _burningTimeLeft = Mathf.Max(_burningTimeLeft, duration);
        _burningTickRate = tickRate;

        _burningDotPerTick = Mathf.Max(_burningDotPerTick, dotPerTick);
    }

    private void TickBurning(float dt)
    {
        if (!_burning)
            return;

        _burningTimeLeft -= dt;
        if (_burningTimeLeft <= 0f)
        {
            _burning = false;
            _burningTickAcc = 0f;
            _burningDotPerTick = 0f;
            TryDisableProcessIfNoActiveStatuses();
            return;
        }

        _burningTickAcc += dt;
        while (_burningTickAcc >= _burningTickRate)
        {
            _burningTickAcc -= _burningTickRate;
            DealStatusDamage(_burningDotPerTick, Element.Fire);
        }
    }

    // ====================== Bleed ======================

    private void ApplyBleed(float duration, float tickRate, float dotPerTick)
    {
        if (duration <= 0f || dotPerTick <= 0f)
            return;

        SetProcess(true);

        if (!_bleeding)
        {
            _bleeding = true;
            _bleedTickAcc = 0f;
            _bleedStacks = 0;
            _bleedBaseDotPerTick = 0f;
            _bleedTickRate = tickRate;
        }

        _bleedStacks = Mathf.Clamp(_bleedStacks + 1, 1, BleedMaxStacks);
        _bleedBaseDotPerTick = Mathf.Max(_bleedBaseDotPerTick, dotPerTick);

        if (BleedUseFastestTickRate)
            _bleedTickRate = Mathf.Min(_bleedTickRate, tickRate);
        else
            _bleedTickRate = tickRate;

        if (BleedRefreshDurationOnStack)
            _bleedTimeLeft = duration;
        else
            _bleedTimeLeft = Mathf.Max(_bleedTimeLeft, duration);

        _bleedDotPerTick = _bleedBaseDotPerTick * _bleedStacks;
    }


    private void TickBleed(float dt)
    {
        if (!_bleeding)
            return;

        _bleedTimeLeft -= dt;
        if (_bleedTimeLeft <= 0f)
        {
            _bleeding = false;
            _bleedTickAcc = 0f;
            _bleedDotPerTick = 0f;
            _bleedBaseDotPerTick = 0f;
            _bleedStacks = 0;

            TryDisableProcessIfNoActiveStatuses();
            return;
        }

        _bleedTickAcc += dt;
        while (_bleedTickAcc >= _bleedTickRate)
        {
            _bleedTickAcc -= _bleedTickRate;
            DealStatusDamage(_bleedDotPerTick, Element.Nature);
        }
    }


    // ====================== Slow ======================

    private void ApplySlow(float duration, float slowPercent)
    {
        if (duration <= 0f)
            return;

        SetProcess(true);
        _slowed = true;
        _slowTimeLeft = Mathf.Max(_slowTimeLeft, duration);

        _slowPercent = Mathf.Max(_slowPercent, slowPercent);

        _speedMultiplier = Mathf.Clamp(1f - _slowPercent, 0.05f, 1f);
        ApplyMoveSpeedMultiplier(_speedMultiplier);
    }

    private void TickSlow(float dt)
    {
        if (!_slowed)
            return;

        _slowTimeLeft -= dt;
        if (_slowTimeLeft <= 0f)
        {
            _slowed = false;
            _slowPercent = 0f;
            _speedMultiplier = 1f;
            TryDisableProcessIfNoActiveStatuses();
            ApplyMoveSpeedMultiplier(1f);
        }
    }

    // ====================== Earth buildup ======================

    private void ApplyStunBuildup(float add, float threshold, float decayPerSec, float stunDuration)
    {
        if (add <= 0f)
            return;

        SetProcess(true);
        _stunBuildup += add;

        _stunDecayPerSecond = Mathf.Max(_stunDecayPerSecond, decayPerSec);

        if (!_stunned && _stunBuildup >= threshold)
        {
            ApplyStun(stunDuration);

            _stunBuildup = 0f;
        }
    }

    private void TickStunBuildup(float dt)
    {
        if (_stunBuildup <= 0f)
            return;

        if (_stunDecayPerSecond <= 0f)
            return;

        _stunBuildup = Mathf.Max(0f, _stunBuildup - _stunDecayPerSecond * dt);

        if (_stunBuildup <= 0f)
        {
            _stunDecayPerSecond = 0f;
            TryDisableProcessIfNoActiveStatuses();
        }
    }

    // ====================== Stun ======================

    private void ApplyStun(float duration)
    {
        _stunned = true;
        _stunTimeLeft = Mathf.Max(_stunTimeLeft, duration);
        ApplyStunState(true);
    }

    private void TickStun(float dt)
    {
        if (!_stunned)
            return;

        _stunTimeLeft -= dt;
        if (_stunTimeLeft <= 0f)
        {
            _stunned = false;
            ApplyStunState(false);
            TryDisableProcessIfNoActiveStatuses();
        }
    }

    private void TryDisableProcessIfNoActiveStatuses()
    {
        bool any =
            _burning ||
            _bleeding ||
            _slowed ||
            _stunned ||
            _stunBuildup > 0f;

        if (!any)
            SetProcess(false);
    }

    // ====================== Integration hooks ======================

    private void DealStatusDamage(float amount, Element sourceElement)
    {
		_health.ApplyStatusDamage(amount, sourceElement);
    }

    private void ApplyMoveSpeedMultiplier(float mult)
    {
        _velocity.SetMoveSpeedMultiplier(mult);
    }

	//DO OGARNIĘCIA: obsługa stuna w AI
    private void ApplyStunState(bool stunned)
    {
        // var owner = GetOwner();
        // if (owner == null)
        //     return;

        // var brain = owner.GetNodeOrNull<Node>("AIComponent");
        // if (brain != null && brain.HasMethod("SetStunned"))
        //     brain.Call("SetStunned", stunned);
    }
}
