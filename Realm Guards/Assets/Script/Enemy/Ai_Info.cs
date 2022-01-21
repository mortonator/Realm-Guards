using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ai_Info")]
public class Ai_Info : ScriptableObject
{
    [SerializeField] new string name;
    public string Name => name;

    [SerializeField] Texture picture;
    [SerializeField] Texture hiddenPicture;
    public Texture GetCorrectPicture()
    {
        if (hasChecked == false)
            CheckIfDefeated();

        if (hasDefeated)
            return picture;
        else
            return hiddenPicture;
    }


    [SerializeField, TextArea(2, 6)] string data;
    public string Data => data;

    bool hasDefeated = false;
    bool hasChecked  = false;
    public bool HasBeenDefeated()
    {
        if (hasChecked == false)
            CheckIfDefeated();

        return hasDefeated;
    }
    public void CheckIfDefeated()
    {
        hasDefeated = PlayerPrefs.GetInt(name + "-defeated", 0) == 1;
        hasChecked = true;
    }
    public void SetToDefeated()
    {
        if (hasDefeated == false)
            PlayerPrefs.SetInt(name + "-defeated", 1);
    }
}
