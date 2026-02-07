using UnityEngine;

public class LockCameraPosition : MonoBehaviour
{
    public Vector3 lockedPosition = Vector3.zero;
    public bool lockOnStart = true;
    
    public bool lockX = true;
    public bool lockY = true;
    public bool lockZ = true;
    
    private Vector3 initialPosition;
    
    void Start()
    {
        if (lockOnStart)
        {
            lockedPosition = transform.position;
        }
        initialPosition = lockedPosition;
    }
    
    void Update()
    {
        Vector3 currentPos = transform.position;
        
        if (lockX) currentPos.x = lockedPosition.x;
        if (lockY) currentPos.y = lockedPosition.y;
        if (lockZ) currentPos.z = lockedPosition.z;
        
        transform.position = currentPos;
    }
    
    public void SetLockedPosition(Vector3 newPosition)
    {
        lockedPosition = newPosition;
    }
    
    public void ResetToInitialPosition()
    {
        lockedPosition = initialPosition;
    }
}