using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CustomEditor(typeof(Entity), editorForChildClasses: true)]
    public sealed class EntityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                base.OnInspectorGUI();
                return;
            }
            
            var guiEnabled = GUI.enabled;
            GUI.enabled = true;
            DrawComponents();
            GUI.enabled = guiEnabled;
        }


        private void DrawComponents()
        {
            var entity = this.target as Entity;
            var components = entity.GetDataSet();

            foreach (var component in components)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                var type = component.GetType();
                var typeName = type.Name;

                EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    DrawTypeField(component, field, entity);
                }

                EditorGUI.indentLevel = indent;

                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private void DrawTypeField(object instance, FieldInfo field, Entity entity)
        {
            var fieldValue = field.GetValue(instance);
            var fieldType = field.FieldType;
            
            if (fieldType == typeof(UnityEngine.Object) || fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(field.Name, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
                var guiEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.ObjectField(fieldValue as UnityEngine.Object, fieldType, false);
                GUI.enabled = guiEnabled;
                GUILayout.EndHorizontal();
                return;
            }

            var strVal = fieldValue != null
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", fieldValue)
                : "null";

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(field.Name, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 16));
            EditorGUILayout.SelectableLabel(strVal, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            GUILayout.EndHorizontal();
        }
    }
}