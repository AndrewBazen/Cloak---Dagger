using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Start.Scripts.Dice
{
    public class DiceController : MonoBehaviour
    {
        private GameObject _dice;
        private Animator _animator;
        private static readonly int MouseHover = Animator.StringToHash("MouseHover");
        private static readonly int MouseButtonDown = Animator.StringToHash("MouseButtonDown");
        private static readonly int MouseButtonUp = Animator.StringToHash("MouseButtonUp");

        private void Awake()
        {
            _dice = gameObject;
            _animator = _dice.GetComponent<Animator>();
        }

        public void OnMouseOver()
        {
            _animator.SetBool(MouseHover, true);
            if (Input.GetMouseButtonDown(0))
            {
                _animator.SetTrigger(MouseButtonDown);
            }

            if (Input.GetMouseButtonUp(0))
            {
                _animator.SetTrigger(MouseButtonUp);
            }
        }

        private void OnMouseExit()
        {
            _animator.SetBool(MouseHover, false);
        }
    }
    
}