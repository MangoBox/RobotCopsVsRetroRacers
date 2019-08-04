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

    public Animator outroAnimator;

    public Text scoreText;
    public Text highScoreMultiplierText;

    public Text cityText;

    public Text copText;

    public Text tipText;

    public Text introCityText;

    public Text intoCitySubtitle;

    public Text gameOverScreenText;

    public string[] tips;
    public int remainingTips = 3;

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

    public void UpdateBanner(string heading, Color color) {
        turnText.text = heading;
        imageBanner.color = color;
    }

    public void DisplayGameOverScreen() {
        gameOverAnimator.SetTrigger("TriggerFadeIn");
    }

    public void RetryButton() {
        GameController.gc.ResetStaticVariables();
        SceneManager.LoadScene("MainScene");
    }

    public void MainMenuButton() {
        SceneManager.LoadScene("MainMenu");
    }

    public void SetScoreText(int amount) {
        string formattedAmount = string.Format("{0:n0}",amount);
        scoreText.text = "Score: " + formattedAmount;
        scoreText.GetComponent<Animator>().SetTrigger("ScorePop");
    }

    public void UpdateCityName(int cityLevel, string cityName) {
        cityText.text = "City " + cityLevel.ToString() + " - " + cityName;
    }

    public void UpdateCityIntro(int cityLevel, string cityName, int difficultyIndex) {
        //Also handles level difficulty.
        string[] difficultyNames = {"Easy","Medium","Hard","Extreme"};
        string difficulty = difficultyNames[difficultyIndex];
        
        introCityText.text = cityName;
        intoCitySubtitle.text = "City " + cityLevel.ToString() + " - Difficulty: " + difficulty;
        
    }

    public void SetCityOutro() {
        outroAnimator.SetTrigger("EndGame");
    }

    public void SetGameOverText(int score, int highscore, string cityName, int cityLevel) {
        string scoreFormatted = "Score: " + string.Format("{0:n0}",score) + "\n";
        string highscoreFormatted = "High Score: " + string.Format("{0:n0}",highscore) + "\n";
        string cityFormatted = "Reached City " + cityLevel + " - " + cityName;
        gameOverScreenText.text = scoreFormatted + highscoreFormatted + cityFormatted;
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

    public void SetCopAmount(int amount) {
        copText.text = "Cops Deployed: " + amount.ToString();
    }

    public void DisplayTip() {
        if(remainingTips <= 0) {
            return;
        }
        //TEMP DISABLED
        return;
        remainingTips--;
        string selectedTip = tips[Random.Range(0,tips.Length)];
        tipText.text = "Tip: " + selectedTip;
        tipText.GetComponentInParent<Animator>().SetTrigger("TipFadeIn");
    }

    public void CloseTip() {
        Animator a = tipText.GetComponentInParent<Animator>();
        a.ResetTrigger("TipFadeOut");
        if(!a.GetCurrentAnimatorStateInfo(0).IsName("TipIdleOut"))
            tipText.GetComponentInParent<Animator>().SetTrigger("TipFadeOut");
    }
}
