using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    public int damage = 10;
    public GameObject ownerGo;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (!_rigidbody)
        {
            Debug.LogError("CannonBallController requires a Rigidbody component.", this);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == ownerGo) return;
    
        if(other.TryGetComponent(out DamageReceiver damageReceiver))
        {
            damageReceiver.SetDamage(damage);
        }
        
        Destroy(gameObject);
    }
    
    public void StartMovement(float speed, Vector3 targetPoint)
    {
        // Calcula la dirección hacia el objetivo
        Vector3 direction = (targetPoint - transform.position).normalized;
        // Aplica la fuerza al Rigidbody del proyectil teniendo en cuenta que no usa gravedad
        _rigidbody.linearVelocity = direction * speed;
    }
}
