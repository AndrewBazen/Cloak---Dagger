// InputManager.cs

using System;
using UnityEngine;
using Start.Scripts.Character;
namespace Start.Scripts.Game
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action OnPausePressed;
        public event Action OnConfirm;
        public event Action OnCancel;
        public event Action OnMoveInput;
        public event Action<Vector2> OnMouseMoved;
        public event Action OnPrimaryClick;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnPausePressed?.Invoke();
                OnCancel?.Invoke();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OnConfirm?.Invoke();

            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                OnMoveInput?.Invoke();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
                OnPrimaryClick?.Invoke();

            Vector2 mousePosition = Input.mousePosition;
            OnMouseMoved?.Invoke(mousePosition);
        }
    }
}

