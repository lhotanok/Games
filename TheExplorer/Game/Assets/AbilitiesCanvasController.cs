using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilitiesCanvasController : MonoBehaviour
{
    public TextMeshProUGUI TextMeshProUGUI;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI.text = "Default abilities";
    }

    // Update is called once per frame
    void Update()
    {
        TextMeshProUGUI.text = AbilitiesEffects.AbilityUpdates;
    }
}
