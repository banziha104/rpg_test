using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _93.RPG.Script
{
    public class GuiControl : MonoBehaviour
    {
        public GUISkin skin = null;
        private void OnGUI()
        {
            GUI.skin = skin;
        }
    }
}