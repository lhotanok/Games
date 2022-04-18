using Gamekit2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesEffects : MonoBehaviour
{
    private float duration = 30;
    private float minSpeed = 2;
    private float maxGravity = 200;

    /// <summary>
    /// Changes player's speed for a specified duration.
    /// </summary>
    /// <param name="ratio">Use number between 0 and 1 for decrease and greater than 1 for increase.</param>
    public void ModifySpeed(float ratio)
    {
        if (AbilitiesCanBeModified())
        {
            PlayerCharacter.PlayerInstance.StartCoroutine(ApplySpeedEffect(ratio));
        }
    }

    /// <summary>
    /// Changes player's jumping height using gravity property for a specified duration.
    /// </summary>
    /// <param name="ratio">Use number between 0 and 1 for gravity decrease and greater than 1 for gravity increase.</param>
    public void ModifyGravity(float ratio)
    {
        if (AbilitiesCanBeModified())
        {
            PlayerCharacter.PlayerInstance.StartCoroutine(ApplyGravityEffect(ratio));
        }
    }

    public void DisableRangedAttack()
    {
        if (AbilitiesCanBeModified())
        {
            PlayerCharacter.PlayerInstance.StartCoroutine(ApplyRangedAttackEffect());
        }
    }

    public void SetDuration(float duration) => this.duration = duration;
    public void SetMinSpeed(float minSpeed) => this.minSpeed = minSpeed;
    public void SetMaxGravity(float maxGravity) => this.maxGravity = maxGravity;

    private bool AbilitiesCanBeModified()
    {
        var player = PlayerCharacter.PlayerInstance;

        return player.isActiveAndEnabled && !player.damageable.IsInvulnerable();
    }

    private IEnumerator ApplySpeedEffect(float ratio)
    {
        float modifiedSpeed = PlayerCharacter.PlayerInstance.maxSpeed * ratio;

        if (modifiedSpeed >= minSpeed)
        {
            // apply effect
            PlayerCharacter.PlayerInstance.maxSpeed = modifiedSpeed;
            print($"Changing max speed to: {PlayerCharacter.PlayerInstance.maxSpeed}");

            yield return new WaitForSeconds(duration);

            // remove effect
            PlayerCharacter.PlayerInstance.maxSpeed /= ratio;
            print($"Reverting max speed to: {PlayerCharacter.PlayerInstance.maxSpeed}");
        }
    }

    private IEnumerator ApplyGravityEffect(float ratio)
    {
        float modifiedGravity = PlayerCharacter.PlayerInstance.gravity * ratio;

        print($"Modified gravity: {modifiedGravity}, max gravity: {maxGravity}");

        if (modifiedGravity <= maxGravity)
        {
            // apply effect
            PlayerCharacter.PlayerInstance.gravity = modifiedGravity;
            print($"Changing gravity to: {PlayerCharacter.PlayerInstance.gravity}");

            yield return new WaitForSeconds(duration);

            // remove effect
            PlayerCharacter.PlayerInstance.gravity /= ratio;
            print($"Changing gravity to: {PlayerCharacter.PlayerInstance.gravity}");
        }
    }

    private IEnumerator ApplyRangedAttackEffect()
    {
        var originalBulletPool = new List<BulletObject>() { };
        originalBulletPool.AddRange(PlayerCharacter.PlayerInstance.bulletPool.pool);

        // apply effect
        PlayerInput.Instance.RangedAttack.Disable();
        print("Ranged attack disabled");

        yield return new WaitForSeconds(duration);

        // remove effect
        PlayerInput.Instance.RangedAttack.Enable();
        print("Ranged attack enabled");
    }
}
