using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class ShipEnemyController : MonoBehaviour
{
    // Animator Hash
    private static readonly int Death = Animator.StringToHash("Death");

    public enum ShipEnemyState
    {
        Patrol,
        Chase,
        Attack,
        AttackAlignment,
        Death,
        RotateAround,
    }
    
    // Variables y Referencias
    #region Serialized References

    [Header("General References")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private BoatDetectionSensor detectionSensor;
    [SerializeField] private CannonController cannonController;

    [Header("Animator / VFX / Collider")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private Collider shipCollider;

    #endregion

    #region State Control

    public ShipEnemyState currentState;

    #endregion

    #region Patrol State

    [Header("Patrol State")]
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float patrolSpeed = 5f;

    private int _currentWaypointIndex;

    #endregion

    #region Chase State

    [Header("Chase State")]
    [SerializeField] private float maxChaseDistance = 30f;
    [SerializeField] private float chaseStopDistance = 10f;
    [SerializeField] private float chaseSpeed = 5f;

    private Transform _currentTarget;
    private Vector3 _chaseForward;
    
    #endregion

    #region Attack State

    [Header("Attack State")]
    [SerializeField] private float lateralPosOffset = 5f;
    [SerializeField] private float forwardPosOffset = 10f;
    [SerializeField] private float attackSpeed = 7f;
    [SerializeField] private float attackStopDistance = 25f;

    private float fixedLateralPosOffset;
    private Vector3 attackDestination;

    #endregion

    #region Attack Alignment

    [Header("Attack Alignment State")]
    [SerializeField] private float distanceToAlign = 5f;
    [SerializeField] private float distanceToExit = 30f;
    [SerializeField] private float alignmentSpeed = 10f;

    [SerializeField] private bool rotationTurnLeft;
    [SerializeField] private float alignRotationDir;

    #endregion

    #region Rotate Around State

    [Header("Rotate Around State")]
    [SerializeField] private bool isAligned;
    [SerializeField] private Vector3 desiredRight;

    #endregion

    // Metodos de Unity
    #region Unity Methods

    private void Start()
    {
        wayPoints = EnemyWaypoints.Instance.waypoints;

        if (maxChaseDistance < detectionSensor.detectionRadius)
        {
            maxChaseDistance = detectionSensor.detectionRadius;
        }

        SetState(ShipEnemyState.Patrol);
    }

    private void Update()
    {
        if (currentState == ShipEnemyState.Death) return;

        switch (currentState)
        {
            case ShipEnemyState.Patrol:
                PatrolState();
                break;
            case ShipEnemyState.Chase:
                ChaseState();
                break;
            case ShipEnemyState.Attack:
                AttackState();
                break;
            case ShipEnemyState.AttackAlignment:
                AttackAlignmentState();
                break;
            case ShipEnemyState.RotateAround:
                RotateAroundState();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (wayPoints == null) return;

        foreach (Transform point in wayPoints)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(point.position, 2f);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, detectionSensor.detectionRadius);
    }

    #endregion

    // State Machine
    #region State Machine

    private void SetState(ShipEnemyState nextState)
    {
        if (currentState == nextState) return;

        currentState = nextState;
        debugText.text = currentState.ToString();

        switch (currentState)
        {
            case ShipEnemyState.Patrol:
                PatrolStateEnter();
                break;
            case ShipEnemyState.Chase:
                ChaseStateEnter();
                break;
            case ShipEnemyState.Attack:
                AttackStateEnter();
                break;
            case ShipEnemyState.AttackAlignment:
                AttackAlignmentStateEnter();
                break;
            case ShipEnemyState.RotateAround:
                RotateAroundStateEnter();
                break;
            case ShipEnemyState.Death:
                DeathStateEnter();
                break;
        }
    }

    #endregion

    // Funciones de Estado
    #region Patrol State

    private void PatrolStateEnter()
    {
        // Establece el índice del waypoint actual
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.autoBraking = true;
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.SetDestination(wayPoints[_currentWaypointIndex].position);
    }

    private void PatrolState()
    {
        // Detecta si hay un objetivo dentro del rango de detección
        if (detectionSensor.Detect(out _currentTarget))
        {
            SetState(ShipEnemyState.Chase);
            return;
        }

        // Si no hay objetivo, continúa patrullando
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % wayPoints.Length;
            navMeshAgent.SetDestination(wayPoints[_currentWaypointIndex].position);
        }
    }

    #endregion

    #region Chase State

    private void ChaseStateEnter()
    {
        // Inicia el chase state
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = chaseSpeed;
    }

    private void ChaseState()
    {
        // Detecta si hay un objetivo dentro del rango de detección
        if (!detectionSensor.Detect(out _currentTarget))
        {
            SetState(ShipEnemyState.Patrol);
            return;
        }

        float distance = Vector3.Distance(transform.position, _currentTarget.position);

        // Si el objetivo está fuera del rango de persecución, vuelve al estado de patrulla
        if (distance > maxChaseDistance)
        {
            SetState(ShipEnemyState.Patrol);
        }
        else
        {
            // Si el objetivo está dentro del rango de persecución, persigue al objetivo
            navMeshAgent.SetDestination(_currentTarget.position);
            
            // Si el objetivo está dentro de la distancia de parada de persecución, cambia al estado de ataque
            if (distance <= chaseStopDistance)
            {
                SetState(ShipEnemyState.Attack);
            }
        }
    }
    
    #endregion

    #region Attack State

    private void AttackStateEnter()
    {
        // Inicia el ataque
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.autoBraking = false;
        navMeshAgent.speed = attackSpeed;
        CheckClosestAttackSide();
    }

    private void AttackState()
    {
        // Si no hay un objetivo dentro del rango de detección, cambia al estado de persecución
        if (!detectionSensor.Detect(out _currentTarget))
        {
            SetState(ShipEnemyState.Chase);
            return;
        }
        
        attackDestination = GetAttackOffset(_currentTarget, fixedLateralPosOffset);
        navMeshAgent.SetDestination(attackDestination);

        // distTotarget es la distancia al objetivo
        float distToTarget = Vector3.Distance(transform.position, _currentTarget.position);
        // forwardDot es el producto punto entre la dirección hacia adelante del barco y la dirección hacia adelante del objetivo
        float forwardDot   = Vector3.Dot(transform.forward, _currentTarget.forward);
        
        // Si la distancia al objetivo es mayor que la distancia de parada del ataque o el producto punto es negativo, cambia al estado de persecución
        if (distToTarget > attackStopDistance || forwardDot < 0f)
        {
            SetState(ShipEnemyState.Chase);
        }
        // Si la distancia al objetivo es menor que la distancia de alineación, cambia al estado de alineación de ataque alineado
        else if (navMeshAgent.remainingDistance <= distanceToAlign)
        {
            SetState(ShipEnemyState.AttackAlignment);
        }
    }

    #endregion

    #region Attack Alignment State

    private void AttackAlignmentStateEnter()
    {
        navMeshAgent.enabled = false;
        alignRotationDir = GetRotationDirectionRight() ? 1 : -1;
    }

    private void AttackAlignmentState()
    {
        // Alinea el barco con el objetivo
        transform.Rotate(0, navMeshAgent.angularSpeed * alignRotationDir * Time.deltaTime, 0);

        // Comprueba si el barco está alineado con el objetivo
        if (Vector3.Dot(transform.forward, _currentTarget.forward) > 0.99f)
        {
            transform.forward = _currentTarget.forward;
            SetState(ShipEnemyState.RotateAround);
        }

        // Si la distancia al objetivo es mayor que la distancia de salida, cambia al estado de ataque
        if (Vector3.Distance(transform.position, _currentTarget.position) >= distanceToExit)
        {
            SetState(ShipEnemyState.Attack);
        }
    }

    #endregion

    #region Rotate Around State

    private void RotateAroundStateEnter()
    {
        navMeshAgent.enabled = false;
        rotationTurnLeft = !GetRotationDirectionRight();
        isAligned = false;
    }

    private void RotateAroundState()
    {
        // Mueve el barco hacia adelante en la dirección de su frente
        transform.position += transform.forward * (alignmentSpeed * Time.deltaTime);

        // Calcula la dirección deseada para el lado derecho del barco
        desiredRight = (_currentTarget.position - transform.position).normalized;
        if (rotationTurnLeft)
        {
            desiredRight = -desiredRight;
        }
        desiredRight.y = 0;

        // Comprueba si el barco está alineado con la dirección deseada
        if (!isAligned)
        {
            // Si no está alineado, rota el barco hacia la dirección deseada
            transform.Rotate(0, navMeshAgent.angularSpeed * Time.deltaTime, 0);
            isAligned = Mathf.Abs(Vector3.Dot(transform.right.normalized, desiredRight.normalized)) > 0.9f;
        }
        else
        {
            // Si está alineado, ajusta la dirección derecha del barco
            transform.right = desiredRight.normalized;
        }

        // Comprueba si el barco está lo suficientemente cerca del objetivo para salir del estado de rotación
        if (Vector3.Distance(transform.position, _currentTarget.position) >= distanceToExit)
        {
            SetState(ShipEnemyState.Attack);
        }
    }

    #endregion

    #region Death State
    
    public void TriggerLowHealth()
    {
        if (currentState == ShipEnemyState.Death) return;

        fireEffect.Play();
    }
    
    public void TriggerDeath()
    {
        if (currentState == ShipEnemyState.Death) return;

        SetState(ShipEnemyState.Death);
    }

    private void DeathStateEnter()
    {
        navMeshAgent.enabled = false;
        shipCollider.enabled = false;
        cannonController.enabled = false;
        StartCoroutine(DestroyShip());
    }

    private IEnumerator DestroyShip()
    {
        animator.SetTrigger(Death);
        destroyEffect.Play();
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    #endregion

    // Ayudas
    #region Helpers

    private bool GetRotationDirectionRight()
    {
        Vector3 dirToTarget = (_currentTarget.position - transform.position).normalized;
        return Vector3.Dot(transform.right, dirToTarget) > 0f;
    }

    private void CheckClosestAttackSide()
    {
        fixedLateralPosOffset = GetRotationDirectionRight() ? lateralPosOffset : -lateralPosOffset;
    }

    private Vector3 GetAttackOffset(Transform target, float offset)
    {
        Vector3 pos = target.position + offset * target.right;
        return pos + target.forward * forwardPosOffset;
    }

    #endregion
}
