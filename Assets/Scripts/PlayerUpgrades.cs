using System.Collections.Generic;
using UnityEngine;

public enum UpgradeId
{
    SpeedMultiplicative,
    SpeedAdditive,
    RepairRange,
    PickupRange,
    SprintSpeed,
    SprintDuration,
    SprintCooldown,
    RepairSpeed,
    RepairKeyBonus,
    RepairKeyMercy
}

public struct UpgradeOption
{
    public UpgradeId Id;
    public string Label;

    public UpgradeOption (UpgradeId id, string label)
    {
        Id = id;
        Label = label;
    }
}

public static class PlayerUpgrades
{
    public const int ChoicesPerLevel = 3;
    public const int EnemyEnhancementInterval = 2;

    // 加成倍率接口：1 = 正常数值，调大后所有玩家加成按同一倍率放大。
    // 数值在读取时换算，所以改完立即对已经拿到的加成生效；只影响玩家加成，不影响敌人加强。
    // 目前临时设为 100 倍，方便快速验证加成效果。
    public static float MagnitudeMultiplier = 100f;

    static float ScaledStep (float baseStep)
    {
        return baseStep * Mathf.Max (0f, MagnitudeMultiplier);
    }

    static float ScaledMultiplier (float baseMultiplier)
    {
        return 1f + (baseMultiplier - 1f) * Mathf.Max (0f, MagnitudeMultiplier);
    }

    static int Stacks (string key)
    {
        return Mathf.Max (0, GameSession.GetInt (key));
    }

    static void AddStack (string key)
    {
        GameSession.SetInt (key, Stacks (key) + 1);
    }

    public const float SpeedMultiplierStep = 1.01f;
    public const float SpeedAddStep = 0.03f;
    public const float RepairRangeStep = 0.05f;
    public const float PickupRangeStep = 0.05f;
    public const float SprintSpeedStep = 0.1f;
    public const float SprintDurationStep = 0.1f;
    public const float SprintCooldownStep = 0.5f;
    public const float RepairSpeedStep = 0.03f;
    public const float CorrectPressStep = 0.05f;
    public const float WrongPressStep = 0.03f;
    public const float EnemyAngleStep = 10f;
    public const float EnemyRadiusStep = 0.35f;
    public const float EnemySpeedStep = 0.15f;

    private const string LevelKey = "run.level";
    private const string SpeedMultiplierKey = "buff.speedMultiplier";
    private const string SpeedAddKey = "buff.speedAdd";
    private const string RepairRangeKey = "buff.repairRange";
    private const string PickupRangeKey = "buff.pickupRange";
    private const string SprintSpeedKey = "buff.sprintSpeed";
    private const string SprintDurationKey = "buff.sprintDuration";
    private const string SprintCooldownKey = "buff.sprintCooldown";
    private const string RepairSpeedKey = "buff.repairSpeed";
    private const string CorrectPressKey = "buff.repairCorrectPress";
    private const string WrongPressKey = "buff.repairWrongPress";
    private const string EnemyAngleKey = "enemy.viewAngle";
    private const string EnemyRadiusKey = "enemy.viewRadius";
    private const string EnemySpeedKey = "enemy.moveSpeed";

    static UpgradeOption[] BuildOptions ()
    {
        return new[]
        {
            new UpgradeOption (UpgradeId.SpeedMultiplicative, "人物速度 \u00D7" + ScaledMultiplier (SpeedMultiplierStep).ToString ("0.###")),
            new UpgradeOption (UpgradeId.SpeedAdditive, "人物速度 +" + ScaledStep (SpeedAddStep).ToString ("0.###")),
            new UpgradeOption (UpgradeId.RepairRange, "修机判定范围 +" + ScaledStep (RepairRangeStep).ToString ("0.###")),
            new UpgradeOption (UpgradeId.PickupRange, "拾取范围 +" + ScaledStep (PickupRangeStep).ToString ("0.###")),
            new UpgradeOption (UpgradeId.SprintSpeed, "冲刺速度倍率 +" + ScaledStep (SprintSpeedStep).ToString ("0.###")),
            new UpgradeOption (UpgradeId.SprintDuration, "冲刺持续 +" + ScaledStep (SprintDurationStep).ToString ("0.###") + " 秒"),
            new UpgradeOption (UpgradeId.SprintCooldown, "冲刺冷却 -" + ScaledStep (SprintCooldownStep).ToString ("0.###") + " 秒"),
            new UpgradeOption (UpgradeId.RepairSpeed, "修机速度 +" + ScaledStep (RepairSpeedStep).ToString ("0.###") + " 倍"),
            new UpgradeOption (UpgradeId.RepairKeyBonus, "修机按键加成 +" + ScaledStep (CorrectPressStep).ToString ("0.###")),
            new UpgradeOption (UpgradeId.RepairKeyMercy, "修机按错惩罚 -" + ScaledStep (WrongPressStep).ToString ("0.###")),
        };
    }

    private static readonly string[] enemyLabels =
    {
        "鬼魂判定扇形角度 +10\u00B0",
        "鬼魂判定半径 +0.35",
        "鬼魂移动速度 +0.15",
    };

    private static readonly string[] enemyKeys =
    {
        EnemyAngleKey, EnemyRadiusKey, EnemySpeedKey,
    };

    private static readonly float[] enemySteps =
    {
        EnemyAngleStep, EnemyRadiusStep, EnemySpeedStep,
    };

    public static int Level
    {
        get { return Mathf.Max (1, GameSession.GetInt (LevelKey, 1)); }
    }

    public static float SpeedFactor
    {
        get
        {
            return Mathf.Pow (ScaledMultiplier (SpeedMultiplierStep), Stacks (SpeedMultiplierKey)) +
                   ScaledStep (SpeedAddStep) * Stacks (SpeedAddKey);
        }
    }

