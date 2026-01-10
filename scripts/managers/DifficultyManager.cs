using Godot;
using System;

public enum RunDifficulty
{
    Casual,
    Normal,
    Hardcore
}

public struct DifficultySnapshot
{
    public float Coeff;
    public int EnemyLevel;
    public float HpMultiplier;
    public float DamageMultiplier;
    public float MoveSpeedMultiplier;

    public float SpawnInterval;
    public int MaxAliveEnemies;
    public int MaxMinionOverCap;
}

public sealed class DifficultyManager
{
    private const float BaseTimeSlope = 0.055f;
    private const float LateRamp = 0.0022f;
    private const float CoeffPerLevel = 0.08f;

    private const float HpPerLevel = 0.28f;
    private const float DmgPerLevel = 0.18f;
    private const float SpdPerLevel = 0.06f;

    private const float SpawnIntervalStart = 7f;
    private const float SpawnIntervalFloor = 0.60f;

    private const int MaxAliveStart = 10;
    private const int MaxAliveCap = 60;

    private const float BudgetStart = 5.5f;
    private const float BudgetGrowth = 0.11f;

    private const float PulseEveryMinutes = 1f;
    private const float PulseStrength = 0.06f;
    private const float PulseWidth = 0.9f;
    public RunDifficulty RunDifficulty { get; set; }

    public DifficultySnapshot GetDifficulty(
        float elapsedSeconds,
        RunDifficulty runDifficulty
    )
    {
        float tMin = Mathf.Max(0f, elapsedSeconds) / 60f;
        float pace = runDifficulty switch
        {
            RunDifficulty.Casual   => 0.75f,
            RunDifficulty.Normal   => 1.00f,
            RunDifficulty.Hardcore => 1.50f,
            _ => 1.0f
        };

        float coeff = 1.0f + pace * (BaseTimeSlope * tMin + LateRamp * tMin * tMin);
        coeff *= 1.0f + Pulse(tMin, PulseEveryMinutes, PulseStrength, PulseWidth);

        int enemyLevel = Mathf.Clamp(1 + Mathf.FloorToInt((coeff - 1.0f) / CoeffPerLevel), 1, 999);

        float hpMul = Mathf.Pow(1f + HpPerLevel, enemyLevel - 1);
        float dmgMul = Mathf.Pow(1f + DmgPerLevel, enemyLevel - 1);
        float spdMul = Mathf.Pow(1f + SpdPerLevel, enemyLevel - 1);

        float spawnInterval = Mathf.Max(
            SpawnIntervalFloor,
            SpawnIntervalStart / (1f + 0.55f * Mathf.Log(1f + coeff))
        );

        int maxAlive = Mathf.Clamp(
            MaxAliveStart + Mathf.RoundToInt(10f * Mathf.Log(1f + coeff) + 0.8f * (enemyLevel - 1)),
            MaxAliveStart,
            MaxAliveCap
        );

        int maxMinionOverCap = 50 + Mathf.RoundToInt(15f * (coeff - 1f));

        return new DifficultySnapshot
        {
            Coeff = coeff,
            EnemyLevel = enemyLevel,
            HpMultiplier = hpMul,
            DamageMultiplier = dmgMul,
            MoveSpeedMultiplier = spdMul,
            SpawnInterval = spawnInterval,
            MaxAliveEnemies = maxAlive,
            MaxMinionOverCap = maxMinionOverCap
        };
    }

    // Miękki “puls” zamiast stageFactor: Gaussian-ish wokół wielokrotności period
    private static float Pulse(float tMin, float period, float strength, float width)
    {
        if (period <= 0.01f) return 0f;

        float k = Mathf.Round(tMin / period) * period;     // najbliższa “kotwica”
        float x = (tMin - k) / Mathf.Max(0.01f, width);    // znormalizowana odległość
        float g = Mathf.Exp(-0.5f * x * x);                // 1 w środku, maleje miękko
        return strength * g;
    }
}
