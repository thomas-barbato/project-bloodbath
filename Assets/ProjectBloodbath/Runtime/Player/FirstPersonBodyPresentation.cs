using UnityEngine;

namespace ProjectBloodbath.Player
{
    [DisallowMultipleComponent]
    public sealed class FirstPersonBodyPresentation : MonoBehaviour
    {
        [SerializeField] private Transform torso;
        [SerializeField] private Transform pelvis;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform leftBoot;
        [SerializeField] private Transform rightBoot;

        private TransformPose rootRestPose;
        private TransformPose torsoRestPose;
        private TransformPose pelvisRestPose;
        private TransformPose leftLegRestPose;
        private TransformPose rightLegRestPose;
        private TransformPose leftBootRestPose;
        private TransformPose rightBootRestPose;

        public float SlideAmount { get; private set; }

        public void SetSlideAmount(float amount)
        {
            SlideAmount = Mathf.Clamp01(amount);
            ApplyPose(SlideAmount);
        }

        private void Awake()
        {
            CacheReferences();
            CacheRestPose();
        }

        private void OnDisable()
        {
            SetSlideAmount(0f);
        }

        private void CacheReferences()
        {
            if (torso == null)
            {
                torso = transform.Find("Torso");
            }

            if (pelvis == null)
            {
                pelvis = transform.Find("Pelvis");
            }

            if (leftLeg == null)
            {
                leftLeg = transform.Find("LeftLeg");
            }

            if (rightLeg == null)
            {
                rightLeg = transform.Find("RightLeg");
            }

            if (leftBoot == null)
            {
                leftBoot = transform.Find("LeftBoot");
            }

            if (rightBoot == null)
            {
                rightBoot = transform.Find("RightBoot");
            }
        }

        private void CacheRestPose()
        {
            rootRestPose = new TransformPose(transform);
            torsoRestPose = new TransformPose(torso);
            pelvisRestPose = new TransformPose(pelvis);
            leftLegRestPose = new TransformPose(leftLeg);
            rightLegRestPose = new TransformPose(rightLeg);
            leftBootRestPose = new TransformPose(leftBoot);
            rightBootRestPose = new TransformPose(rightBoot);
        }

        private void ApplyPose(float amount)
        {
            rootRestPose.ApplyOffset(
                transform,
                new Vector3(0f, -0.1f, 0.18f) * amount,
                Quaternion.Euler(10f * amount, 0f, 0f));
            torsoRestPose.ApplyOffset(
                torso,
                new Vector3(0f, -0.18f, 0.12f) * amount,
                Quaternion.Euler(18f * amount, 0f, 0f));
            pelvisRestPose.ApplyOffset(
                pelvis,
                new Vector3(0f, -0.16f, 0.18f) * amount,
                Quaternion.Euler(12f * amount, 0f, 0f));
            leftLegRestPose.ApplyOffset(
                leftLeg,
                new Vector3(0f, 0.08f, 0.3f) * amount,
                Quaternion.Euler(-52f * amount, 0f, 7f * amount));
            rightLegRestPose.ApplyOffset(
                rightLeg,
                new Vector3(0f, -0.02f, 0.14f) * amount,
                Quaternion.Euler(-27f * amount, 0f, -5f * amount));
            leftBootRestPose.ApplyOffset(
                leftBoot,
                new Vector3(0f, 0.16f, 0.58f) * amount,
                Quaternion.Euler(-48f * amount, 0f, 5f * amount));
            rightBootRestPose.ApplyOffset(
                rightBoot,
                new Vector3(0f, 0.06f, 0.34f) * amount,
                Quaternion.Euler(-24f * amount, 0f, -4f * amount));
        }

        private readonly struct TransformPose
        {
            private readonly Vector3 localPosition;
            private readonly Quaternion localRotation;

            public TransformPose(Transform target)
            {
                localPosition = target != null
                    ? target.localPosition
                    : Vector3.zero;
                localRotation = target != null
                    ? target.localRotation
                    : Quaternion.identity;
            }

            public void ApplyOffset(
                Transform target,
                Vector3 positionOffset,
                Quaternion rotationOffset)
            {
                if (target == null)
                {
                    return;
                }

                target.localPosition = localPosition + positionOffset;
                target.localRotation = localRotation * rotationOffset;
            }
        }
    }
}
