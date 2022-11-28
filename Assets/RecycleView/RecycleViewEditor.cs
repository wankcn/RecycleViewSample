// Author: 文若
// CreateDate: 2022/10/26

using UnityEngine;
using UnityEditor;

namespace WenRuo
{
    [CustomEditor(typeof(RecycleView))]
     public class UICircularScrollViewEditor : Editor
    {
        RecycleView rv;

        public override void OnInspectorGUI()
        {
            rv = (RecycleView)target;

            rv.dir = (E_Direction)EditorGUILayout.EnumPopup("Direction", rv.dir);
            rv.lines = EditorGUILayout.IntSlider("Row Or Column", rv.lines, 1, 10);
            rv.squareSpacing = EditorGUILayout.FloatField("Square Spacing", rv.squareSpacing);
            rv.Spacing = EditorGUILayout.Vector2Field("Spacing", rv.Spacing);
            rv.paddingLeft = EditorGUILayout.FloatField("Padding Left", rv.paddingLeft);
            rv.paddingTop = EditorGUILayout.FloatField("Padding Top", rv.paddingTop);

            rv.cell =
                (GameObject)EditorGUILayout.ObjectField("Cell", rv.cell, typeof(GameObject), true);
            // rv.isShowArrow = EditorGUILayout.ToggleLeft("IsShowArrow", rv.isShowArrow);
            // if (rv.isShowArrow)
            // {
            //     rv.firstArrow = (GameObject)EditorGUILayout.ObjectField("Up or Left Arrow",
            //         rv.firstArrow, typeof(GameObject), true);
            //     rv.endArrow = (GameObject)EditorGUILayout.ObjectField("Down or Right Arrow",
            //         rv.endArrow, typeof(GameObject), true);
            // }
        }
    }
}