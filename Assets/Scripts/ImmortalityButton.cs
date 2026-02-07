using System;
using UnityEngine;

public class ImmortalityButton : MonoBehaviour
{
    public MonsterBehaviour monsterBehaviour;
    public PeepersBehaviour peepersBehaviour;

    void Start()
    {
        Debug.Log("ImmortalityButton script started.");
    }

    public void ImmortalityChange()
    {
        Debug.Log("Immortality button pressed.");
        // Send tag to InitialiseSimon
        if (monsterBehaviour != null)
        {
            if (monsterBehaviour.LoadDeathScene)
            {
                monsterBehaviour.LoadDeathScene = false;
                peepersBehaviour.LoadDeathScene = false;
                Debug.Log("Player is now IMMORTAL.");
            }
            else
            {
                monsterBehaviour.LoadDeathScene = true;
                peepersBehaviour.LoadDeathScene = true;
                Debug.Log("Player can now DIE.");
            }
        }
    }
}