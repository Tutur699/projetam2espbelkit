using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Items", menuName = "Scriptable Objects/Items")]
public class Items : ScriptableObject
{
    [Header("Item Properties")]
    public ActionType action;
    public Sprite image;
    public bool isDefaultItem = false;

    [Header("Weapon Properties")]
    public GameObject prefab; 
    public float useRate = 0.1f;
    public bool singleUse = false;
    public float weaponDamage = 5;
    public bool isAutomatic = false;

}
public enum itemType {Fork, Knife, Rifle, Pistol, Sniper, Shotgun, SMG};
public enum ActionType {Attack, Utility};
