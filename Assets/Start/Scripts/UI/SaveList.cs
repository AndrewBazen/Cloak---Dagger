using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;

namespace Start.Scripts.UI
{
    [Serializable]
    public class SaveList : MonoBehaviour
    {
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject savePrefab;
        
        private void Start()
        {
            GetSaveList();
        }

        private void GetSaveList()
        {
            string path = Application.persistentDataPath + "/saves/";
            List<string> files = Directory.GetFiles(path).ToList();
            
            if (files.Count == 0)
            {
                Debug.LogError("No Saves found");
                return;
            }
            foreach (var fileName in files)
            {
                var newFileName = fileName.Trim((Application.persistentDataPath + "/saves/").ToCharArray());
                var newSave = Instantiate(savePrefab, contentContainer);
                newSave.transform.localScale = Vector2.one;
                newSave.GetComponentInChildren<TextMeshProUGUI>().text = newFileName;
            }
        }
    }
}