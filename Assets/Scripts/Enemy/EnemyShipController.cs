using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class ShipEnemyController : MonoBehaviour
{
    public enum ShipEnemyState
    {
        Patrol,
        Chase,
        Attack,
        AttackAlignment,
        Death,
        RotateArround,
    }

    [Header("Refs")]
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private TMP_Text debugText;
    
    [Header("Detection")]
    [SerializeField] private BoatDetectionSensor detectionSensor;

    [Header("STATES")]
    public ShipEnemyState currentState;


    #region Patrol Variables
    [Header("Patrol State")]
    public Transform[] _wayPoints;
    private int _currentWaypointIndex = 0;
    [SerializeField] private float patrolSpeed = 5;
    #endregion

    #region Chase Variables
    [Header("Chase State")]
    [SerializeField] private float maxChaseDistance;
    [SerializeField] private float chaseStopDistance;
    [SerializeField] private float chaseSpeed = 5;
    private Transform _currentTarget;
    private Vector3 _chaseForward;
    #endregion

    #region Attack Variables
    [Header("Attack State")]
    [SerializeField] private float lateralPosOffset;
    private float fixedLateralPosOffset;
    [SerializeField] private float forwardPosOffset;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackStopDistance;
    private Vector3 attackDestination;
    #endregion

    #region Attack Align Variables
    [Header("Attack Alignment State")]
    [SerializeField] private float distanceToAlign = 5f;
    [SerializeField] private float distanceToExit = 30f;
    [SerializeField] private float alignmentSpeed = 10f;
    private bool _rotationTurnLeft;
    private float _alignRotationDir;
    #endregion
    
    #region Rotate Around Variables
    [Header("Rotate Around State")]
    [SerializeField]
    private bool _isAligned = false;
    private Vector3 desiredRight;
    #endregion
    
    void OnDrawGizmos()
    {

        for (int i = 0; i < _wayPoints.Length; i++)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_wayPoints[i].position, 2f);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, detectionSensor.detectionRadius);
    }
    
    public void EnemyShipDeath()
    {
        SetState(ShipEnemyState.Death);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _wayPoints = EnemyWaypoints.Instance.waypoints;
        
        if (maxChaseDistance < detectionSensor.detectionRadius)
        {
            maxChaseDistance = detectionSensor.detectionRadius;
        }

        SetState(ShipEnemyState.Patrol);
    }

    // Update is called once per frame
    void Update()
    {
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
                AttackAlignmentStateV2();
                break;
            case ShipEnemyState.RotateArround:
                RotateArroundState();
                break;
        }
    }

    #region States
    private void SetState(ShipEnemyState nextState)
    {
        currentState = nextState;
        debugText.text = currentState.ToString();
        
        switch (currentState)
        {
            case ShipEnemyState.Patrol:
                PatrolStateEnter();
                break;
            case ShipEnemyState.Death:
                DeathStateEnter();
                break;
            case ShipEnemyState.Chase:
                ChaseStateEnter();
                break;
            case ShipEnemyState.Attack:
                AttackStateEnter();
                break;
            case ShipEnemyState.RotateArround:
                RotateArroundStateEnter();
                break;
            case ShipEnemyState.AttackAlignment:
                AttackAlignmentStateEnter();
                break;

        }
    }


    #region Patrol
    private void PatrolStateEnter()
    {
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(_wayPoints[_currentWaypointIndex].position);
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = patrolSpeed;
        _navMeshAgent.autoBraking = true;
    }

    private void PatrolState()
    {
        //Comprobamos si hay un target al que seguir
        if (detectionSensor.Detect(out _currentTarget))
        {
            SetState(ShipEnemyState.Chase);
            return;
        }

        //Cambio de checkpoint
        if (_navMeshAgent.remainingDistance < 0.1f)
        {
            //Comprobación de que el indice sea el ultimo, en ese caso volvemos a 0
            _currentWaypointIndex = _currentWaypointIndex == (_wayPoints.Length - 1) ? 0 : _currentWaypointIndex + 1;
            _navMeshAgent.SetDestination(_wayPoints[_currentWaypointIndex].position);
        }
    }

    #endregion
    #region Attack

    private void AttackStateEnter()
    {
        _navMeshAgent.enabled = true; 
        _chaseForward = transform.forward;      
        _navMeshAgent.isStopped = false;
        _navMeshAgent.autoBraking = false;
        _navMeshAgent.speed = attackSpeed;
        CheckClosestAttackSide();
    }

    private void AttackState()
    {
        attackDestination = GetAttackOffset(_currentTarget, fixedLateralPosOffset);

        _navMeshAgent.SetDestination(attackDestination);

        //Obtenemos la distancia entre el enemigo y el target
        float remainingDistance = Vector3.Distance(transform.position, _currentTarget.position);

        //Vector3.Distance(transform.position, attackDestination)
        //Si el barco llega al punto de ataque, inicia la alineación para comenzar a darle vueltas al target
        if (_navMeshAgent.remainingDistance <= distanceToAlign)
        {
            SetState(ShipEnemyState.RotateArround);
            //SetState(ShipEnemyState.AttackAlignment);
            return;
        }

        //Si la posición de ataque se encuentra por detras del barco, vuelve a perseguirlo de normal
        float scalar = Vector3.Dot(transform.forward, _currentTarget.forward);

        if (scalar <= 0f)
        {
            SetState(ShipEnemyState.Chase);            
        }

        //Si nos alejamos simplemente nos persigue
        if (remainingDistance >= attackStopDistance)
        {
            SetState(ShipEnemyState.Chase);
        }
    }

    #endregion
    #region Death

    private void DeathStateEnter()
    {        
        _navMeshAgent.isStopped = true;
        // Destruye el barco enemigo
        Destroy(gameObject);
    }

    #endregion
    #region Chase

    private void ChaseStateEnter()
    {
        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = chaseSpeed;
    }

    private void ChaseState()
    {
        float remainingDistance = Vector3.Distance(transform.position, _currentTarget.position);

        //Si la distancia que queda hasta el barco es mayor que la distancia para pasar a ataque que siga persiguiendo el punto
        if (remainingDistance >= chaseStopDistance)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_currentTarget.position);
        }
        else
        {
            SetState(ShipEnemyState.Attack);
        }


        if (remainingDistance >= maxChaseDistance)
        {
            SetState(ShipEnemyState.Patrol);
        }
    }

    #region AttackAlignment
    private void AttackAlignmentStateEnter()
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.enabled = false;

        

        if (GetRotationDirection())
        {
            _alignRotationDir = 1;
        }
        else
        {
            _alignRotationDir = -1;
        }

        Debug.DrawLine(transform.position, transform.position + _currentTarget.forward, Color.red, 4f);
    }

    private void AttackAlignmentStateV2()
    {
        SetState(ShipEnemyState.RotateArround);

        //Indicamos que siga al target
        //_navMeshAgent.SetDestination(_currentTarget.position);

        //Rotamos poco a poco
        //transform.forward = Vector3.MoveTowards(_alignWithTargetLastForward, transform.position + _currentTarget.forward, _alignWithTargetSpeed).normalized;

        transform.Rotate(new Vector3(0, _navMeshAgent.angularSpeed * _alignRotationDir, 0) * Time.deltaTime);

        //Si estan alineados que entre el rotate arround
        if (Vector3.Dot(transform.forward, _currentTarget.forward) > 0.99f)
        {
            transform.forward = _currentTarget.forward;
            SetState(ShipEnemyState.RotateArround);
        }

        //Si se aleja, vuelve al Attack
        if (Vector3.Distance(transform.position, _currentTarget.position) >= distanceToExit)
        {
            SetState(ShipEnemyState.Attack);
            return;
        }
    }

    #endregion AttackAlignment
    #region RotateArround
    private void RotateArroundStateEnter()
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.enabled = false;
        _rotationTurnLeft = !GetRotationDirection();
        _isAligned = false;
    }
    private void RotateArroundState()
    {
        //Movimiento hacia delante
        transform.position += transform.forward * (alignmentSpeed * Time.deltaTime);

        //Hacemos que se mueva para la derecha
        desiredRight = (_currentTarget.position - transform.position).normalized;

        //Si queremos que vaya para la izquierda lo invertimos
        if (_rotationTurnLeft) desiredRight = -desiredRight;
        
        //Ajustamos el eje "Y"
        Vector3 fixedRight = desiredRight;
        fixedRight.y = 0;

        //Asignamos el .right
        desiredRight = fixedRight;

        //Si no esta alineado lo alineamos poco a poco en el caso de que ya este alineado, lo ponemos a rotar de normal
        if (!_isAligned)
        {   
            transform.Rotate(new Vector3(transform.rotation.x, _navMeshAgent.angularSpeed /* * _alignRotationDir*/, transform.rotation.z) * Time.deltaTime);
            _isAligned = Vector3.Dot(transform.right.normalized, desiredRight.normalized) > 0.9f || Vector3.Dot(transform.right.normalized, desiredRight.normalized) < -0.9f;
            //Debug.Log(Vector3.Dot(transform.right.normalized, desiredRight));
        }
        else
        {
            transform.right = desiredRight.normalized;
        }

        //Si esta muy lejos cambia a attack
        if (Vector3.Distance(transform.position, _currentTarget.position) >= distanceToExit)
        {
            SetState(ShipEnemyState.Attack);
            return;
        }
    }
    #endregion
    #endregion

    #region OtherMethods
    
    #region AttackMethods
    /// <summary>
    /// Obtener la posicion de ataque
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private Vector3 GetAttackOffset(Transform target, float offset)
    {
        //Sumamos offset lateral
        Vector3 offsetPos = target.position + offset * target.right;
        //Sumamos el offset frontal
        offsetPos += _currentTarget.forward * forwardPosOffset;

        return offsetPos;
    }

    /// <summary>
    /// Método que incica la posición de ataque mas cercana
    /// </summary>
    /// <returns></returns>
    private void CheckClosestAttackSide()
    {
        float distance1 = Vector3.Distance(transform.position, GetAttackOffset(_currentTarget, lateralPosOffset));
        float distance2 = Vector3.Distance(transform.position, GetAttackOffset(_currentTarget, -lateralPosOffset));

        fixedLateralPosOffset = distance1 > distance2 ? -lateralPosOffset : lateralPosOffset;
    }

    /// <summary>
    /// Método que devuelve true cuando tiene que girar a la derecha y false si tiene que girar a la izquierda
    /// </summary>
    /// <returns></returns>
    private bool GetRotationDirection()
    {
        float forwardDot = Vector3.Dot(_currentTarget.forward, _chaseForward.normalized);
        float rightDot = Vector3.Dot(_currentTarget.right, (_currentTarget.position - transform.position).normalized);

        //Ambos positivos izq
        //Ambos negativos izq
        //Fwd positivo, right negativo derecha
        //Fwd negativo, right positivo derecha

        if (forwardDot > 0 && rightDot > 0 || forwardDot < 0 && rightDot < 0)
        {
            return true;
        }
        else if (forwardDot > 0 && rightDot < 0 || forwardDot < 0 && rightDot > 0)
        {
            return false;
        }

        return true;
    }
    
    #endregion
    #endregion
    #endregion
}
