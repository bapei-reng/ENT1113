using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    public Transform player;
    public GameEnding gameEnding;

    [SerializeField] private float viewRadius = 2.4f;
    [SerializeField] private float viewAngle = 40f;

    float EffectiveRadius => viewRadius + PlayerUpgrades.EnemyRadiusBonus;
    float EffectiveHalfAngle => (viewAngle + PlayerUpgrades.EnemyAngleBonus) * 0.5f;

    void Update ()
    {
        if (player == null || gameEnding == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flat = new Vector3 (toPlayer.x, 0f, toPlayer.z);
        float distance = flat.magnitude;
        if (distance > EffectiveRadius)
            return;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (distance > 0.001f && forward.sqrMagnitude > 0.001f &&
            Vector3.Angle (forward, flat) > EffectiveHalfAngle)
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
