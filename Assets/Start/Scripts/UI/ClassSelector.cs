using Start.Scripts.Character;
using Start.Scripts.Classes;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Start.Scripts.UI
{
    public class ClassSelector
    {
        public TMP_Dropdown dropdown;
        public Image playerPic;
        public CharacterInfoData player;
        public List<PlayerClass> classes;
    }
}