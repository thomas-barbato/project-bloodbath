using ProjectBloodbath.Input;
using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    [DefaultExecutionOrder(900)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PrototypeInterfaceCoordinator : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;

        private IPrototypeModalView activeView;

        public bool HasOpenView =>
            activeView != null && activeView.IsOpen;

        public IPrototypeModalView ActiveView => activeView;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }
        }

        private void OnDisable()
        {
            IPrototypeModalView previous = activeView;
            activeView = null;
            previous?.CloseFromCoordinator();
            ApplyInputState(false);
        }

        public void Open(IPrototypeModalView view)
        {
            if (view == null || activeView == view)
            {
                return;
            }

            IPrototypeModalView previous = activeView;
            activeView = null;
            previous?.CloseFromCoordinator();
            activeView = view;
            ApplyInputState(true);
        }

        public void Close(IPrototypeModalView view)
        {
            if (view == null || activeView != view)
            {
                return;
            }

            activeView = null;
            ApplyInputState(false);
        }

        private void ApplyInputState(bool menuOpen)
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }
            inputReader?.SetGameplaySuppressed(menuOpen);
            Cursor.lockState = menuOpen
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = menuOpen;
            if (!menuOpen)
            {
                PrototypeInterfaceCursor.Reset();
            }
        }
    }
}
