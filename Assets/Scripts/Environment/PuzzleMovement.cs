using UnityEngine;

public class PuzzleMovement : MonoBehaviour
{
    public GameObject puzzle;
    public bool isFinalPuzzle;
    private bool Active = false;
    
    public Transform puzzleTransform;
    private Vector3 puzzleInitialPos;
    private Vector3 puzzleFinalPos;

    private float slideSpeed = 0.3f;
    private float slideDistance = 0.1f;

    void Start()
    {
        puzzleInitialPos = puzzleTransform.localPosition;
        
        // local "up" relative to parent
        Vector3 localUp = puzzleTransform.parent.InverseTransformDirection(puzzleTransform.up).normalized;

        // open positions offset along local up/down
        puzzleFinalPos = puzzleInitialPos + localUp * slideDistance;
    }
    
    public System.Collections.IEnumerator MovePuzzleIn()
    {
        while (puzzleTransform.localPosition != puzzleFinalPos)
        {
            puzzleTransform.localPosition = Vector3.MoveTowards(puzzleTransform.localPosition, puzzleFinalPos, slideSpeed * Time.deltaTime);
            yield return null;
        }
    }
    
    public System.Collections.IEnumerator MovePuzzleBack()
    {
        while (puzzleTransform.localPosition != puzzleInitialPos)
        {
            puzzleTransform.localPosition = Vector3.MoveTowards(puzzleTransform.localPosition, puzzleInitialPos, slideSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
