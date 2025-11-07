using UnityEngine;

//Entity interafce
public interface IEntity
{
    void ApplyDamage(float points);
}

//Pickable item interface
public interface IPickable
{
    void PickItem();
}