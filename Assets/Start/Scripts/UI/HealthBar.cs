using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Start.Scripts.UI
{
    public class HealthBar : MonoBehaviour
    {
        public Slider slider;

        public void SetMaxHealth(int health)
        {
            slider.maxValue = health;
            slider.value = health;
        }

        public void SetHealth(int health)
        {
            slider.value = health;
        }
    }
}
