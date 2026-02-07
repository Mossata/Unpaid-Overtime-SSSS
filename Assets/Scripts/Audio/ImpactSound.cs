using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    // Play the sound effect when the object hit the ground
    public AudioSource impactSF;

    void HitTheGround(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 0.5)
        {
            impactSF.Play();
        }
    }
}
