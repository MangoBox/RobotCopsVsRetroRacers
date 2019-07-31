using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public Text movesText;

    public Text turnText;

    public Image imageBanner;

    [Header("Colors")]
    public Color playerColor;
    public Color copColor;

    public Canvas canvas;


    public void UpdateMoveCount(int count) {
        movesText.text = "Moves Left: " + count.ToString();
    }

    public void UpdateTurnUI(bool isPlayer) {
        turnText.text =  isPlayer ? "Players Turn" : "Cops Turn";
        imageBanner.color = isPlayer ? playerColor : copColor;
    }

    public void UpdateBanner(string heading) {
        turnText.text = heading;
    }

    public void UpdateBanner(string heading, string subtitle) {
        turnText.text = heading;
        movesText.text = subtitle;
    }

    public void UpdateBanner(string heading, string subtitle, Color color) {
        turnText.text = heading;
        movesText.text = subtitle;
        imageBanner.color = color;
    }
}
