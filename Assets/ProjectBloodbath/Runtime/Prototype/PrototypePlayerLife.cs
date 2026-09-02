using System.Collections;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Input;
using ProjectBloodbath.Player;
using ProjectBloodbath.Progression;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(CharacterController))]
    [RequireComponent(typeof(AbilityResource))]
    public sealed class PrototypePlayerLife : MonoBehaviour,
        ICombatTarget,
        IDamageOutputProvider
    {
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField, Min(0.1f)] private float respawnDelay = 0.8f;
        [SerializeField, Range(0f, 1f)]
        private float resurrectionDamageMultiplier = 0.5f;
        [SerializeField, Min(0f)] private float resurrectionPenaltyDuration = 5f;
        [SerializeField] private Transform bodyVisual;
        [SerializeField] private Color soulColor =
            new(0.42f, 0.72f, 0.82f, 0.72f);

        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");

        private Health health;
        private AbilityResource abilityResource;
        private CharacterController characterController;
        private PlayerInputReader inputReader;
        private PrototypeWeaponLoadout weaponLoadout;
        private CharacterEquipment equipment;
        private Renderer[] bodyRenderers;
        private MaterialPropertyBlock soulPropertyBlock;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private float damageFlashUntil;
        private float resurrectionPenaltyUntil;
        private Coroutine respawnRoutine;
        private PrototypeCorpseRecovery activeCorpse;

        public bool IsRespawning => respawnRoutine != null;
        public bool IsSoul { get; private set; }
        public bool CanBeTargeted => !IsSoul && health != null && health.IsAlive;
        public float RespawnDelay => respawnDelay;
        public float OutgoingDamageMultiplier
        {
            get
            {
                float resurrectionMultiplier =
                    Time.time < resurrectionPenaltyUntil
                        ? resurrectionDamageMultiplier
                        : 1f;
                float equipmentMultiplier =
                    equipment?.OutgoingDamageMultiplier ?? 1f;
                return resurrectionMultiplier * equipmentMultiplier;
            }
        }
        public float ResurrectionPenaltyRemaining => Mathf.Max(
            0f,
            resurrectionPenaltyUntil - Time.time);
        public float DamageFlashRemaining => Mathf.Max(
            0f,
            damageFlashUntil - Time.time);
        public PrototypeCorpseRecovery ActiveCorpse => activeCorpse;

        public void Configure(FpsPlayerController controller)
        {
            playerController = controller;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            abilityResource = GetComponent<AbilityResource>();
            characterController = GetComponent<CharacterController>();
            inputReader = GetComponent<PlayerInputReader>();
            weaponLoadout = GetComponent<PrototypeWeaponLoadout>();
            equipment = GetComponent<CharacterEquipment>();
            if (playerController == null)
            {
                playerController = GetComponent<FpsPlayerController>();
            }

            if (bodyVisual == null)
            {
                bodyVisual = transform.Find("PrototypeBody");
            }

            bodyRenderers = bodyVisual != null
                ? bodyVisual.GetComponentsInChildren<Renderer>(true)
                : new Renderer[0];
            soulPropertyBlock = new MaterialPropertyBlock();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
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

        public bool TryRecoverBody(PrototypeCorpseRecovery corpse)
        {
            if (
                !IsSoul ||
                corpse == null ||
                !ReferenceEquals(corpse, activeCorpse))
            {
                return false;
            }

            IsSoul = false;
            health.SetInvulnerable(false);
            health.RestoreFull();
            abilityResource?.RestoreFull();
            weaponLoadout?.SetCombatEnabled(true);
            SetSoulVisuals(false);
            resurrectionPenaltyUntil =
                Time.time + resurrectionPenaltyDuration;

            activeCorpse = null;
            Destroy(corpse.gameObject);
            return true;
        }

        private void OnDamaged(DamageInfo damage)
        {
            damageFlashUntil = Time.time + 0.14f;
        }

        private void OnDied(DamageInfo damage)
        {
            if (respawnRoutine == null)
            {
                respawnRoutine = StartCoroutine(Respawn());
            }
        }

        private IEnumerator Respawn()
        {
            CreateCorpse();
            health.SetInvulnerable(true);
            if (inputReader != null)
            {
                inputReader.enabled = false;
            }

            weaponLoadout?.SetCombatEnabled(false);
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            characterController.enabled = false;
            yield return new WaitForSeconds(respawnDelay);

            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            characterController.enabled = true;
            playerController?.ResetForRespawn();
            health.RestoreFull();
            damageFlashUntil = 0f;
            IsSoul = true;
            SetSoulVisuals(true);

            if (inputReader != null)
            {
                inputReader.enabled = true;
            }

            if (playerController != null)
            {
                playerController.enabled = true;
            }

            respawnRoutine = null;
        }

        private void CreateCorpse()
        {
            if (activeCorpse != null)
            {
                Destroy(activeCorpse.gameObject);
            }

            GameObject corpseObject = new("PlayerCorpse");
            corpseObject.transform.SetPositionAndRotation(
                transform.position,
                transform.rotation);

            SphereCollider recoveryArea = corpseObject.AddComponent<
                SphereCollider>();
            recoveryArea.isTrigger = true;
            recoveryArea.center = Vector3.up * 0.55f;
            recoveryArea.radius = 1.15f;

            Rigidbody corpseBody = corpseObject.AddComponent<Rigidbody>();
            corpseBody.isKinematic = true;
            corpseBody.useGravity = false;

            activeCorpse = corpseObject.AddComponent<
                PrototypeCorpseRecovery>();
            activeCorpse.Initialize(this);

            if (bodyVisual == null)
            {
                return;
            }

            GameObject corpseVisual = Instantiate(
                bodyVisual.gameObject,
                corpseObject.transform);
            corpseVisual.name = "Body";
            corpseVisual.transform.localPosition = Vector3.up * 0.4f;
            corpseVisual.transform.localRotation =
                Quaternion.Euler(0f, 0f, 90f);

            foreach (Collider bodyCollider in
                corpseVisual.GetComponentsInChildren<Collider>(true))
            {
                Destroy(bodyCollider);
            }
        }

        private void SetSoulVisuals(bool soulVisible)
        {
            foreach (Renderer bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer == null)
                {
                    continue;
                }

                if (!soulVisible)
                {
                    bodyRenderer.SetPropertyBlock(null);
                    continue;
                }

                bodyRenderer.GetPropertyBlock(soulPropertyBlock);
                soulPropertyBlock.SetColor(BaseColorId, soulColor);
                bodyRenderer.SetPropertyBlock(soulPropertyBlock);
            }
        }
    }
}
