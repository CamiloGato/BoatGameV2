using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BoatPlayer : MonoBehaviour
{
    private static readonly int Death = Animator.StringToHash("Death");

    public int player = 1;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider boatCollider;
    [SerializeField] private CannonController cannonController;

    [Header("Movement")]
    public float maxSpeed = 20f;
    public float acceleration = 5f;
    public float driftFactor = 0.97f;

    [Header("Steering")]
    public float steeringSpeed = 15f;
    public float rotationSpeed = 50f;
    public float minRotationSpeed = 5f;
    public float minSpeedFactor = 0.05f;

    private float _steeringInput;
    private float _steeringValue;
    private float _currentSpeed;
    private Vector3 _velocity;
    private bool _accelerating;
    private bool _deacceleration;
    private bool _isDead;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private ParticleSystem fireEffect;

    public UnityEvent onDeath;
    
    void Update()
    {
        if(_isDead) return;

        Inputs();

        Steering(_steeringInput);

        if(_accelerating)
        {
            Accelerate();
        }

        if(_deacceleration)
        {
            AddDeacceleration();
        }

        FixVelocity();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = _velocity;
    }

    private void Inputs()
    {
        _steeringInput = Input.GetAxisRaw("Horizontal_" + player);

        if (Input.GetButtonDown("Accelerate_" + player))
        {
            _accelerating = true;
        }
        else if (Input.GetButtonUp("Accelerate_" + player))
        {
            _accelerating = false;
        }

        if (Input.GetButtonDown("Brake_" + player))
        {
            _deacceleration = true;
        }
        else if (Input.GetButtonUp("Brake_" + player))
        {
            _deacceleration = false;
        }
        
    }

    private void Accelerate()
    {
        _currentSpeed += acceleration * Time.deltaTime;
    }

    private void AddDeacceleration()
    {
        _currentSpeed -= acceleration * Time.deltaTime;
    }

    private void FixVelocity()
    {
        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, maxSpeed);
        _velocity = transform.forward * _currentSpeed;

        // Aplica el drift factor
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(_velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(_velocity, transform.right);
        Vector3 fixedVelocity = forwardVelocity + rightVelocity * driftFactor;
        _velocity = fixedVelocity;
    }

    private void Steering(float dir)
    {
        float steeringFactor = rb.linearVelocity.magnitude * minSpeedFactor;
        steeringFactor = Mathf.Clamp01(steeringFactor);


        _steeringValue = Mathf.MoveTowards(_steeringValue, dir * steeringFactor, steeringSpeed * Time.deltaTime);
        float finalRotationSpeed = rotationSpeed * steeringFactor;
        finalRotationSpeed = Mathf.Clamp(finalRotationSpeed, minRotationSpeed, rotationSpeed);
        transform.Rotate(0f, finalRotationSpeed * dir * Time.deltaTime, 0f);
    }

    public void TriggerDeath()
    {
        cannonController.enabled = false;
        boatCollider.enabled = false;
        _currentSpeed = 0f;
        _velocity = Vector3.zero;
        _isDead = true;
        animator.SetTrigger(Death);
        StartCoroutine(DieAnimation());
    }

    public void TriggerLowHealth()
    {
        fireEffect.Play();
    }
    
    private IEnumerator DieAnimation()
    {
        animator.SetTrigger(Death);
        explosionEffect.Play();
        yield return new WaitForSeconds(3f);
        onDeath.Invoke();
    }
}