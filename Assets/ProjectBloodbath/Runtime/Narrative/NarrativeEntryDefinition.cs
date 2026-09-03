using UnityEngine;

namespace ProjectBloodbath.Narrative
{
    public enum NarrativeEntryKind
    {
        TerminalReport,
        WrittenNote,
        AudioLog,
        Examination
    }

    [CreateAssetMenu(
        fileName = "NarrativeEntry",
        menuName = "Project Bloodbath/Narrative/Entry")]
    public sealed class NarrativeEntryDefinition : ScriptableObject
    {
        [SerializeField] private string identifier = "narrative_entry";
        [SerializeField] private string displayName = "Rapport";
        [SerializeField] private NarrativeEntryKind kind =
            NarrativeEntryKind.TerminalReport;
        [SerializeField] private string sourceDisplayName = string.Empty;
        [SerializeField, TextArea(5, 14)] private string body = string.Empty;

        public string Identifier => identifier;
        public string DisplayName => displayName;
        public NarrativeEntryKind Kind => kind;
        public string SourceDisplayName => sourceDisplayName;
        public string Body => body;

        public void Configure(
            string entryIdentifier,
            string entryDisplayName,
            NarrativeEntryKind entryKind,
            string sourceName,
            string entryBody)
        {
            identifier = entryIdentifier;
            displayName = entryDisplayName;
            kind = entryKind;
            sourceDisplayName = sourceName;
            body = entryBody;
            ValidateValues();
        }

        private void OnValidate()
        {
            ValidateValues();
        }

        private void ValidateValues()
        {
            identifier = string.IsNullOrWhiteSpace(identifier)
                ? "narrative_entry"
                : identifier.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Rapport"
                : displayName.Trim();
            sourceDisplayName = sourceDisplayName?.Trim() ?? string.Empty;
            body = body?.Trim() ?? string.Empty;
        }
    }
}
