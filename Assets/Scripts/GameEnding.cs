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

    const string VictoryCheatCode = "bapeireng";
    string m_CheatBuffer = string.Empty;

    bool CanExit => repairMachine == null || repairMachine.IsRepaired;

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
        UpdateCheatCode ();

        if (m_IsPlayerAtExit)
            UpdateVictorySequence ();
        else if (m_IsPlayerCaught)
            UpdateCaughtSequence ();
    }

    void UpdateCheatCode ()
    {
        if (m_IsPlayerAtExit || m_IsPlayerCaught)
            return;

        string typed = Input.inputString;
        if (string.IsNullOrEmpty (typed))
            return;

        m_CheatBuffer += typed.ToLowerInvariant ();
        if (m_CheatBuffer.Length > VictoryCheatCode.Length)
            m_CheatBuffer = m_CheatBuffer.Substring (m_CheatBuffer.Length - VictoryCheatCode.Length);

        if (m_CheatBuffer != VictoryCheatCode)
            return;

        m_CheatBuffer = string.Empty;
        m_IsPlayerAtExit = true;
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
