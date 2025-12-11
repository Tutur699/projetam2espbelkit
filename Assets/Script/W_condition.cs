// Exemple de W_Condition.cs
using UnityEngine;

public class W_Condition : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager playerScript = other.GetComponent<PlayerManager>();
            
            if (playerScript != null)
            {
                playerScript.Win(); 
            }
        }
    }
}