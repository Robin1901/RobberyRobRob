using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WalkToDistantPoints : MonoBehaviour
{
    [Header("Wander-Einstellungen")]
    private float maxWanderDistance = 30f;        // Maximale Distanz für neues Ziel
    private float minTravelTime = 3f;             // Mindestzeit unterwegs
    private float arrivalThreshold = 0.5f;        // Ziel erreicht ab Distanz

    [Header("Drehung bei Ankunft (Basiswerte)")]
    private float pauseBeforeTurnMin = 0.75f;      // Min Wartezeit vor Drehung
    private float pauseBeforeTurnMax = 1.5f;      // Max Wartezeit vor Drehung
    private float rotationDuration = 2.5f;        // Dauer für Drehungen
    private float finalRotationDuration = 1f;     // Dauer Zielausrichtung
    private float pauseBeforeMoveMin = 1f;         // Min Wartezeit vor Loslaufen
    private float pauseBeforeMoveMax = 2.5f;       // Max Wartezeit vor Loslaufen

    // Drehwinkelbereiche (für zufällige Auswahl)
    private float firstTurnAngleMin = 20f;
    private float firstTurnAngleMax = 90f;

    private NavMeshAgent agent;
    private bool isInArrivalRoutine = false;
    private bool hasStarted = false;

    // Basisgeschwindigkeit für Geschwindigkeitsvariation
    private float baseSpeed;
    private Coroutine speedVariationCoroutine;

    private AudioSource footstepAudio;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        baseSpeed = agent.speed;
        speedVariationCoroutine = StartCoroutine(VarySpeedRoutine());

        StartCoroutine(InitialMove());

        footstepAudio = GetComponent<AudioSource>();

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

        if (agent.isStopped || agent.velocity.sqrMagnitude < 0.1f)
        {
            if (footstepAudio.isPlaying)
                footstepAudio.Pause();
        }
        else
        {
            if (!footstepAudio.isPlaying)
                footstepAudio.Play();
        }
    }




    private IEnumerator InitialMove()
    {
        PickAndGo();
        yield return RotateToDestination(finalRotationDuration);

        agent.isStopped = false;
        agent.updateRotation = true;
        hasStarted = true;
    }



    private IEnumerator ArrivalRoutine()
    {
        isInArrivalRoutine = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        // Zufällige Wartezeit vor Drehung
        float pauseBeforeTurn = Random.Range(pauseBeforeTurnMin, pauseBeforeTurnMax);
        yield return new WaitForSeconds(pauseBeforeTurn);

        // Zufälligen ersten Drehwinkel bestimmen (- für links)
        float firstTurnAngle = -Random.Range(firstTurnAngleMin, firstTurnAngleMax);
        yield return RotateOverTime(firstTurnAngle, rotationDuration * 0.5f);

        // Zweite Drehung so, dass Gesamtwinkel etwa +60° ±20°
        float secondTurnAngle = -firstTurnAngle + Random.Range(100f, 140f);
        yield return RotateOverTime(secondTurnAngle, rotationDuration);

        // Neues Ziel wählen
        PickAndGo();

        // Zielrichtung berechnen und hin drehen
        Vector3 dir = (agent.destination - transform.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            yield return RotateToRotation(targetRot, finalRotationDuration);
        }

        // Zufällige Wartezeit vor Bewegung zum neuen Ziel
        float pauseBeforeMoveToNewTarget = Random.Range(pauseBeforeMoveMin, pauseBeforeMoveMax);
        yield return new WaitForSeconds(pauseBeforeMoveToNewTarget);

        agent.isStopped = false;
        agent.updateRotation = true;
        isInArrivalRoutine = false;
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



    private IEnumerator RotateOverTime(float angle, float duration)
    {
        Quaternion start = transform.rotation;
        Quaternion end = start * Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.rotation = Quaternion.Slerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = end;
    }



    private IEnumerator RotateToRotation(Quaternion targetRot, float duration)
    {
        Quaternion start = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(start, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
    }



    private IEnumerator RotateToDestination(float duration)
    {
        Vector3 dir = (agent.destination - transform.position).normalized;
        if (dir.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        yield return RotateToRotation(targetRot, duration);
    }



    private IEnumerator VarySpeedRoutine()
    {
        while (true)
        {
            float targetSpeed = baseSpeed * Random.Range(0.7f, 1.3f);
            float duration = Random.Range(1f, 3f);

            yield return SmoothSpeedChange(targetSpeed, duration);

            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }



    private IEnumerator SmoothSpeedChange(float targetSpeed, float duration)
    {
        float startSpeed = agent.speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            agent.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.speed = targetSpeed;
    }


}
