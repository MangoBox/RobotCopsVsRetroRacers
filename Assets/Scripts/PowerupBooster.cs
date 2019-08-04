using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Powerup", menuName = "PowerupBooster", order = 1)]
public class PowerupBooster : Powerup
{
    public override void Activate(GameController gc) {
        gc.remaining3xMove += 3;
    }
}
