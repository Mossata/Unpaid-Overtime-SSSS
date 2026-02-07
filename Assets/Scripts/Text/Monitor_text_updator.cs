using UnityEngine;
using TMPro;

public class Monitor_text_updator : MonoBehaviour
{
    public TextMeshPro Monitor_text;
    int count = 0;

    public void showNextText()
    {
        Debug.Log("help!!!!");
        if (count == 0)
        {
            Monitor_text.text = "Look to the left";
            ++count;
        }
        else if (count == 1)
        {
            Monitor_text.text = "Look to the right";
            ++count;
        }
        else if (count == 2)
        {
            Monitor_text.text = "Look behind the screen";
        }
    }
}
