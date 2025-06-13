
using System.Collections;
using UnityEngine;

namespace Tower
{
    public class TowerController : MonoBehaviour
    {
        private static readonly int Death = Animator.StringToHash("Death");
        
        [SerializeField] private Animator animator;
        [SerializeField] private Collider towerCollider;
        [SerializeField] private CannonController cannonController;
        [SerializeField] private ParticleSystem destroyEffect;
        [SerializeField] private ParticleSystem fireEffect;

        public void TriggerLowHealth()
        {
            fireEffect.Play();
        }
        
        public void TriggerDestroy()
        {
            // Desactiva el controlador de la torre y el collider
            cannonController.enabled = false;
            towerCollider.enabled = false;
            // Inicia la animación de destrucción
            StartCoroutine(DestroyAnimation());
        }
        
        // Inicia la animación de destrucción y espera a que termine antes de destruir el objeto
        private IEnumerator DestroyAnimation()
        {
            animator.SetTrigger(Death);
            destroyEffect.Play();
            yield return new WaitForSeconds(4f); // Espera a que la animación de destrucción termine
            Destroy(gameObject);
        }
        
    }
}