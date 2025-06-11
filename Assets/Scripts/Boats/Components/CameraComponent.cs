using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;
    

    [Header("Follow")]
    [SerializeField] private Transform target;

    [Header("Offsets")]
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private Vector3 lookOffset;


    [Header("Limits")]
    public LayerMask projectionPlaneLayer;
    [SerializeField] private Vector3 minWorldLimit;
    [SerializeField] private Vector3 maxWorldLimit;


    void Update()
    {
        FollowTarget();
    }

    void OnValidate()
    {
        FollowTarget();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 size = maxWorldLimit - minWorldLimit;
        Vector3 center = minWorldLimit + size / 2;
        Gizmos.DrawCube(center, size);

        //Dibujado de proyeccion de la camara en el plano del jugador
        Gizmos.color = Color.magenta;
    }


    private void FollowTarget()
    {
        if (target == null) return;


        Vector3 nextPosition = target.position + followOffset;
        nextPosition.x = Mathf.Clamp(nextPosition.x, minWorldLimit.x, maxWorldLimit.x);
        nextPosition.z = Mathf.Clamp(nextPosition.z, minWorldLimit.z, maxWorldLimit.z);
        transform.position = nextPosition;
       

        transform.forward = (target.position + lookOffset) - transform.position;
    }

}
