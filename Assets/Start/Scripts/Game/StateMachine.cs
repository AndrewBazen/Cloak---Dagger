using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Start.Scripts
{
    /* Public class: GameState
     * Description: This will allow AI within the current game instance to obtain the most current game data within
     * the scene.
     */
    public class StateMachine : MonoBehaviour
    {
        private Dictionary<Type, BaseState> _availableStates;
        
        public BaseState CurrentState { get; private set; }
        public event Action<BaseState> OnStateChanged;

        public void SetStates(Dictionary<Type, BaseState> states)
        {
            _availableStates = states;
        }

        private void Update()
        {
            if (CurrentState == null)
            {
                CurrentState = _availableStates.Values.First();
            }

            var nextState = CurrentState?.Tick();

            if (nextState != null && nextState != CurrentState?.GetType())
            {
                SwitchToNewState(nextState);
            }
        }

        public void SwitchToNewState(Type nextState)
        {
            CurrentState = _availableStates[nextState];
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}