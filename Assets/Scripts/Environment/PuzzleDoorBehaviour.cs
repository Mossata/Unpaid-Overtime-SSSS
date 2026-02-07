using UnityEngine;

public class PuzzleDoorBehaviour : MonoBehaviour
{
    // Info needed for door opening and closing
    public Transform Door1;
    public Transform Door2;
    
    private Vector3 door1ClosedPos;
    private Vector3 door2ClosedPos;
    private Vector3 door1OpenPos;
    private Vector3 door2OpenPos;
    
    public float slideDistance;  // how far each half should slide
    public float slideSpeed;       // how fast it slides

    private bool isOpen = false;
    private Coroutine currentCoroutine;
    
    // Info needed for keeping track of puzzles 
    public PuzzleMovement[] puzzles;
    private PuzzleMovement currentPuzzle;
    private int currentPuzzleIndex;
    public AudioSource doorOpeningSF;
    public AudioSource hydraulicSF;

    void Start()
    {
        // closed positions
        door1ClosedPos = Door1.localPosition;
        door2ClosedPos = Door2.localPosition;

        // local "up" relative to parent
        Vector3 localUp1 = Door1.parent.InverseTransformDirection(Door1.up).normalized;
        Vector3 localUp2 = Door2.parent.InverseTransformDirection(Door2.up).normalized;

        // open positions offset along local up/down
        door1OpenPos = door1ClosedPos + localUp1 * slideDistance;
        door2OpenPos = door2ClosedPos - localUp2 * slideDistance;
        
        // Set first puzzle to be shown
        currentPuzzleIndex = 0;
        currentPuzzle = puzzles[currentPuzzleIndex];
        
        // Set all puzzles except the first as not active
        for (int i = 1; i < puzzles.Length; i++)
        {
            puzzles[i].puzzle.SetActive(false);
        }
    }

    public System.Collections.IEnumerator OpenDoor(bool andClose)
    {   
        if (doorOpeningSF != null && hydraulicSF != null)
        {
            Debug.Log("Playing doorOpeningSF.");
            doorOpeningSF.Play();
            hydraulicSF.Play();
        }
        while (Door1.localPosition != door1OpenPos)
        {
            Door1.localPosition = Vector3.MoveTowards(Door1.localPosition, door1OpenPos, slideSpeed * Time.deltaTime);
            Door2.localPosition = Vector3.MoveTowards(Door2.localPosition, door2OpenPos, slideSpeed * Time.deltaTime);
            yield return null;
        }
        // Move puzzle into the doorway
        if (!currentPuzzle.isFinalPuzzle)
        {
            StartCoroutine(currentPuzzle.MovePuzzleIn());
        }

        if (andClose)
        {
            StartCoroutine(CloseDoor(false));
        }
    }

    public System.Collections.IEnumerator CloseDoor(bool andOpen)
    {
        if (doorOpeningSF != null && hydraulicSF != null)
        {
            Debug.Log("Playing doorOpeningSF.");
            doorOpeningSF.Play();
            hydraulicSF.Play();
        }
        // Move puzzle out of the way before closing
        if (!currentPuzzle.isFinalPuzzle)
        {
            StartCoroutine(currentPuzzle.MovePuzzleBack());
        }
        yield return new WaitForSeconds(0.5f);
        
        while (Door1.localPosition != door1ClosedPos)
        {
            Door1.localPosition = Vector3.MoveTowards(Door1.localPosition, door1ClosedPos, slideSpeed * Time.deltaTime);
            Door2.localPosition = Vector3.MoveTowards(Door2.localPosition, door2ClosedPos, slideSpeed * Time.deltaTime);
            yield return null;
        }

        
        // Move to next puzzle in list. If end of list is reached, go to the start of list
        currentPuzzle.puzzle.SetActive(false);
        currentPuzzleIndex = (currentPuzzleIndex + 1) % puzzles.Length;
        currentPuzzle = puzzles[currentPuzzleIndex];
        currentPuzzle.puzzle.SetActive(true);

        if (andOpen)
        {
            yield return new WaitForSeconds(3f);
            StartCoroutine(OpenDoor(false));
        }
    }
    
    public System.Collections.IEnumerator FinalPuzzleSkip()
    {
        
        // Set final puzzle as active
        while (!currentPuzzle.isFinalPuzzle)
        {
            currentPuzzle.puzzle.SetActive(false);
            currentPuzzleIndex = (currentPuzzleIndex + 1) % puzzles.Length;
            currentPuzzle = puzzles[currentPuzzleIndex];
            currentPuzzle.puzzle.SetActive(true);
        }
        
        if (doorOpeningSF != null && hydraulicSF != null)
        {
            Debug.Log("Playing doorOpeningSF.");
            doorOpeningSF.Play();
            hydraulicSF.Play();
        }
        
        while (Door1.localPosition != door1OpenPos)
        {
            Door1.localPosition = Vector3.MoveTowards(Door1.localPosition, door1OpenPos, slideSpeed * Time.deltaTime);
            Door2.localPosition = Vector3.MoveTowards(Door2.localPosition, door2OpenPos, slideSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void UpdateDoor()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        
        // If door is open, close it/rotate puzzle 
        if (isOpen)
        {
           currentCoroutine = StartCoroutine(CloseDoor(false));
        }

        // If door is closed, open it 
        else
        {
            currentCoroutine = StartCoroutine(OpenDoor(false));
        }
        
        isOpen = !isOpen;
    }
}
