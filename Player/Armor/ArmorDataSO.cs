using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmorSprites
{
    public Sprite leftHand;
    public Sprite rightHand;
    public Sprite upperBody;
    public Sprite headAccessory;
    public Sprite leftArm;
    public Sprite rightArm;
    public Sprite backPack;
    public Sprite backLine;
    public Sprite head;
    public Sprite upBody;
    public Sprite body;
    public RuntimeAnimatorController legAnimController;
}

[CreateAssetMenu(menuName = "Gear/ArmorData")]
public class ArmorDataSO : ScriptableObject
{
    public string armorName;
    public string armorDescription;
    public Sprite armorSprite;
    public List<SkillPerLevel> passiveSkills;
    public List<SkillPerLevel> activeSkills;
    public Sprite backSprite;
    public ArmorSprites armorSprites;
}