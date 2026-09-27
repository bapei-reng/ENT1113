using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    public float victoryResetDelay = 3f;
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
        if (m_IsPlayerAtExit)
            UpdateVictorySequence ();
        else if (m_IsPlayerCaught)
            UpdateCaughtSequence ();
    }

    void UpdateVictorySequence ()
    {
        if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))
        {
            ResetLevelAfterVictory ();
            return;
        }

        if (!m_HasVictoryAudioPlayed)
        {
            if (exitAudio != null)
                exitAudio.Play ();
            m_HasVictoryAudioPlayed = true;
        }

        m_VictoryTimer += Time.deltaTime;
        FadeIn (exitBackgroundImageCanvasGroup, m_VictoryTimer);

        if (m_VictoryTimer > victoryResetDelay)
            ResetLevelAfterVictory ();
    }

    void UpdateCaughtSequence ()
    {
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
