using Gamekit2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesEffects : MonoBehaviour
{
    private static readonly float[] randomRatioValues = {
        2f / 3f,
        3f / 2f, 
        3f / 4f,
        4f / 3f,
        4f / 5f,
        3f / 5f,
        5f / 4f,
        5f / 3f
    };

    private static readonly bool[] ratioEffect = { true, false, true };

    private float duration = 30;

    private float minSpeed = 2;
    private float maxSpeed = 20;
    private float originalSpeed = 8;

    private float minGravity = 1;
    private float maxGravity = 200;
    private float originalGravity = 80;

    private float minHeight = 0.3f;
    private float maxHeight = 2;

    private static List<string> effectIds = new List<string>();

    /// <summary>
    /// Changes player's speed for a specified duration.
    /// </summary>
    /// <param name="ratio">Use number between 0 and 1 for decrease and greater than 1 for increase.</param>
    public void ModifySpeed(float ratio)
    {
        if (IsPlayerActive())
        {
            var routine = ApplySpeedEffect(ratio);
            PlayerCharacter.PlayerInstance.StartCoroutine(routine);
        }
    }

    /// <summary>
    /// Changes player's jumping height using gravity property for a specified duration.
    /// </summary>
    /// <param name="ratio">Use number between 0 and 1 for gravity decrease and greater than 1 for gravity increase.</param>
    public void ModifyGravity(float ratio)
    {
        if (IsPlayerActive())
        {
            var routine = ApplyGravityEffect(ratio);
            PlayerCharacter.PlayerInstance.StartCoroutine(routine);
        }
    }

    public void DisableRangedAttack()
    {
        if (IsPlayerActive() && PlayerInput.Instance.RangedAttack.Enabled)
        {
            PlayerCharacter.PlayerInstance.StartCoroutine(DisableRangedAttackEffect());
        }
    }

    public void ResetAbilities()
    {
        print("Reseting abilities");
        effectIds.Clear();

        SafeEnableMeleeAttack();
        SafeEnableRangedAttack();

        var player = PlayerCharacter.PlayerInstance;

        player.maxSpeed = originalSpeed;
        player.gravity = originalGravity;
        player.transform.localScale = Vector3.one;
    }

    public void UseRandomEffect()
    {
        bool useRatio = ratioEffect[UnityEngine.Random.Range(0, ratioEffect.Length)];

        if (useRatio)
        {
            Func<float, IEnumerator>[] ratioEffects = {
                ApplySpeedEffect,
                ApplyGravityEffect,
                ApplySizeEffect,
           };

            var randomEffect = ratioEffects[UnityEngine.Random.Range(0, ratioEffects.Length)];

            var randomRatioIndex = UnityEngine.Random.Range(0, randomRatioValues.Length);
            var randomRatio = randomRatioValues[randomRatioIndex];

            print($"randomRatioIndex: {randomRatioIndex}, random ratio: {randomRatio}");
            foreach (var ratio in randomRatioValues)
            {
                print(ratio);
            }

            var routine = randomEffect(randomRatio);
            PlayerCharacter.PlayerInstance.StartCoroutine(routine);
        }
        else
        {
            Func<IEnumerator>[] nonRatioEffects = {
                DisableRangedAttackEffect,
                EnableRangedAttackEffect,
                GainHealth,
                DisableMeleeAttackEffect,
                EnableMeleeAttackEffect,
                GainHealth,
            };

            var randomEffect = nonRatioEffects[UnityEngine.Random.Range(0, nonRatioEffects.Length)];

            var routine = randomEffect();
            PlayerCharacter.PlayerInstance.StartCoroutine(routine);
        }
    }

    public void SetDuration(float duration) => this.duration = duration;
    public void SetMinSpeed(float minSpeed) => this.minSpeed = minSpeed;
    public void SetMaxGravity(float maxGravity) => this.maxGravity = maxGravity;

    private bool IsPlayerActive() => PlayerCharacter.PlayerInstance.isActiveAndEnabled;
    private bool NegativeEffectCanBeUsed() => !PlayerCharacter.PlayerInstance.damageable.IsInvulnerable();
    private bool EffectCanBeUsed(bool isNegativeEffect) => !isNegativeEffect || isNegativeEffect && NegativeEffectCanBeUsed();

    private IEnumerator ApplySpeedEffect(float ratio)
    {
        float modifiedSpeed = PlayerCharacter.PlayerInstance.maxSpeed * ratio;

        bool isNegativeEffect = ratio < 1;

        if (SpeedWithinRange(modifiedSpeed) && EffectCanBeUsed(isNegativeEffect))
        {
            // apply effect
            PlayerCharacter.PlayerInstance.maxSpeed = modifiedSpeed;
            print($"Changing max speed to: {PlayerCharacter.PlayerInstance.maxSpeed}");

            string effectId = Guid.NewGuid().ToString();
            effectIds.Add(effectId);

            yield return new WaitForSeconds(duration);

            // remove effect
            float revertedSpeed = PlayerCharacter.PlayerInstance.maxSpeed / ratio;
            if (SpeedWithinRange(revertedSpeed) && effectIds.Contains(effectId))
            {
                PlayerCharacter.PlayerInstance.maxSpeed = revertedSpeed;
                print($"Reverting max speed to: {PlayerCharacter.PlayerInstance.maxSpeed}");
            }
        }
    }

    private bool SpeedWithinRange(float speed) => speed >= minSpeed && speed <= maxSpeed;

    private IEnumerator ApplyGravityEffect(float ratio)
    {
        float modifiedGravity = PlayerCharacter.PlayerInstance.gravity * ratio;

        bool isNegativeEffect = ratio > 1;

        if (GravityWithinRange(modifiedGravity) && EffectCanBeUsed(isNegativeEffect))
        {
            // apply effect
            PlayerCharacter.PlayerInstance.gravity = modifiedGravity;
            print($"Changing gravity to: {PlayerCharacter.PlayerInstance.gravity}");

            string effectId = Guid.NewGuid().ToString();
            effectIds.Add(effectId);

            yield return new WaitForSeconds(duration);

            // remove effect
            float revertedGravity = PlayerCharacter.PlayerInstance.gravity / ratio;
            if (GravityWithinRange(revertedGravity) && effectIds.Contains(effectId))
            {
                PlayerCharacter.PlayerInstance.gravity = revertedGravity;
                print($"Reverting gravity to: {PlayerCharacter.PlayerInstance.gravity}");
            }
        }
    }

    private bool GravityWithinRange(float gravity) => gravity >= minGravity && gravity <= maxGravity;

    private IEnumerator ApplySizeEffect(float ratio)
    {
        var modifiedSize = Vector3.one * ratio;

        if (SizeWithinRange(modifiedSize))
        {
            // apply effect
            PlayerCharacter.PlayerInstance.transform.localScale = modifiedSize;
            print($"Changing height to: {modifiedSize.y}");

            string effectId = Guid.NewGuid().ToString();
            effectIds.Add(effectId);

            yield return new WaitForSeconds(duration);

            // remove effect
            if (effectIds.Contains(effectId))
            {
                PlayerCharacter.PlayerInstance.transform.localScale = Vector3.one;
                print($"Reverting height to: {Vector3.one.y}");
            }
        }
    }

    private bool SizeWithinRange(Vector3 size) => size.y >= minHeight && size.y <= maxHeight;

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
        if (NegativeEffectCanBeUsed() && PlayerInput.Instance.RangedAttack.Enabled)
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
        if (NegativeEffectCanBeUsed() && PlayerCharacter.PlayerInstance.meleeDamager.enabled)
        {
            PlayerCharacter.PlayerInstance.DisableMeleeAttack();
            print("Melee attack disabled");
        }
    }

    private IEnumerator GainHealth()
    {
        var randomHealthAmount = UnityEngine.Random.Range(1, 4);
        PlayerCharacter.PlayerInstance.damageable.GainHealth(randomHealthAmount);

        print($"Gained health: {randomHealthAmount}");

        yield return null;
    }

    private IEnumerator DisableRangedAttackEffect()
    {
        // apply effect
        SafeDisableRangedAttack();
        print("Ranged attack disabled before yield");

        yield return new WaitForSeconds(duration);

        // remove effect
        SafeEnableRangedAttack();
        print("Ranged attack enabled after yield");
    }

    private IEnumerator EnableRangedAttackEffect()
    {
        // apply effect
        SafeEnableRangedAttack();

        yield return null;
    }

    private IEnumerator EnableMeleeAttackEffect()
    {
        // apply effect
        SafeEnableMeleeAttack();

        yield return null;
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
