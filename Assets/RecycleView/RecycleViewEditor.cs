using UnityEngine;
using UnityEditor;

namespace GameNeon.Modules.UIModule
{
    [CustomEditor(typeof(RecycleView))]
    public class UICircularScrollViewEditor : Editor
    {
        RecycleView rv;

        public override void OnInspectorGUI()
        {
            rv = (RecycleView)target;

            rv.dir = (e_Direction)EditorGUILayout.EnumPopup("Direction", rv.dir);
            rv.lines = EditorGUILayout.IntSlider("Row Or Column", rv.lines,1,10);
            rv.squareSpacing = EditorGUILayout.FloatField("Square Spacing", rv.squareSpacing);
            rv.Spacing = EditorGUILayout.Vector2Field("Spacing", rv.Spacing);
           
            rv.cell =
                (GameObject)EditorGUILayout.ObjectField("Cell", rv.cell, typeof(GameObject), true);
            rv.m_IsShowArrow = EditorGUILayout.ToggleLeft("IsShowArrow", rv.m_IsShowArrow);
            if (rv.m_IsShowArrow)
            {
                rv.m_PointingFirstArrow = (GameObject)EditorGUILayout.ObjectField("Up or Left Arrow",
                    rv.m_PointingFirstArrow, typeof(GameObject), true);
                rv.m_PointingEndArrow = (GameObject)EditorGUILayout.ObjectField("Down or Right Arrow",
                    rv.m_PointingEndArrow, typeof(GameObject), true);
            }
        }
    }
}