using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform cam;

    void Update()
    {
        transform.position = cam.position;
        transform.rotation = cam.rotation;

    }
}
