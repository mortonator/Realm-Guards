using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Character Data")]
public class CharacterData : ScriptableObject
{
    [TextArea(1, 1)] public string character_name;
    public Texture2D character_image;
    [TextArea(3, 6)] public string character_details;
    [TextArea(5, 15)] public string character_backstory;
}