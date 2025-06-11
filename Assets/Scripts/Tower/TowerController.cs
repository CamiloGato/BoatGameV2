
using UnityEngine;

namespace Tower
{
    public class TowerController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Collider towerCollider;
        [SerializeField] private DamageReceiver damageReceiver;

        public void TowerHasBeenDestroyed()
        {
            towerCollider.enabled = false;
            damageReceiver.enabled = false;

            Debug.Log("The tower has been destroyed");
        }
    }
}