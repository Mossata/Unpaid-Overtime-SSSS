using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlayerFOVRaycast : MonoBehaviour
{

    [Header("Monster Locations")]
    [Tooltip("Assign the 4 monster location transforms here")]
    public Transform[] monsterLocations;
    [Tooltip("How many times the raycast hit each monsterLocation this frame")]
    public int[] monsterLocationHitCounts;
    [Header("FOV Settings")]
    [Tooltip("Total width of the FOV cone in degrees")]
    public float viewAngle = 4f;

    [Tooltip("Distance rays are cast")]
    public float viewDistance = 100f;

    [Tooltip("Number of rays in the cone (higher = more coverage, impacts performance)")]
    public int rayCount = 5;

    [Tooltip("Layer mask to raycast against (should include monster's layer)")]
    public LayerMask layerMask = Physics.DefaultRaycastLayers;
    
    [Tooltip("Layers that raycast will pass through (won't be detected)")]
    public LayerMask ignoreLayerMask;

    [Header("Debug")]
    public Color rayColor = Color.green;
    public Color hitColor = Color.red;
    public bool debugDraw = true;

    private MonsterCollisionHandler lastHitMonsterHandler;
    private PeepersCollisionHandler lastHitPeepersHandler;
    public MonsterBehaviour monsterBehaviour;

    void Update()
    {
        CastRaysInCone();
    }

    void CastRaysInCone()
    {
        bool playerIsSeeingMonster = false;
        bool playerIsSeeingPeepers = false;

        // Horizontal cone (around transform.up)
        CastCone(transform.up, ref playerIsSeeingMonster, ref playerIsSeeingPeepers);

        // Vertical cone (around transform.right)
        CastCone(transform.right, ref playerIsSeeingMonster, ref playerIsSeeingPeepers);

        if (!playerIsSeeingMonster && lastHitMonsterHandler != null)
        {
            lastHitMonsterHandler.OnLostSightOfPlayer();
            lastHitMonsterHandler = null;
        }
        if (!playerIsSeeingPeepers && lastHitPeepersHandler != null)
        {
            lastHitPeepersHandler.OnLostSightOfPlayer();
            lastHitPeepersHandler = null;
        }
    }

    void CastCone(Vector3 rotationAxis, ref bool playerIsSeeingMonster, ref bool playerIsSeeingPeepers)
    {
        float angleStep = viewAngle / (rayCount - 1);
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.AngleAxis(angle, rotationAxis) * transform.forward;

            // Create final layer mask by excluding ignore layers
            LayerMask finalLayerMask = layerMask & ~ignoreLayerMask;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, viewDistance, finalLayerMask))
            {
                if (monsterBehaviour != null && monsterBehaviour.isAgentControlled)
                {
                    if (monsterLocations != null && monsterLocations.Length > 0)
                    {
                        for (int j = 0; j < monsterLocations.Length; j++)
                        {
                            if (hit.transform == monsterLocations[j])
                            {
                                monsterLocationHitCounts[j]++;
                                //Debug.Log("Monster location hit count: " + j + " = " + monsterLocationHitCounts[j]);
                            }
                        }
                    }
                }

                MonsterCollisionHandler monsterHandler = hit.collider.GetComponent<MonsterCollisionHandler>();
                if (monsterHandler != null)
                {
                    monsterHandler.OnSeenByPlayer();
                    lastHitMonsterHandler = monsterHandler;
                    playerIsSeeingMonster = true;

                    if (debugDraw)
                        Debug.DrawRay(transform.position, direction * hit.distance, hitColor);
                    return;
                }

                PeepersCollisionHandler peepersHandler = hit.collider.GetComponent<PeepersCollisionHandler>();
                if (peepersHandler != null)
                {
                    peepersHandler.OnSeenByPlayer();
                    lastHitPeepersHandler = peepersHandler;
                    playerIsSeeingPeepers = true;

                    if (debugDraw)
                        Debug.DrawRay(transform.position, direction * hit.distance, hitColor);
                    return;
                }
            }

            if (debugDraw)
                Debug.DrawRay(transform.position, direction * viewDistance, rayColor);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = rayColor;

        // Horizontal cone
        Vector3 leftRay = Quaternion.AngleAxis(-viewAngle / 2f, transform.up) * transform.forward;
        Vector3 rightRay = Quaternion.AngleAxis(viewAngle / 2f, transform.up) * transform.forward;
        Gizmos.DrawRay(transform.position, leftRay * viewDistance);
        Gizmos.DrawRay(transform.position, rightRay * viewDistance);
        DrawGizmoArc(transform.position, viewDistance, -viewAngle / 2f, viewAngle, transform.up);

        // Vertical cone
        Vector3 downRay = Quaternion.AngleAxis(-viewAngle / 2f, transform.right) * transform.forward;
        Vector3 upRay = Quaternion.AngleAxis(viewAngle / 2f, transform.right) * transform.forward;
        Gizmos.DrawRay(transform.position, downRay * viewDistance);
        Gizmos.DrawRay(transform.position, upRay * viewDistance);
        DrawGizmoArc(transform.position, viewDistance, -viewAngle / 2f, viewAngle, transform.right);
    }

    void DrawGizmoArc(Vector3 center, float radius, float startAngle, float angleLength, Vector3 axis)
    {
        int segments = 12;
        float deltaAngle = angleLength / segments;
        Vector3 lastPoint = center + Quaternion.AngleAxis(startAngle, axis) * transform.forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = startAngle + i * deltaAngle;
            Vector3 nextPoint = center + Quaternion.AngleAxis(angle, axis) * transform.forward * radius;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}