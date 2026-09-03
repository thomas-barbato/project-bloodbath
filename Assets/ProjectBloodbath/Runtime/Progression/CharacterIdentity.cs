using System;
using UnityEngine;

namespace ProjectBloodbath.Progression
{
    [DisallowMultipleComponent]
    public sealed class CharacterIdentity : MonoBehaviour
    {
        [SerializeField] private string characterName = "Mara Voss";
        [SerializeField] private string classDisplayName = "Classe prototype";

        public event Action IdentityChanged;

        public string CharacterName => characterName;
        public string ClassDisplayName => classDisplayName;

        public void Configure(string newCharacterName, string newClassName)
        {
            characterName = Normalize(newCharacterName, "Personnage");
            classDisplayName = Normalize(
                newClassName,
                "Classe prototype");
            IdentityChanged?.Invoke();
        }

        private void OnValidate()
        {
            characterName = Normalize(characterName, "Personnage");
            classDisplayName = Normalize(
                classDisplayName,
                "Classe prototype");
        }

        private static string Normalize(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
        }
    }
}
