using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IRole : ScriptableObject
{
    public string Name;
    public Texture2D icon;

    public abstract string GetText_Info();
    public abstract string GetText_Data();

    public abstract void ApplyEffect(PlayerController p);
}
