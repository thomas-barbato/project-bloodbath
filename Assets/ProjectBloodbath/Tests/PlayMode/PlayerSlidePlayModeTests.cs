using System.Collections;
using NUnit.Framework;
using ProjectBloodbath.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectBloodbath.Tests.PlayMode
{
    public sealed class PlayerSlidePlayModeTests
    {
        private const string ScenePath =
            "Assets/Scenes/Prototype/MovementLab.unity";

        private Keyboard keyboard;
        private FpsPlayerController playerController;
        private CharacterController characterController;
        private FirstPersonBodyPresentation bodyPresentation;
        private Transform playerTransform;
        private Transform cameraPivot;
        private float standingHeight;
        private float standingCameraHeight;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                ScenePath,
                LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null);
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            GameObject.Find("PrototypeEnemy")?.SetActive(false);
            GameObject.Find("PrototypeSkirmisher")?.SetActive(false);

            GameObject player = GameObject.Find("Player");
            Assert.That(player, Is.Not.Null);
            playerController = player.GetComponent<FpsPlayerController>();
            playerTransform = player.transform;
            characterController = player.GetComponent<CharacterController>();
            bodyPresentation = player.GetComponentInChildren<
                FirstPersonBodyPresentation>();
            cameraPivot = player.transform.Find("CameraPivot");

            Assert.That(playerController, Is.Not.Null);
            Assert.That(characterController, Is.Not.Null);
            Assert.That(bodyPresentation, Is.Not.Null);
            Assert.That(cameraPivot, Is.Not.Null);

            standingHeight = characterController.height;
            standingCameraHeight = cameraPivot.localPosition.y;
            keyboard = InputSystem.AddDevice<Keyboard>();

            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SlidePressedBeforeLandingStartsOnTouchdown()
        {
            SetKeys(Key.W, Key.LeftShift);
            yield return new WaitForSeconds(0.3f);

            SetKeys(Key.W, Key.LeftShift, Key.Space);
            yield return null;
            SetKeys(Key.W, Key.LeftShift);

            float airborneTimeout = Time.time + 1f;
            while (characterController.isGrounded && Time.time < airborneTimeout)
            {
                yield return null;
            }

            Assert.That(characterController.isGrounded, Is.False);

            float landingApproachTimeout = Time.time + 2f;
            while (
                (playerController.Velocity.y >= 0f ||
                 playerTransform.position.y > 0.32f) &&
                Time.time < landingApproachTimeout)
            {
                yield return null;
            }

            Assert.That(playerController.Velocity.y, Is.LessThan(0f));
            SetKeys(Key.W, Key.LeftShift, Key.LeftCtrl);
            yield return null;
            SetKeys(Key.W, Key.LeftShift);

            float slideTimeout = Time.time + 0.4f;
            while (!playerController.IsSliding && Time.time < slideTimeout)
            {
                yield return null;
            }

            Assert.That(
                playerController.IsSliding,
                Is.True,
                "Une glissade demandée juste avant l'atterrissage doit être conservée.");
            Assert.That(characterController.isGrounded, Is.True);

            SetKeys();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (keyboard != null && keyboard.added)
            {
                InputSystem.RemoveDevice(keyboard);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MovingFastThenPressingSlideStartsVisibleSlide()
        {
            SetKeys(Key.W, Key.LeftShift);
            float sprintTimeout = Time.time + 0.75f;
            while (
                playerController.Velocity.magnitude <= 6.8f &&
                Time.time < sprintTimeout)
            {
                yield return null;
            }

            Assert.That(
                characterController.isGrounded,
                Is.True,
                "Le joueur doit être au sol avant la glissade.");
            Assert.That(
                playerController.Velocity.magnitude,
                Is.GreaterThan(6.8f),
                "Le sprint simulé doit atteindre la vitesse minimale.");

            SetKeys(Key.W, Key.LeftShift, Key.LeftCtrl);
            yield return null;
            yield return new WaitForSeconds(0.12f);

            Assert.That(
                playerController.IsSliding,
                Is.True,
                "L'action Slide doit déclencher la glissade.");
            Assert.That(
                playerController.Velocity.magnitude,
                Is.GreaterThan(10.5f));
            Assert.That(characterController.height, Is.LessThan(standingHeight));
            Assert.That(
                cameraPivot.localPosition.y,
                Is.LessThan(standingCameraHeight));
            Assert.That(bodyPresentation.SlideAmount, Is.GreaterThan(0.5f));

            SetKeys();
            yield return new WaitForSeconds(1.1f);

            Assert.That(playerController.IsSliding, Is.False);
            Assert.That(characterController.height, Is.EqualTo(standingHeight)
                .Within(0.02f));
            Assert.That(cameraPivot.localPosition.y,
                Is.EqualTo(standingCameraHeight).Within(0.02f));
            Assert.That(bodyPresentation.SlideAmount,
                Is.EqualTo(0f).Within(0.02f));
        }

        [UnityTest]
        public IEnumerator BodyFramingChangesWithoutTogglingItsRenderers()
        {
            bodyPresentation.SetSlideAmount(0f);
            AssertBodyRenderersEnabled();

            playerController.AddLookImpulse(-60f, 0f);
            yield return null;
            AssertBodyRenderersEnabled();

            playerController.AddLookImpulse(-24f, 0f);
            yield return null;
            AssertBodyRenderersEnabled();
        }

        [UnityTest]
        public IEnumerator StandingBodyUsesAConsistentAnatomicalAxis()
        {
            Transform torso = bodyPresentation.transform.Find("Torso");
            Transform pelvis = bodyPresentation.transform.Find("Pelvis");
            Transform leftLeg = bodyPresentation.transform.Find("LeftLeg");
            Transform rightLeg = bodyPresentation.transform.Find("RightLeg");
            Transform leftBoot = bodyPresentation.transform.Find("LeftBoot");
            Transform rightBoot = bodyPresentation.transform.Find("RightBoot");

            Assert.That(pelvis.localPosition.z,
                Is.EqualTo(torso.localPosition.z).Within(0.001f));
            Assert.That(leftLeg.localPosition.z,
                Is.EqualTo(torso.localPosition.z).Within(0.001f));
            Assert.That(rightLeg.localPosition.z,
                Is.EqualTo(torso.localPosition.z).Within(0.001f));
            Assert.That(leftBoot.localPosition.z,
                Is.GreaterThan(leftLeg.localPosition.z));
            Assert.That(rightBoot.localPosition.z,
                Is.GreaterThan(rightLeg.localPosition.z));
            yield break;
        }

        private void AssertBodyRenderersEnabled()
        {
            string[] names =
            {
                "Torso",
                "Pelvis",
                "LeftLeg",
                "RightLeg",
                "LeftBoot",
                "RightBoot"
            };

            foreach (string childName in names)
            {
                Renderer renderer = bodyPresentation.transform
                    .Find(childName)
                    .GetComponent<Renderer>();
                Assert.That(
                    renderer.enabled,
                    Is.True,
                    $"Le rendu de {childName} ne doit jamais apparaître par commutation.");
            }
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }
    }
}
