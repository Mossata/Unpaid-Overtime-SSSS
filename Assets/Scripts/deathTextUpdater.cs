using UnityEngine;
using TMPro;

public class deathTextUpdater : MonoBehaviour {
    public TextMeshProUGUI deathText;

    void Update() {
        if (DeathIntegerAccess.deathInt == 1) // player died to monster
        {
            deathText.text = "PAY ATTENTION TO YOUR SURROUNDINGS";
        }
        else if (DeathIntegerAccess.deathInt == 2) // player was too loud
        {
            deathText.text = "KEEP QUIET";
        }
        else if (DeathIntegerAccess.deathInt == 3) // player died to peepers
        {
            deathText.text = "DON'T LET THEM WATCH YOU";
        }
        else
        {
            deathText.text = "";
        }
    }
}

