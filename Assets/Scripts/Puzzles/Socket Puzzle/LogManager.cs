using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class LogManager : MonoBehaviour
{
    [Header("Hole GameObjects")]
    [SerializeField] private GameObject hole1;
    [SerializeField] private GameObject hole2;
    [SerializeField] private GameObject hole3;

    [Header("Materials")]
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material greyMaterial;

    [Header("Slot GameObjects")]
    [SerializeField] private GameObject slot1;
    [SerializeField] private GameObject slot2;
    [SerializeField] private GameObject slot3;

    [SerializeField]
    private TextMeshProUGUI textElement;

    public List<string> snappedNames = new List<string>();
    private readonly List<string> targetNames = new List<string> { "Blue Snap", "Green Snap", "Red Snap" };

    public MonsterAgent monsterAgent;

    public LightingLogic lightingLogic;
    public GameFlow gameFlow;
    public AudioSource chargingSFX;
    private void Start()
    {
        if (slot1 != null) slot1.SetActive(true);
        if (slot2 != null) slot2.SetActive(false);
        if (slot3 != null) slot3.SetActive(false);
    }
    public void firstHoleWhite()
    {
        if (hole1 != null && whiteMaterial != null)
        {
            var renderer = hole1.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = whiteMaterial;
                Debug.Log("Hole 1 material set to white!");
            }
        }
    }

    public void AddSnappedName(string name)
    {
        if (!snappedNames.Contains(name))
        {
            snappedNames.Add(name);
            Debug.Log($"Added: {name}");
            Debug.Log($"Snapped names: [{string.Join(", ", snappedNames)}]");
            if (chargingSFX != null)
            {
                chargingSFX.Play();
            }
            if (textElement != null)
            {
                string[] parts = name.Split(' ');
                string color = parts.Length > 0 ? parts[0] : name;
                textElement.text = $"Added {color} cell";
            }
            if (snappedNames.Count == 1 && slot2 != null)
            {
                if (hole1 != null && greyMaterial != null)
                {
                    var renderer = hole1.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = greyMaterial;
                        Debug.Log("Hole 1 material set to grey!");
                    }
                }
                slot2.SetActive(true);
                Debug.Log("Slot 2 enabled!");
                if (hole2 != null && whiteMaterial != null)
                {
                    var renderer = hole2.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = whiteMaterial;
                        Debug.Log("Hole 2 material set to white!");
                    }
                }
                gameFlow.CellInserted();
            }
            if (snappedNames.Count == 2 && slot3 != null)
            {
                if (hole2 != null && greyMaterial != null)
                {
                    var renderer = hole2.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = greyMaterial;
                        Debug.Log("Hole 2 material set to grey!");
                    }
                }
                slot3.SetActive(true);
                Debug.Log("Slot 3 enabled!");
                if (hole3 != null && whiteMaterial != null)
                {
                    var renderer = hole3.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = whiteMaterial;
                        Debug.Log("Hole 3 material set to white!");
                    }
                }
                gameFlow.CellInserted();
            }
            if (snappedNames.Count == 3)
            {
                if (hole3 != null && greyMaterial != null)
                {
                    var renderer = hole3.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = greyMaterial;
                        Debug.Log("Hole 3 material set to grey!");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"Name {name} already in list.");
        }

        Debug.Log($"Target list: [{string.Join(", ", targetNames)}]");
        Debug.Log($"Snapped names list: [{string.Join(", ", snappedNames)}]");

        if (snappedNames.Count == targetNames.Count && !targetNames.Except(snappedNames).Any())
        {
            Debug.Log("Yahoo yippee!!!");
            textElement.text = "Power: ON \nEscape while you can...";
            monsterAgent.HandlePlayerWin(-0.3f);
        }
        else
        {
            textElement.text = $" {snappedNames.Count}/3 Cells";
        }
    }

    public void ResetSnappedNames()
    {
        snappedNames.Clear();
        if (textElement != null)
            textElement.text = "";
    }
    public void ResetHolesAndSlots()
    {
        if (slot2 != null) slot2.SetActive(false);
        if (slot3 != null) slot3.SetActive(false);

        if (hole2 != null && whiteMaterial != null)
        {
            var renderer = hole2.GetComponent<Renderer>();
            if (renderer != null) renderer.material = greyMaterial;
        }
        if (hole3 != null && whiteMaterial != null)
        {
            var renderer = hole3.GetComponent<Renderer>();
            if (renderer != null) renderer.material = greyMaterial;
        }
    }
}