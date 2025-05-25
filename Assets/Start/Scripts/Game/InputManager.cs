// InputManager.cs

using System;
using UnityEngine;

namespace Start.Scripts.Systems
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action OnPausePressed;
        public event Action OnConfirm;
        public event Action OnCancel;
        public event Action<Vector2> OnMoveInput;
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

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OnConfirm?.Invoke();

            if (Input.GetKeyDown(KeyCode.Backspace))
                OnCancel?.Invoke();

            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (move != Vector2.zero)
                OnMoveInput?.Invoke(move);
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

