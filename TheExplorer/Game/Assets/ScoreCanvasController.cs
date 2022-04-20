using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreCanvasController : MonoBehaviour
{
    public ScoreController ScoreController;
    public TextMeshProUGUI TextMeshProUGUI;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI.text = $"{ScoreController.Score} seconds";
    }

    // Update is called once per frame
    void Update()
    {
        TextMeshProUGUI.text = $"{ScoreController.Score:0.00} seconds";
    }
}
