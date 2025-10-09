

using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    //This script will keep track of player HP
    public float playerHP = 100;
    public PlayerControler playerControler;
    public WPManager weaponManager;

    public Texture crosshairTexture;


    public void ApplyDamage(float points)
    {
        playerHP -= points;

        if (playerHP <= 0)
        {
            //Player is dead
            playerControler.canMove = false;
            playerHP = 0;
        }
    }
    void OnGUI()
    {
        GUI.Box(new Rect(10, Screen.height - 35, 100, 25), ((int)playerHP).ToString() + " HP");
        if(playerHP <= 0)
        {
            GUI.Box(new Rect(Screen.width / 2 - 85, Screen.height / 2 - 20, 170, 40), "Game Over");
        }
        else
        {
            GUI.DrawTexture(new Rect(Screen.width / 2 - 3, Screen.height / 2 - 3, 6, 6), crosshairTexture);
        }

    }
}

