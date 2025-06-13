using UnityEngine;

public class BoatDetectionSensor : MonoBehaviour
{   [Header("Detection Settings")]
    [SerializeField]
    private Transform detectionCenterPoint;
    private float startRadius;
    public float detectionRadius;
    [SerializeField]
    private LayerMask targetLayers;
    [SerializeField]
    private Transform _target;

    [Header("Debug")]
    [SerializeField]
    private bool _executeDetectionOnEditor;
    [SerializeField]
    private Color _gizmoDefaultAreaColor = Color.red;
    [SerializeField]
    private Color _gizmoDetectionColor = Color.green;

    private void OnDrawGizmos() {
        // Si no hay asignado un detectionCenter se lo asignamos para evitar errores
        if (detectionCenterPoint == null)
        {
            detectionCenterPoint = transform;
        }

        if (_executeDetectionOnEditor)
        {
            //Se comprueba constantemente en el editor (NO INGAME)
            if(!CheckIfTargetInRange()) _target = null;
            if(_target == null) Detect(out Transform targettest);
            
            //Área de detección
            Gizmos.color = (_executeDetectionOnEditor && _target != null) ? _gizmoDetectionColor : _gizmoDefaultAreaColor;
            Gizmos.DrawWireSphere(detectionCenterPoint.position, detectionRadius);
        } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startRadius = detectionRadius;
    }
    
    [ContextMenu("Detect")]
    public bool Detect(out Transform target)
    {
        Collider[] targetsDetected = Physics.OverlapSphere(detectionCenterPoint.position, detectionRadius, targetLayers);

        if (targetsDetected == null|| targetsDetected.Length == 0)
        {
            target = null;
            return false;
        } 

        //Detección del barco mas cercano
        //Indicamos que la última distancia es la misma que la del primer barco que se detecta
        float lastDistance = Vector3.Distance(detectionCenterPoint.position, targetsDetected[0].transform.position);
        //Si el target esta en el angulo..                       Lo asignamos
        _target = targetsDetected[0].transform;

        //Si hay mas de un barco recorre el for
        for (int i = 0 ; i < targetsDetected.Length; i++)
        {
            //Distancia con el barco que comprobamos
            float currentDistance = Vector3.Distance(detectionCenterPoint.position, targetsDetected[i].transform.position);

            //Si es menor que la del último barco comprobado...
            if (currentDistance < lastDistance)
            {   
                //Se selecciona ese barco como target
                _target = targetsDetected[i].transform;

                //Indicamos que la distancia de este barco es la última comprobada para la siguiente comprobación
                lastDistance = currentDistance;
            }
        }

        target = _target;

        return true;
    }

    private bool CheckIfTargetInRange()
    {
        if (_target == null) return false;

        float distance = Vector3.Distance(detectionCenterPoint.position, _target.position);

        return distance < detectionRadius - 1f;
    }

    public void RestartRadius()
    {
        detectionRadius = startRadius;
    }

}

