using UnityEngine;
using UnityEngine.AI;   // F�r den NavMeshAgent

public class RandomNavWalker : MonoBehaviour
{
    [Header("Einstellungen f�r die Zielauswahl")]
    [Tooltip("Maximale Entfernung (Radius) um den NPC, in der neue Ziele gew�hlt werden.")]
    public float wanderRadius = 10f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0f; // Agent soll nicht vorher anhalten
        SetRandomDestination();
    }

    void Update()
    {
        // Neues Ziel setzen, sobald das aktuelle erreicht wurde
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetRandomDestination();
        }
    }

    /// <summary>
    /// W�hlt eine zuf�llige Position auf dem NavMesh innerhalb des vorgegebenen Radius
    /// und setzt sie als neues Ziel des Agents.
    /// </summary>
    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
