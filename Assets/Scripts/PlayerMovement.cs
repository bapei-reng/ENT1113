using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 20f;
    public float sprintSpeedMultiplier = 3f;
    public float sprintHoldDuration = 1f;
    public float sprintDuration = 2f;
    public float sprintCooldown = 15f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    AudioSource m_AudioSource;
    PlayerInteraction m_Interaction;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;
    float m_SprintStartTime = float.NegativeInfinity;
    float m_SprintReadyTime = float.NegativeInfinity;
    float m_BaseSpeedMultiplier = 1f;

    public bool IsSprintReady => Time.time >= m_SprintReadyTime;
    public bool IsSprinting => Time.time - m_SprintStartTime < sprintDuration;
    public float SprintRemaining => Mathf.Max (0f, m_SprintStartTime + sprintDuration - Time.time);
    public float SprintCooldownRemaining => Mathf.Max (0f, m_SprintReadyTime - Time.time);

    public float SpeedMultiplier
    {
        get
        {
            float elapsed = Time.time - m_SprintStartTime;
            if (elapsed >= sprintDuration)
                return 1f;

            if (elapsed <= sprintHoldDuration)
                return sprintSpeedMultiplier;

            float decayDuration = sprintDuration - sprintHoldDuration;
            if (decayDuration <= 0f)
                return 1f;

            return Mathf.Lerp (sprintSpeedMultiplier, 1f, (elapsed - sprintHoldDuration) / decayDuration);
        }
    }

    void Start ()
    {
        m_Animator = GetComponent<Animator> ();
        m_Rigidbody = GetComponent<Rigidbody> ();
        m_AudioSource = GetComponent<AudioSource> ();
        m_Interaction = GetComponent<PlayerInteraction> ();

        m_BaseSpeedMultiplier = PlayerUpgrades.SpeedFactor;
        sprintSpeedMultiplier += PlayerUpgrades.SprintSpeedBonus;
        sprintHoldDuration += PlayerUpgrades.SprintDurationBonus;
        sprintDuration += PlayerUpgrades.SprintDurationBonus;
        sprintCooldown = Mathf.Max (sprintCooldown - PlayerUpgrades.SprintCooldownReduction, sprintHoldDuration);
    }

    void Update ()
    {
        if (Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift))
            TryStartSprint ();

        m_Animator.speed = SpeedMultiplier * m_BaseSpeedMultiplier;
    }

    void TryStartSprint ()
    {
        if (!IsSprintReady)
            return;

        m_SprintStartTime = Time.time;
        m_SprintReadyTime = Time.time + sprintCooldown;
    }

    void FixedUpdate ()
    {
        float horizontal;
        float vertical;

        if (m_Interaction != null && m_Interaction.IsRepairing)
        {
            horizontal = ReadWasdAxis (KeyCode.A, KeyCode.D);
            vertical = ReadWasdAxis (KeyCode.S, KeyCode.W);
        }
        else
        {
            horizontal = Input.GetAxis ("Horizontal");
            vertical = Input.GetAxis ("Vertical");
        }
        
        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize ();

        bool hasHorizontalInput = !Mathf.Approximately (horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately (vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool ("IsWalking", isWalking);
        
        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }
        else
        {
            m_AudioSource.Stop ();
        }

        Vector3 desiredForward = Vector3.RotateTowards (transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation (desiredForward);
    }

    float ReadWasdAxis (KeyCode negative, KeyCode positive)
    {
        float value = 0f;
        if (Input.GetKey (positive))
            value += 1f;
        if (Input.GetKey (negative))
            value -= 1f;
        return value;
    }

    void OnAnimatorMove ()
    {
        m_Rigidbody.MovePosition (m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation (m_Rotation);
    }
}
