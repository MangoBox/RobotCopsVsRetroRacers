using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{

    public Text movesText;

    public Text turnText;

    public Image imageBanner;

    [Header("Colors")]
    public Color playerColor;
    public Color copColor;

    public Canvas canvas;

    public Animator gameOverAnimator;

    public Text scoreText;
    public Text highScoreMultiplierText;

    public void UpdateMoveCount(int count) {
        movesText.text = count.ToString();
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

    public void DisplayGameOverScreen() {
        gameOverAnimator.SetTrigger("TriggerFadeIn");
    }

    public void RetryButton() {
        SceneManager.LoadScene("MainScene");
    }

    public void MainMenuButton() {
        SceneManager.LoadScene("MainMenu");
    }

    public void SetScoreText(int amount) {
        scoreText.text = "Score: " + amount.ToString();
        scoreText.GetComponent<Animator>().SetTrigger("ScorePop");
    }

    public void SetHighScore(int amount) {
        string t = highScoreMultiplierText.text;
        string formattedAmount = string.Format("{0:n0}",amount);
        string[] parts = t.Split('|');
        parts[0] = "High Score: " + formattedAmount + " ";
        highScoreMultiplierText.text = parts[0] + "|" + parts[1];
    }

    public void SetScoreMultiplier(int amount) {
        string t = highScoreMultiplierText.text;
        string formattedAmount = string.Format("{0:n0}",amount);
        string[] parts = t.Split('|');
        parts[1] = " MULTIPLIER: " + formattedAmount + "x";
        highScoreMultiplierText.text = parts[0] + "|" +parts[1];
    }
}
