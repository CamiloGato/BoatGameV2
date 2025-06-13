using System.Collections;
using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    public int damage = 10;
    public GameObject ownerGo;
    public float lifetime = 5f;
    public ParticleSystem explosionEffect;
    public MeshRenderer meshRenderer;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (!_rigidbody)
        {
            Debug.LogError("CannonBallController requires a Rigidbody component.", this);
        }
    }

    private void Start()
    {
        // Ejecuta la destrucción del proyectil después de un tiempo determinado
        Invoke(nameof(Destroy), lifetime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == ownerGo) return;
    
        if(other.TryGetComponent(out DamageReceiver damageReceiver))
        {
            damageReceiver.SetDamage(damage);
        }
        
        Destroy();
    }
    
    public void StartMovement(float speed, Vector3 targetPoint)
    {
        // Calcula la dirección hacia el objetivo
        Vector3 direction = (targetPoint - transform.position).normalized;
        // Aplica la fuerza al Rigidbody del proyectil teniendo en cuenta que no usa gravedad
        _rigidbody.linearVelocity = direction * speed;
    }

    private void Destroy()
    {
        StartCoroutine(DestroyBullet());
    }
    
    private IEnumerator DestroyBullet()
    {
        _rigidbody.isKinematic = true; // Desactiva la física del proyectil
        meshRenderer.enabled = false; // Desactiva el renderizado del proyectil
        explosionEffect.Play();
        yield return new WaitForSeconds(explosionEffect.main.duration);
        Destroy(gameObject);
    }
}
