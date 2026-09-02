using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCorpseRecovery : MonoBehaviour
    {
        private PrototypePlayerLife owner;

        public PrototypePlayerLife Owner => owner;

        public void Initialize(PrototypePlayerLife playerLife)
        {
            owner = playerLife;
        }

        private void OnTriggerEnter(Collider other)
        {
            PrototypePlayerLife playerLife =
                other.GetComponentInParent<PrototypePlayerLife>();
            if (playerLife != null && ReferenceEquals(playerLife, owner))
            {
                owner.TryRecoverBody(this);
            }
        }
    }
}
