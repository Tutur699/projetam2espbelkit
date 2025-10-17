using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Items", menuName = "Scriptable Objects/Items")]
public class Items : ScriptableObject
{
    [Header("Item Properties")]
    public itemType type;
    public Sprite icon;
    public bool isDefaultItem = false;

}
public enum itemType { Fork, Knife, Gun};
