using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Powerup", menuName = "PowerupJail", order = 1)]
public class PowerupJail : Powerup
{
    public override void Activate(GameController gc) {
        gc.remainingEscapeCards += 1;
    }
}
