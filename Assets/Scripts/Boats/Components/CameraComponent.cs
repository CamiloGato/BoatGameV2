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
