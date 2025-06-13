using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    public int health = 100; // Vida inicial objeto
    private int _currentHealth;
    [Header("Sprites")]
    public Transform fillSpriteRenderer;
    [SerializeField] private float _initialFillSpriteRendererSize; // 100 % vida
    
    public UnityEvent onDeath;
    public UnityEvent onHealthLow; // Evento para vida baja
    private bool _lowHealthEventInvoked = false; // Evita que el evento se invoque múltiples veces

    void Start()
    {
        // Guardar el tamaño inicial del sprite del llenado
        _initialFillSpriteRendererSize = fillSpriteRenderer.localScale.x;
        _currentHealth = health;
    }

    public void SetDamage(int damage)
    {
        if(_currentHealth == 0) return;
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        // Porcentaje de vida actual
        float percentage = (float)_currentHealth / health;
        // Actualizar el tamaño del sprite del llenado
        Vector3 newScale = fillSpriteRenderer.localScale;
        newScale.x = _initialFillSpriteRendererSize * percentage; // Formula para calcular el nuevo tamaño
        // Reemplazar el tamaño del sprite del llenado
        fillSpriteRenderer.localScale = newScale;

        // Si la vida es baja, invocar el evento
        if (percentage <= 0.4f && !_lowHealthEventInvoked)
        {
            _lowHealthEventInvoked = true; // Evita que se invoque varias veces
            onHealthLow.Invoke();
        }
        
        // Si la vida llega a 0, invocar el evento de muerte
        if (_currentHealth == 0)
        {
            onDeath.Invoke();
        }
    }
}
