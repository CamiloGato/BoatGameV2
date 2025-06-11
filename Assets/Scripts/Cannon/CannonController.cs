
using UnityEngine;



    public class CannonController : MonoBehaviour
    {
        public GameObject ownerGo;
        [Header("Detection Area")]
        public LayerMask targetLayers;
        public float detectionRadius = 3f;
        public float detectionAngle = 45f;
        public Transform detectionCenterPoint;

        [Header("Shoot")]
        [SerializeField]
        private CannonBallController _cannonBallPrefab;
        [SerializeField]
        private Transform _shootingPoint;

        [SerializeField]
        private float _shootVerticalOffset;

        [SerializeField] private float _shootTimeMin = 1f;
        [SerializeField] private float _shootTimeMax = 3f;
        [SerializeField] private float _shootTimestamp;

        [Header("Follow")]
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] public bool rotate360;
        [SerializeField] private Transform _body;

        [Header("Debug")]
        [SerializeField] private bool _executeDetectionOnEdtiror;
        [SerializeField] private Color _gizmoDefaultColor = Color.red;
        [SerializeField] private Color _gizmoDetectionColor = Color.green;
        private Transform _target;

        private void OnDrawGizmos()
        {
            // Comprobamos si hay asignado un detectionPoint para evitar errores en el editor
            if (detectionCenterPoint == null)
            {
                detectionCenterPoint = transform;  // Si no lo hay asignamos el transform del propio cañón
            }

            if (_executeDetectionOnEdtiror)
            {
                if (!CheckIfTargetInRange()) _target = null;
                if (_target == null) Detect();
            }

            // Dibujamos el área de detección
            Gizmos.color = (_executeDetectionOnEdtiror && _target != null) ? _gizmoDetectionColor : _gizmoDefaultColor;
            Gizmos.DrawWireSphere(detectionCenterPoint.position, detectionRadius);
            // Dibujamos los vectores que delimitan el área de detección según detectionAngle
            Quaternion leftRotation = Quaternion.Euler(0f, -detectionAngle, 0f);
            Quaternion rightRotation = Quaternion.Euler(0f, detectionAngle, 0f);
            Gizmos.DrawRay(detectionCenterPoint.position, leftRotation * detectionCenterPoint.forward * detectionRadius);
            Gizmos.DrawRay(detectionCenterPoint.position, rightRotation * detectionCenterPoint.forward * detectionRadius);

            if (_target != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(detectionCenterPoint.position, _target.position);
            }
        }

        void Start()
        {
            // Asegúrate de que los objetos estén correctamente asignados
            if (_cannonBallPrefab == null || _shootingPoint == null)
            {
                Debug.LogError("Cannon ball prefab or shooting point is not assigned in the Inspector!", this);
            }
        }

        void Update()
        {
            if (!CheckIfTargetInRange()) _target = null;
            if (_target == null) Detect();
            if (_target == null) return;

            FollowTarget();

            if (_shootTimestamp <= Time.time)
            {
                Shoot();
                _shootTimestamp = Random.Range(_shootTimeMin, _shootTimeMax) + Time.time;
            }
        }

        [ContextMenu("Detect")]
        private void Detect()
        {
            Collider[] targetsDetected = Physics.OverlapSphere(detectionCenterPoint.position, detectionRadius, targetLayers);

            if (targetsDetected == null || targetsDetected.Length == 0) return;

            int firstInRange = -1;
            for (int i = 0; i < targetsDetected.Length; i++)
            {
                if (CheckIfPointInRange(targetsDetected[i].transform.position))
                {
                    firstInRange = i;
                    break;
                }
            }

            if (firstInRange < 0) return;

            float minDistance = Vector3.Distance(detectionCenterPoint.position, targetsDetected[firstInRange].transform.position);
            _target = targetsDetected[firstInRange].transform;

            for (int i = firstInRange + 1; i < targetsDetected.Length; i++)
            {
                if (CheckIfPointInRange(targetsDetected[i].transform.position)) continue;

                float distance = Vector3.Distance(detectionCenterPoint.position, targetsDetected[0].transform.position);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    _target = targetsDetected[i].transform;
                }
            }
        }

        private bool CheckIfPointInRange(Vector3 point)
        {
            Vector3 dir = point - detectionCenterPoint.position;
            dir.y = 0;
            dir.Normalize();

            float scalar = Vector3.Dot(detectionCenterPoint.forward, dir);
            float angle = (1 - scalar) * 90f;

            return detectionAngle >= angle;
        }

        private bool CheckIfTargetInRange()
        {
            if (_target == null || !CheckIfPointInRange(_target.position)) return false;

            float distance = Vector3.Distance(detectionCenterPoint.position, _target.position);
            return distance < detectionRadius;
        }

        /// <summary>
        /// Ejecuta el disparo hacia el frente del cañón
        /// </summary>
        private void Shoot()
        {
            CannonBallController cannonBall = Instantiate(_cannonBallPrefab, _shootingPoint.position, Quaternion.identity);

            if(cannonBall.TryGetComponent(out CannonBallController damageDealer))
            {
                damageDealer.ownerGo = ownerGo;
            }

            // Aplicar la fuerza al proyectil
            Vector3 targetPoint = _target.position;
            targetPoint.y += _shootVerticalOffset;
            cannonBall.StartMovement(30f, targetPoint);
        }

        /// <summary>
        /// Método que orienta el cañón hacia su objetivo
        /// </summary>
        private void FollowTarget()
        {
            if (_target == null) return;

            Vector3 direction = _target.position - transform.position;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);

            if (rotate360)
            {
                if (_body)
                {
                    _body.forward = transform.forward;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
            else if (_body)
            {
                _body.rotation = Quaternion.RotateTowards(_body.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

