using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    public float upgradeConfirmDelay = 1.2f;
    public GameObject player;
    public CanvasGroup exitBackgroundImageCanvasGroup;
    public AudioSource exitAudio;
    public CanvasGroup caughtBackgroundImageCanvasGroup;
    public AudioSource caughtAudio;
    public RepairMachine repairMachine;
    public GameHUD gameHud;
    public Collider lockedDoorCollider;
    public GameObject lockedDoor;

    bool m_IsPlayerAtExit;
    bool m_IsPlayerCaught;
    float m_VictoryTimer;
    float m_CaughtTimer;
    bool m_HasVictoryAudioPlayed;
    bool m_HasCaughtAudioPlayed;

    enum VictoryPhase { Inactive, Choosing, Confirmed }

    VictoryPhase m_VictoryPhase;
    UpgradeOption[] m_UpgradeOptions;
    float m_ConfirmTimer;

    const string ControlModeCode = "bapeireng";
    string m_CodeBuffer = string.Empty;

    const float ResetConfirmDelay = 0.5f;
    float m_ResetPendingTimer;
    bool m_ResetPending;
    bool m_ResetSkipTypedFrame;

    bool CanExit => repairMachine == null || repairMachine.IsRepaired;

    void Start ()
    {
        string pending = PlayerUpgrades.PendingHudMessage;
        PlayerUpgrades.PendingHudMessage = null;
        if (!string.IsNullOrEmpty (pending))
            gameHud?.ShowMessage (pending, 4f);
    }

    void OnEnable ()
    {
        if (repairMachine != null)
            repairMachine.StateChanged += RefreshDoor;
        RefreshDoor();
    }

    void OnDisable ()
    {
        if (repairMachine != null)
            repairMachine.StateChanged -= RefreshDoor;
    }

    void RefreshDoor ()
    {
        if (lockedDoor != null)
        {
            lockedDoor.SetActive(!CanExit);
        }
        else if (lockedDoorCollider != null && lockedDoorCollider.enabled != !CanExit)
            lockedDoorCollider.enabled = !CanExit;
    }

    public void NotifyLockedDoorContact (GameObject other)
    {
        if (other == player && !CanExit)
            gameHud?.ShowMessage("门不能从这一侧打开");
    }
    
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject == player)
        {
            if (CanExit)
                m_IsPlayerAtExit = true;
            else
                gameHud?.ShowMessage("门不能从这一侧打开");
        }
    }

    void OnTriggerStay (Collider other)
    {
        if (other.gameObject == player && CanExit)
            m_IsPlayerAtExit = true;
    }

    void OnCollisionEnter (Collision collision)
    {
        NotifyLockedDoorContact(collision.gameObject);
    }

    public void CaughtPlayer ()
    {
        m_IsPlayerCaught = true;
    }

    void Update ()
    {
        UpdateControlMode ();

        if (m_IsPlayerAtExit)
            UpdateVictorySequence ();
        else if (m_IsPlayerCaught)
            UpdateCaughtSequence ();
    }

    void UpdateControlMode ()
    {
        if (m_IsPlayerAtExit || m_IsPlayerCaught)
            return;

        if (ReadControlModeCode ())
        {
            PlayerUpgrades.ControlMode = !PlayerUpgrades.ControlMode;
            gameHud?.RefreshStatus ();
            gameHud?.ShowMessage (PlayerUpgrades.ControlMode
                ? "控制模式：开｜/ 获胜｜Z / X 我方倍率｜C / V 敌方倍率｜B 重置全部｜Ctrl 步长 ×10"
                : "控制模式：关");
            return;
        }

        if (!PlayerUpgrades.ControlMode)
            return;

        if (gameHud != null && gameHud.UpgradeChoiceVisible)
            return;

        if (Input.GetKeyDown (KeyCode.Slash))
        {
            m_IsPlayerAtExit = true;
            return;
        }

        if (Input.GetKeyDown (KeyCode.B))
            BeginPendingReset ();

        if (m_ResetPending)
        {
            UpdatePendingReset ();
            return;
        }

        float step = PlayerUpgrades.MagnitudeStep;
        float enemyStep = PlayerUpgrades.EnemyMagnitudeStep;
        if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
        {
            step *= 10f;
            enemyStep *= 10f;
        }

        if (Input.GetKeyDown (KeyCode.X))
            ShowMagnitude (PlayerUpgrades.AdjustMagnitude (step));
        else if (Input.GetKeyDown (KeyCode.Z))
            ShowMagnitude (PlayerUpgrades.AdjustMagnitude (-step));
        else if (Input.GetKeyDown (KeyCode.V))
            ShowEnemyMagnitude (PlayerUpgrades.AdjustEnemyMagnitude (enemyStep));
        else if (Input.GetKeyDown (KeyCode.C))
            ShowEnemyMagnitude (PlayerUpgrades.AdjustEnemyMagnitude (-enemyStep));
    }

    bool ReadControlModeCode ()
    {
        string typed = Input.inputString;
        if (string.IsNullOrEmpty (typed))
            return false;

        m_CodeBuffer += typed.ToLowerInvariant ();
        if (m_CodeBuffer.Length > ControlModeCode.Length)
            m_CodeBuffer = m_CodeBuffer.Substring (m_CodeBuffer.Length - ControlModeCode.Length);

        if (m_CodeBuffer != ControlModeCode)
            return false;

        m_CodeBuffer = string.Empty;
        return true;
    }

    // B 键先等一小会儿再重置：正在输入 bapeireng 关闭控制模式时不会误触。
    void BeginPendingReset ()
    {
        m_ResetPending = true;
        m_ResetPendingTimer = ResetConfirmDelay;
        m_ResetSkipTypedFrame = true;
    }

    void UpdatePendingReset ()
    {
        if (m_ResetSkipTypedFrame)
        {
            m_ResetSkipTypedFrame = false;
            return;
        }

        if (!string.IsNullOrEmpty (Input.inputString))
        {
            m_ResetPending = false;
            return;
        }

        m_ResetPendingTimer -= Time.deltaTime;
        if (m_ResetPendingTimer > 0f)
            return;

        m_ResetPending = false;
        PlayerUpgrades.ResetAll ();
        PlayerUpgrades.PendingHudMessage = "已重置全部加成：我方 / 敌方 / 关卡进度 / 倍率";
        SceneManager.LoadScene (0);
    }

    void ShowMagnitude (float magnitude)
    {
        gameHud?.RefreshStatus ();
        gameHud?.ShowMessage ("我方加成倍率 ×" + magnitude.ToString ("0.##") + "（关底结算）");
    }

    void ShowEnemyMagnitude (float magnitude)
    {
        gameHud?.RefreshStatus ();
        gameHud?.ShowMessage ("敌方加成倍率 ×" + magnitude.ToString ("0.##") + "（关底结算）");
    }

    void UpdateVictorySequence ()
    {
        if (m_VictoryPhase == VictoryPhase.Inactive)
        {
            m_VictoryPhase = VictoryPhase.Choosing;
            m_UpgradeOptions = PlayerUpgrades.RollOptions (PlayerUpgrades.ChoicesPerLevel);
            gameHud?.ShowUpgradeChoice (m_UpgradeOptions);
        }

        if (!m_HasVictoryAudioPlayed)
        {
            if (exitAudio != null)
                exitAudio.Play ();
            m_HasVictoryAudioPlayed = true;
        }

        m_VictoryTimer += Time.deltaTime;
        FadeIn (exitBackgroundImageCanvasGroup, m_VictoryTimer);

        if (m_VictoryPhase == VictoryPhase.Choosing)
        {
            if (gameHud == null)
            {
                ConfirmUpgrade (Random.Range (0, m_UpgradeOptions.Length));
                return;
            }

            int index = gameHud.PollUpgradeChoice ();
            if (index < 0)
                return;

            ConfirmUpgrade (index);
            return;
        }

        m_ConfirmTimer += Time.deltaTime;
        if (m_ConfirmTimer >= upgradeConfirmDelay)
            ResetLevelAfterVictory ();
    }

    void ConfirmUpgrade (int index)
    {
        UpgradeOption option = m_UpgradeOptions[Mathf.Clamp (index, 0, m_UpgradeOptions.Length - 1)];
        PlayerUpgrades.Apply (option);
        string enemyNote = PlayerUpgrades.CompleteLevel ();

        m_VictoryPhase = VictoryPhase.Confirmed;
        m_ConfirmTimer = 0f;
        gameHud?.ShowUpgradeResult (option.Label, enemyNote);
    }

    void UpdateCaughtSequence ()
    {
        if (m_VictoryPhase != VictoryPhase.Inactive)
        {
            m_VictoryPhase = VictoryPhase.Inactive;
            gameHud?.HideUpgradeChoice ();
        }

        if (!m_HasCaughtAudioPlayed)
        {
            if (caughtAudio != null)
                caughtAudio.Play ();
            m_HasCaughtAudioPlayed = true;
        }

        m_CaughtTimer += Time.deltaTime;
        FadeIn (caughtBackgroundImageCanvasGroup, m_CaughtTimer);

        if (m_CaughtTimer > fadeDuration + displayImageDuration)
            RestartLevelAfterCaught ();
    }

    void FadeIn (CanvasGroup imageCanvasGroup, float elapsed)
    {
        if (imageCanvasGroup != null)
            imageCanvasGroup.alpha = elapsed / fadeDuration;
    }

    void ResetLevelAfterVictory ()
    {
        SceneManager.LoadScene (0);
    }

    void RestartLevelAfterCaught ()
    {
        GameSession.Clear ();
        SceneManager.LoadScene (0);
    }
}
