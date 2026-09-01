using System.Collections;
using ProjectBloodbath.Combat;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(Renderer), typeof(Rigidbody))]
    public sealed class PrototypeCombatTarget : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float resetDelay = 2.5f;
        [SerializeField] private Color hitColor = new(1f, 0.9f, 0.65f, 1f);
        [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Health health;
        private Renderer targetRenderer;
        private Rigidbody targetRigidbody;
        private MaterialPropertyBlock propertyBlock;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Color baseColor;
        private float flashUntil;

        private void Awake()
        {
            health = GetComponent<Health>();
            targetRenderer = GetComponent<Renderer>();
            targetRigidbody = GetComponent<Rigidbody>();
            propertyBlock = new MaterialPropertyBlock();
            startPosition = transform.position;
            startRotation = transform.rotation;
            baseColor = targetRenderer.sharedMaterial != null &&
                targetRenderer.sharedMaterial.HasProperty(BaseColorId)
                ? targetRenderer.sharedMaterial.GetColor(BaseColorId)
                : Color.gray;
            targetRigidbody.isKinematic = true;
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        private void Update()
        {
            SetColor(Time.time < flashUntil ? hitColor : baseColor);
        }

        private void OnDamaged(DamageInfo damage)
        {
            flashUntil = Time.time + flashDuration;
        }

        private void OnDied(DamageInfo damage)
        {
            targetRigidbody.isKinematic = false;
            targetRigidbody.AddForceAtPosition(
                damage.Direction * damage.Force + Vector3.up * damage.Force * 0.35f,
                damage.Point,
                ForceMode.Impulse);
            StartCoroutine(ResetTarget());
        }

        private IEnumerator ResetTarget()
        {
            yield return new WaitForSeconds(resetDelay);
            targetRigidbody.linearVelocity = Vector3.zero;
            targetRigidbody.angularVelocity = Vector3.zero;
            targetRigidbody.isKinematic = true;
            transform.SetPositionAndRotation(startPosition, startRotation);
            health.RestoreFull();
            flashUntil = 0f;
            SetColor(baseColor);
        }

        private void SetColor(Color color)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
