using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    public Transform player;
    public GameEnding gameEnding;

    [SerializeField] private float viewRadius = 2.4f;
    [SerializeField] private float viewAngle = 40f;

    // 敌人加强在关卡开始时结算一次，和玩家加成一样到下一关才生效。
    float m_EffectiveRadius;
    float m_EffectiveHalfAngle;

    void Start ()
    {
        m_EffectiveRadius = viewRadius + PlayerUpgrades.EnemyRadiusBonus;
        m_EffectiveHalfAngle = (viewAngle + PlayerUpgrades.EnemyAngleBonus) * 0.5f;
    }

    void Update ()
    {
        if (player == null || gameEnding == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3 (toPlayer.x, 0f, toPlayer.z);
        float distance = flat.magnitude;
        if (distance > m_EffectiveRadius)
            return;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (distance > 0.001f && forward.sqrMagnitude > 0.001f &&
            Vector3.Angle (forward, flat) > m_EffectiveHalfAngle)
            return;

        Vector3 direction = toPlayer + Vector3.up;
        Ray ray = new Ray (transform.position, direction);
        RaycastHit raycastHit;

        if (Physics.Raycast (ray, out raycastHit) && raycastHit.collider.transform == player)
        {
            gameEnding.CaughtPlayer ();
        }
    }
}
