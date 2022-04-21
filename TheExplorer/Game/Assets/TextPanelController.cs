using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPanelController : MonoBehaviour
{
    public Canvas Canvas;
    public RectTransform rectTransform;

    public float xPositionRatio;
    public float yPositionRatio;

    public float scaleRatio;
    // Start is called before the first frame update
    void Start()
    {
        SetPanelRectangle();
    }

    // Update is called once per frame
    void Update()
    {
        SetPanelRectangle();
    }

    private void SetPanelRectangle()
    {
        //  print($"Width: {Canvas.renderingDisplaySize.x}, Height: {Canvas.renderingDisplaySize.y}");

        var screenWidth = Canvas.renderingDisplaySize.x;
        var screenHeight = Canvas.renderingDisplaySize.y;

        rectTransform.localScale = new Vector2(screenWidth * scaleRatio, screenWidth * scaleRatio);
        rectTransform.anchoredPosition = new Vector2(xPositionRatio, screenHeight * yPositionRatio);
    }
}
