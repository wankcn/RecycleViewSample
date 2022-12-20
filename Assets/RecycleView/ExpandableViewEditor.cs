using UnityEngine;
using System.Collections;
using UnityEditor;
using WenRuo;

namespace WenRuo
{
    [CustomEditor(typeof(ExpandableView))]
    public class ExpandableViewEditor : Editor
    {
        ExpandableView list;

        public override void OnInspectorGUI()
        {
            list = (ExpandableView)target;
            list.dir = (E_Direction)EditorGUILayout.EnumPopup("Direction: ", list.dir);

            list.lines = EditorGUILayout.IntField("Row Or Column: ", list.lines);
            list.squareSpacing = EditorGUILayout.FloatField("Spacing: ", list.squareSpacing);
            list.m_ExpandButton =
                (GameObject)EditorGUILayout.ObjectField("Cell: ", list.m_ExpandButton, typeof(GameObject), true);
            list.cell = (GameObject)EditorGUILayout.ObjectField("ExpandCell: ", list.cell, typeof(GameObject), true);
            list.m_IsExpand = EditorGUILayout.ToggleLeft(" isDefaultExpand", list.m_IsExpand);
            //list.m_BackgroundMargin = EditorGUILayout.FloatField("BackgroundScale：", list.m_BackgroundMargin);
        }
    }
}