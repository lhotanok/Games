using Gamekit2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesEffects : MonoBehaviour
{
    private static readonly float[] randomRatioValues = { 2f / 3f, 3f / 2f, 3f / 4f, 4f / 3f };
    private static readonly bool[] ratioEffect = { true, false, true };


    private float duration = 30;

    private float minSpeed = 2;
    private float maxSpeed = 20;

    private float minGravity = 1;
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
            PlayerCharacter.PlayerInstance.StartCoroutine(DisableRangedAttackEffect());
        }
    }

    public void UseRandomEffect()
    {
        bool useRatio = ratioEffect[UnityEngine.Random.Range(0, ratioEffect.Length)];

        if (useRatio)
        {
            Func<float, IEnumerator>[] ratioEffects = { ApplySpeedEffect, ApplyGravityEffect };

            var randomEffect = ratioEffects[UnityEngine.Random.Range(0, ratioEffects.Length)];

            var randomRatioIndex = UnityEngine.Random.Range(0, randomRatioValues.Length);
            var randomRatio = randomRatioValues[randomRatioIndex];

            print($"randomRatioIndex: {randomRatioIndex}, random ratio: {randomRatio}");
            foreach (var ratio in randomRatioValues)
            {
                print(ratio);
            }

            PlayerCharacter.PlayerInstance.StartCoroutine(randomEffect(randomRatio));
        }
        else
        {
            Func<IEnumerator>[] nonRatioEffects = {
                DisableRangedAttackEffect,
                EnableRangedAttackEffect,
                DisableMeleeAttackEffect,
                EnableMeleeAttackEffect,
            };

            var randomEffect = nonRatioEffects[UnityEngine.Random.Range(0, nonRatioEffects.Length)];
            PlayerCharacter.PlayerInstance.StartCoroutine(randomEffect());
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

        if (modifiedSpeed >= minSpeed && modifiedSpeed <= maxSpeed)
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

        if (modifiedGravity >= minGravity && modifiedGravity <= maxGravity)
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

    private void SafeEnableRangedAttack()
    {
        if (!PlayerInput.Instance.RangedAttack.Enabled)
        {
            PlayerInput.Instance.RangedAttack.Enable();
            print("Ranged attack enabled");
        }
    }

    private void SafeDisableRangedAttack()
    {
        if (PlayerInput.Instance.RangedAttack.Enabled)
        {
            PlayerInput.Instance.RangedAttack.Disable();
            print("Ranged attack disabled");
        }
    }

    private void SafeEnableMeleeAttack()
    {
        if (!PlayerCharacter.PlayerInstance.meleeDamager.enabled)
        {
            PlayerCharacter.PlayerInstance.EnableMeleeAttack();
            print("Melee attack enabled");
        }
    }

    private void SafeDisableMeleeAttack()
    {
        if (PlayerCharacter.PlayerInstance.meleeDamager.enabled)
        {
            PlayerCharacter.PlayerInstance.DisableMeleeAttack();
            print("Melee attack disabled");
        }
    }

    private IEnumerator DisableRangedAttackEffect()
    {
        // apply effect
        SafeDisableRangedAttack();

        yield return new WaitForSeconds(duration);

        // remove effect
        SafeEnableRangedAttack();
        print("Ranged attack enabled");
    }

    private IEnumerator EnableRangedAttackEffect()
    {
        // apply effect
        SafeEnableRangedAttack();

        yield return new WaitForSeconds(duration);
    }

    private IEnumerator EnableMeleeAttackEffect()
    {
        // apply effect
        SafeEnableMeleeAttack();

        yield return new WaitForSeconds(duration);
    }

    private IEnumerator DisableMeleeAttackEffect()
    {
        // apply effect
        SafeDisableMeleeAttack();

        yield return new WaitForSeconds(duration);

        // remove effect
        SafeEnableMeleeAttack();
    }
}