    public static float RepairRangeBonus { get { return ScaledStep (RepairRangeStep) * Stacks (RepairRangeKey); } }
    public static float PickupRangeBonus { get { return ScaledStep (PickupRangeStep) * Stacks (PickupRangeKey); } }
    public static float SprintSpeedBonus { get { return ScaledStep (SprintSpeedStep) * Stacks (SprintSpeedKey); } }
    public static float SprintDurationBonus { get { return ScaledStep (SprintDurationStep) * Stacks (SprintDurationKey); } }
    public static float SprintCooldownReduction { get { return ScaledStep (SprintCooldownStep) * Stacks (SprintCooldownKey); } }
    public static float RepairSpeedFactor { get { return 1f + ScaledStep (RepairSpeedStep) * Stacks (RepairSpeedKey); } }
    public static float CorrectPressBonus { get { return ScaledStep (CorrectPressStep) * Stacks (CorrectPressKey); } }
    public static float WrongPressBonus { get { return ScaledStep (WrongPressStep) * Stacks (WrongPressKey); } }
    public static float EnemyAngleBonus { get { return GameSession.GetFloat (EnemyAngleKey); } }
    public static float EnemyRadiusBonus { get { return GameSession.GetFloat (EnemyRadiusKey); } }
    public static float EnemySpeedBonus { get { return GameSession.GetFloat (EnemySpeedKey); } }

    public static UpgradeOption[] RollOptions (int count)
    {
        List<UpgradeOption> pool = new List<UpgradeOption> (BuildOptions ());
        count = Mathf.Clamp (count, 1, pool.Count);

        UpgradeOption[] result = new UpgradeOption[count];
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range (0, pool.Count);
            result[i] = pool[index];
            pool.RemoveAt (index);
        }

        return result;
    }

    public static void Apply (UpgradeOption option)
    {
        switch (option.Id)
        {
            case UpgradeId.SpeedMultiplicative:
                AddStack (SpeedMultiplierKey);
                break;
            case UpgradeId.SpeedAdditive:
                AddStack (SpeedAddKey);
                break;
            case UpgradeId.RepairRange:
                AddStack (RepairRangeKey);
                break;
            case UpgradeId.PickupRange:
                AddStack (PickupRangeKey);
                break;
            case UpgradeId.SprintSpeed:
                AddStack (SprintSpeedKey);
                break;
            case UpgradeId.SprintDuration:
                AddStack (SprintDurationKey);
                break;
            case UpgradeId.SprintCooldown:
                AddStack (SprintCooldownKey);
                break;
            case UpgradeId.RepairSpeed:
                AddStack (RepairSpeedKey);
                break;
            case UpgradeId.RepairKeyBonus:
                AddStack (CorrectPressKey);
                break;
            case UpgradeId.RepairKeyMercy:
                AddStack (WrongPressKey);
                break;
        }
    }

    public static string CompleteLevel ()
    {
        int level = Level + 1;
        GameSession.SetInt (LevelKey, level);

        if (level % EnemyEnhancementInterval != 0)
            return string.Empty;

        int index = Random.Range (0, enemyLabels.Length);
        GameSession.SetFloat (enemyKeys[index], GameSession.GetFloat (enemyKeys[index]) + enemySteps[index]);
        return enemyLabels[index];
    }

    public static string NextLevelHint ()
    {
        int nextLevel = Level + 1;
        return nextLevel % EnemyEnhancementInterval == 0
            ? "第 " + nextLevel + " 关会出现一项敌人加强"
            : string.Empty;
    }

    public static string DescribeBonuses ()
    {
        List<string> parts = new List<string> ();

        if (!Mathf.Approximately (SpeedFactor, 1f))
            parts.Add ("速度 \u00D7" + SpeedFactor.ToString ("0.00"));
        if (!Mathf.Approximately (RepairRangeBonus, 0f))
            parts.Add ("修机范围 +" + RepairRangeBonus.ToString ("0.00"));
        if (!Mathf.Approximately (PickupRangeBonus, 0f))
            parts.Add ("拾取范围 +" + PickupRangeBonus.ToString ("0.00"));
        if (!Mathf.Approximately (SprintSpeedBonus, 0f))
            parts.Add ("冲刺倍率 +" + SprintSpeedBonus.ToString ("0.0"));
        if (!Mathf.Approximately (SprintDurationBonus, 0f))
            parts.Add ("冲刺时长 +" + SprintDurationBonus.ToString ("0.0") + "s");
        if (!Mathf.Approximately (SprintCooldownReduction, 0f))
            parts.Add ("冲刺冷却 -" + SprintCooldownReduction.ToString ("0.0") + "s");
        if (!Mathf.Approximately (RepairSpeedFactor, 1f))
            parts.Add ("修机速度 \u00D7" + RepairSpeedFactor.ToString ("0.00"));
        if (!Mathf.Approximately (CorrectPressBonus, 0f))
            parts.Add ("按键加成 +" + CorrectPressBonus.ToString ("0.00"));
        if (!Mathf.Approximately (WrongPressBonus, 0f))
            parts.Add ("按错惩罚 -" + WrongPressBonus.ToString ("0.00"));

        if (EnemyAngleBonus > 0f || EnemyRadiusBonus > 0f || EnemySpeedBonus > 0f)
            parts.Add ("敌人已强化 " + Mathf.FloorToInt (Level / (float) EnemyEnhancementInterval) + " 次");

        return parts.Count > 0 ? string.Join ("\uFF5C", parts.ToArray ()) : "\u6682\u65E0\uFF08\u901A\u5173\u540E\u53EF\u9009\u62E9\uFF09";
    }
}
