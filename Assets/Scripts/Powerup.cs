using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class Powerup : ScriptableObject
{
    public abstract void Activate(GameController gc);
    public string powerUpName;
    public string powerUpDescription;
    public Sprite icon;
    public Color bannerColor;
}
