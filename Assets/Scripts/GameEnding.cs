using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
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
    float m_Timer;
    bool m_HasAudioPlayed;

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
        {
            EndLevel (exitBackgroundImageCanvasGroup, false, exitAudio);
        }
        else if (m_IsPlayerCaught)
        {
            EndLevel (caughtBackgroundImageCanvasGroup, true, caughtAudio);
        }
    }

    void EndLevel (CanvasGroup imageCanvasGroup, bool doRestart, AudioSource audioSource)
    {
        if (!m_HasAudioPlayed)
        {
            audioSource.Play();
            m_HasAudioPlayed = true;
        }
            
        m_Timer += Time.deltaTime;
        imageCanvasGroup.alpha = m_Timer / fadeDuration;

        if (m_Timer > fadeDuration + displayImageDuration)
        {
            if (doRestart)
            {
                SceneManager.LoadScene (0);
            }
            else
            {
                Application.Quit ();
            }
        }
    }
}
