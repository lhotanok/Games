using Gamekit2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    private static float extraSeconds = 0;

    private static readonly float lostLifePenalty = 15;
    private static readonly float usedTeleportPenalty = 5;
    private static readonly float collectedFlowerPenalty = 2;

    private static readonly float gainedLifeBonus = 5;
    private static readonly float chomperKilledBonus = 12;
    private static readonly float spitterKilledBonus = 16;

    // 5 minutes time limit, the resulting score is increased by the number of seconds below this limit
    private static readonly float timeLimit = 8 * 60;

    private float ElapsedSeconds { get => Time.timeSinceLevelLoad; }

    public float Score { get => timeLimit - ElapsedSeconds + extraSeconds; }

    public DialogueCanvasController dialogueCanvasController;
    
    public void DisplayScore()
    {
        var ellapsedSeconds = ElapsedSeconds;

        print($"Ellapsed seconds: {ellapsedSeconds}, extra seconds: {extraSeconds}");

        var dialogueText = $"Congratulations! Your score: {Score} seconds.\n" +
            $"Ellapsed time: {ellapsedSeconds} seconds, extra score: {extraSeconds}";

        dialogueCanvasController.ActivateCanvasWithText(dialogueText);
    }

    public void LostLife()
    {
        DecreaseSeconds(lostLifePenalty);
    }

    public void GainedLife()
    {
        IncreaseSeconds(gainedLifeBonus);
    }

    public void CollectedFlower()
    {
        DecreaseSeconds(collectedFlowerPenalty);
    }

    public void UsedTeleport()
    {
        DecreaseSeconds(usedTeleportPenalty);
    }

    public void ChomperKilled()
    {
        IncreaseSeconds(chomperKilledBonus);
    }

    public void SpitterKilled()
    {
        IncreaseSeconds(spitterKilledBonus);
    }

    private void IncreaseSeconds(float seconds)
    {
        extraSeconds += seconds;
    }

    private void DecreaseSeconds(float seconds)
    {
        extraSeconds -= seconds;
    }
}
