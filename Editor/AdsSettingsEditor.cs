using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Ads
{
    [CustomEditor(typeof(AdsSettings))]
    public class AdsSettingsEditor : Editor
    {
   


    

        private readonly List<ISettingSegmentGUI> _segmentGuis = new();

        private void OnEnable()
        {
            _segmentGuis.Clear();
            _segmentGuis.AddRange(new ISettingSegmentGUI[]
            {
                new AdsSettingsSegmentGUI(serializedObject),
                new ConsentSegmentGUI(serializedObject.FindProperty(AdsSettings.CONSENT_SETTINGS_FIELD))
            });

        }

        public override void OnInspectorGUI()
        {
            foreach (var settingSegmentGUI in _segmentGuis)
            {
                settingSegmentGUI.OnGUI();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            DrawFixIfNeeded();
        }

        private void DrawFixIfNeeded()
        {
            if (MissingSymbols())
            {
                if (GUILayout.Button("Fix Missing Symbols"))
                {
                    FixMissingSymbols();
                }
            }
        }

        private void FixMissingSymbols()
        {
            foreach (var settingSegmentGUIWithSymbols in _segmentGuis.OfType<ISettingSegmentGUIWithSymbols>().Where(s=>s.HasSymbolsProblem()))
            {
                settingSegmentGUIWithSymbols.FixSymbolsProblem();
            }
        }

        private bool MissingSymbols()
        {
            return _segmentGuis.OfType<ISettingSegmentGUIWithSymbols>().Any(s => s.HasSymbolsProblem());
        }
    
    }
}