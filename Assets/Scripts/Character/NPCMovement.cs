using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WalkToDistantPoints : MonoBehaviour
{
    [Header("Wander-Einstellungen")]
    public float maxWanderDistance = 100f;
    public float minTravelTime = 10f; // Mindestzeit in Sekunden unterwegs
    public float arrivalThreshold = 0.5f;

    [Header("Drehung")]
    public float pauseBeforeTurn = 0.3f;
    public float pauseBetweenTurns = 2.0f;
    public float rotationDuration = 2.0f;
    public float pauseAfterTurn = 0.1f;
    public float finalRotationDuration = 0.5f;

    private NavMeshAgent agent;
    private bool isInArrivalRoutine = false;
    private bool hasStarted = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        StartCoroutine(InitialMove());
    }

    private void Update()
    {
        if (!isInArrivalRoutine &&
            hasStarted &&
            !agent.pathPending &&
            agent.remainingDistance <= arrivalThreshold)
        {
            StartCoroutine(ArrivalRoutine());
        }
    }

    private IEnumerator InitialMove()
    {
        PickAndGo(); // Ziel setzen

        // Anfangsrotation zur ersten Zielrichtung
        yield return RotateToDestination(finalRotationDuration);

        agent.isStopped = false;
        agent.updateRotation = true;
        hasStarted = true;
    }

    private IEnumerator RotateToRotation(Quaternion targetRot, float duration)
    {
        float elapsed = 0f;
        Quaternion start = transform.rotation;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(start, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
    }

    private IEnumerator ArrivalRoutine()
    {
        isInArrivalRoutine = true;
        agent.isStopped = true;
        agent.updateRotation = false;

        float turnStartTime = Time.time;

        // [MARK#1] Pause vor der ersten Drehung
        yield return new WaitForSeconds(pauseBeforeTurn);

        // [MARK#2] Erste 180° Drehung (Drehdauer: rotationDuration)
        yield return RotateOverTime(180f, rotationDuration);

        // [MARK#3] Wartezeit zwischen den Drehungen (hier 0.3s z. B.)
        yield return new WaitForSeconds(pauseBetweenTurns);

        // Zweite 180° Drehung zurück
        yield return RotateOverTime(-180f, rotationDuration);

        // Sofort das nächste Ziel wählen (kein zusätzliches Warten)
        PickAndGo();

        // [MARK#4] Direkt zum neuen Ziel ausrichten (nur 1x)
        Vector3 dir = (agent.destination - transform.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            yield return RotateToRotation(targetRot, finalRotationDuration);
        }

        agent.isStopped = false;
        agent.updateRotation = true;
        isInArrivalRoutine = false;
    }


    private IEnumerator RotateOverTime(float angle, float duration)
    {
        Quaternion start = transform.rotation;
        Quaternion end = start * Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = end;
    }

    private IEnumerator RotateToDestination(float duration)
    {
        Vector3 dir = (agent.destination - transform.position).normalized;
        if (dir.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
    }

    private void PickAndGo()
    {
        Vector3 start = transform.position;
        float requiredDistance = agent.speed * minTravelTime;

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * maxWanderDistance;
            randomDirection.y = 0;
            Vector3 candidate = start + randomDirection;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    float length = GetPathLength(path);
                    if (length >= requiredDistance)
                    {
                        agent.SetDestination(hit.position);
                        Debug.DrawLine(start, hit.position, Color.green, 5f);
                        return;
                    }
                }
            }
        }

        Debug.LogWarning("⚠️ Kein weit genug entfernter Zielpunkt gefunden.");
    }

    private float GetPathLength(NavMeshPath path)
    {
        float total = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            total += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return total;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxWanderDistance);
    }
}
