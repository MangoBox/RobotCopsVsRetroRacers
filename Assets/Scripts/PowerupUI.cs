using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupUI : MonoBehaviour
{
    public Text headingText;
    public Text descriptionText;
    public Image powerupIcon;

    // Start is called before the first frame update
    public void ApplyPowerup(Powerup powerup) {
        headingText.text = powerup.powerUpName;
        descriptionText.text = powerup.powerUpDescription;
        powerupIcon.sprite = powerup.icon;

    }
}
