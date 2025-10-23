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
    public bool isStackable = true;

    [Header("Weapon Properties")]
    public GameObject prefab; //The actual item object in the scene
    public float useRate = 0.1f;
    public bool singleUse = false;
    public float weaponDamage = 5; //How much damage should this weapon deal

}
public enum itemType {Fork, Knife, Gun};
public enum ActionType {Attack, Utility};
