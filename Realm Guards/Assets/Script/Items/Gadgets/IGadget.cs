using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGadget : ScriptableObject
{
    public Sprite iconSprite;
    public Sprite iconBack;
    public string interactText;
    [Range(1, 15)] public int value;

    public abstract void ApplyEffect(PlayerController p);

    public static Gradient gradientValueColour;
    public Color GetValueColour() => gradientValueColour.Evaluate(value / 15f);
}
