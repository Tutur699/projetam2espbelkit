using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Items", menuName = "Scriptable Objects/Items")]
public class Items : ScriptableObject
{
    [Header("Item Properties")]
    public itemType type;
    public ActionType action;
    public Sprite image;
    public bool isDefaultItem = false;
    public bool isStackable = true;

}
public enum itemType {Fork, Knife, Gun};
public enum ActionType {Attack, Utility};
