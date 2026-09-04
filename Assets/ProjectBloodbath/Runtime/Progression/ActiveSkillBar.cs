using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    public enum SkillAssignmentBlocker
    {
        None,
        InvalidSlot,
        MissingDefinition,
        PassiveSkill,
        SkillNotLearned
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterSkillProgression))]
    public sealed class ActiveSkillBar : MonoBehaviour
    {
        public const int SlotCount = 5;

        [SerializeField] private CharacterSkillProgression skillProgression;
        [SerializeField] private SkillDefinition[] slots =
            new SkillDefinition[SlotCount];

        public event Action<int, SkillDefinition> SlotChanged;

        public CharacterSkillProgression SkillProgression => skillProgression;
        public int Capacity => SlotCount;

        public void Configure(CharacterSkillProgression characterSkills)
        {
            skillProgression = characterSkills;
            NormalizeSlots();
        }

        public SkillDefinition GetSkill(int slotIndex)
        {
            return IsValidSlot(slotIndex) ? slots[slotIndex] : null;
        }

        public int GetEffectiveRank(int slotIndex)
        {
            SkillDefinition skill = GetSkill(slotIndex);
            return skill == null || skillProgression == null
                ? 0
                : skillProgression.GetEffectiveRank(skill);
        }

        public SkillAssignmentBlocker GetAssignmentBlocker(
            int slotIndex,
            SkillDefinition skill)
        {
            if (!IsValidSlot(slotIndex))
            {
                return SkillAssignmentBlocker.InvalidSlot;
            }

            if (skill == null)
            {
                return SkillAssignmentBlocker.MissingDefinition;
            }

            if (skill.SkillType != SkillType.Active)
            {
                return SkillAssignmentBlocker.PassiveSkill;
            }

            return
                skillProgression == null ||
                skillProgression.GetEffectiveRank(skill) <= 0
                    ? SkillAssignmentBlocker.SkillNotLearned
                    : SkillAssignmentBlocker.None;
        }

        public bool TryAssign(int slotIndex, SkillDefinition skill)
        {
            if (
                GetAssignmentBlocker(slotIndex, skill) !=
                SkillAssignmentBlocker.None)
            {
                return false;
            }

            for (int index = 0; index < slots.Length; index++)
            {
                if (index != slotIndex && slots[index] == skill)
                {
                    slots[index] = null;
                    SlotChanged?.Invoke(index, null);
                }
            }

            if (slots[slotIndex] == skill)
            {
                return false;
            }

            slots[slotIndex] = skill;
            SlotChanged?.Invoke(slotIndex, skill);
            return true;
        }

        public bool Clear(int slotIndex)
        {
            if (!IsValidSlot(slotIndex) || slots[slotIndex] == null)
            {
                return false;
            }

            slots[slotIndex] = null;
            SlotChanged?.Invoke(slotIndex, null);
            return true;
        }

        private void Awake()
        {
            if (skillProgression == null)
            {
                skillProgression = GetComponent<CharacterSkillProgression>();
            }

            NormalizeSlots();
        }

        private void OnValidate()
        {
            NormalizeSlots();
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < SlotCount;
        }

        private void NormalizeSlots()
        {
            if (slots == null || slots.Length != SlotCount)
            {
                SkillDefinition[] normalized =
                    new SkillDefinition[SlotCount];
                if (slots != null)
                {
                    Array.Copy(
                        slots,
                        normalized,
                        Mathf.Min(slots.Length, normalized.Length));
                }

                slots = normalized;
            }

            for (int index = 0; index < slots.Length; index++)
            {
                SkillDefinition skill = slots[index];
                if (skill == null || skill.SkillType == SkillType.Active)
                {
                    continue;
                }

                slots[index] = null;
            }
        }
    }
}
