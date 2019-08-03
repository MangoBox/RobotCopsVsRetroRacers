using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Powerup", menuName = "PowerupScore", order = 1)]
public class PowerupScore : Powerup
{
    public override void Activate(GameController gc) {
        gc.AddScoreMultiplier(5);
    }
}
