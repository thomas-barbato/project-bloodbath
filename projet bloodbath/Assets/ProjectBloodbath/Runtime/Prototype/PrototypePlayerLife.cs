using System.Collections;
using ProjectBloodbath.Combat;
using ProjectBloodbath.Player;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health), typeof(CharacterController))]
    public sealed class PrototypePlayerLife : MonoBehaviour
    {
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField, Min(0.1f)] private float respawnDelay = 0.8f;

        private Health health;
        private CharacterController characterController;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private float damageFlashUntil;
        private Coroutine respawnRoutine;

        public void Configure(FpsPlayerController controller)
        {
            playerController = controller;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            characterController = GetComponent<CharacterController>();
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

        private void OnGUI()
        {
            if (health == null)
            {
                return;
            }

            float ratio = health.Maximum > 0f ? health.Current / health.Maximum : 0f;
            Rect background = new(28f, Screen.height - 42f, 230f, 14f);
            Color previousColor = GUI.color;

            GUI.color = new Color(0.02f, 0.025f, 0.025f, 0.9f);
            GUI.DrawTexture(background, Texture2D.whiteTexture);

            GUI.color = Color.Lerp(
                new Color(0.75f, 0.04f, 0.02f, 0.95f),
                new Color(0.2f, 0.85f, 0.48f, 0.95f),
                ratio);
            GUI.DrawTexture(
                new Rect(background.x + 2f, background.y + 2f, (background.width - 4f) * ratio, 10f),
                Texture2D.whiteTexture);

            if (Time.time < damageFlashUntil)
            {
                GUI.color = new Color(0.85f, 0.02f, 0.01f, 0.25f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 22f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(0f, Screen.height - 22f, Screen.width, 22f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(0f, 0f, 22f, Screen.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(Screen.width - 22f, 0f, 22f, Screen.height), Texture2D.whiteTexture);
            }

            GUI.color = previousColor;
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
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            characterController.enabled = false;
            yield return new WaitForSeconds(respawnDelay);

            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            characterController.enabled = true;
            health.RestoreFull();
            damageFlashUntil = 0f;

            if (playerController != null)
            {
                playerController.enabled = true;
            }

            respawnRoutine = null;
        }
    }
}
