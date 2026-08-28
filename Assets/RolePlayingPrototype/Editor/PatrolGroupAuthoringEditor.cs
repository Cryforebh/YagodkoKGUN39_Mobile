using Entities;
using SampleProject;
using UnityEditor;
using UnityEngine;

namespace RolePlayingPrototype.Editor
{
    [CustomEditor(typeof(PatrolGroupAuthoring))]
    public sealed class PatrolGroupAuthoringEditor : UnityEditor.Editor
    {
        private const float DefaultPointSpacing = 1.5f;
        private static readonly Color RouteColor = new(0.15f, 0.8f, 1f, 0.9f);

        private SerializedProperty _members;
        private SerializedProperty _points;

        private void OnEnable()
        {
            _members = serializedObject.FindProperty("_members");
            _points = serializedObject.FindProperty("_points");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Shared Patrol Group", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Drag CharacterEntity objects into Members. Their shared patrol reference is synchronized automatically.", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_members, true);
            if (EditorGUI.EndChangeCheck())
            {
                var members = ReadMembersProperty();
                AssignMembers((PatrolGroupAuthoring) target, members);
                serializedObject.Update();
            }

            if (GUILayout.Button("Select Group Members"))
            {
                SelectMembers();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Patrol Route", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Points are stored in world space. Drag numbered handles in the Scene view.", MessageType.Info);
            var pointCount = EditorGUILayout.IntSlider("Point Count", _points.arraySize, 0, PatrolRouteEditor.MaximumPointCount);
            if (pointCount != _points.arraySize)
            {
                ResizeRoute(pointCount);
            }

            EditorGUI.indentLevel++;
            for (var i = 0; i < _points.arraySize; i++)
            {
                EditorGUILayout.PropertyField(_points.GetArrayElementAtIndex(i), new GUIContent("Point " + (i + 1)));
            }
            EditorGUI.indentLevel--;

            if (serializedObject.ApplyModifiedProperties())
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying || _points == null)
            {
                return;
            }

            serializedObject.Update();
            Handles.color = RouteColor;
            for (var i = 0; i < _points.arraySize; i++)
            {
                var point = _points.GetArrayElementAtIndex(i);
                var position = point.vector3Value;
                var handleSize = HandleUtility.GetHandleSize(position) * 0.12f;
                Handles.SphereHandleCap(0, position, Quaternion.identity, handleSize, EventType.Repaint);
                Handles.Label(position + Vector3.up * handleSize, (i + 1).ToString());

                EditorGUI.BeginChangeCheck();
                var newPosition = Handles.PositionHandle(position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move Shared Patrol Point");
                    point.vector3Value = newPosition;
                }

                if (i > 0)
                {
                    Handles.DrawLine(_points.GetArrayElementAtIndex(i - 1).vector3Value, position, 3f);
                }
            }

            if (_points.arraySize > 1)
            {
                Handles.DrawLine(_points.GetArrayElementAtIndex(_points.arraySize - 1).vector3Value, _points.GetArrayElementAtIndex(0).vector3Value, 3f);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }

        public static void AssignMembers(PatrolGroupAuthoring group, CharacterEntity[] members)
        {
            var groupSerializedObject = new SerializedObject(group);
            var currentMembers = groupSerializedObject.FindProperty("_members");
            for (var i = 0; i < currentMembers.arraySize; i++)
            {
                var oldMember = currentMembers.GetArrayElementAtIndex(i).objectReferenceValue as CharacterEntity;
                if (oldMember != null)
                {
                    SetMemberGroup(oldMember, group, null);
                }
            }

            currentMembers.arraySize = members.Length;
            for (var i = 0; i < members.Length; i++)
            {
                currentMembers.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
                SetMemberGroup(members[i], null, group);
            }

            groupSerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(group);
        }

        private static void SetMemberGroup(CharacterEntity member, PatrolGroupAuthoring expectedGroup, PatrolGroupAuthoring newGroup)
        {
            var memberSerializedObject = new SerializedObject(member);
            var groupProperty = memberSerializedObject.FindProperty("_initialPatrolGroup");
            if (expectedGroup != null && groupProperty.objectReferenceValue != expectedGroup)
            {
                return;
            }

            groupProperty.objectReferenceValue = newGroup;
            memberSerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(member);
        }

        private CharacterEntity[] ReadMembersProperty()
        {
            var members = new System.Collections.Generic.List<CharacterEntity>(_members.arraySize);
            for (var i = 0; i < _members.arraySize; i++)
            {
                var member = _members.GetArrayElementAtIndex(i).objectReferenceValue as CharacterEntity;
                if (member != null && !members.Contains(member))
                {
                    members.Add(member);
                }
            }

            return members.ToArray();
        }

        private void SelectMembers()
        {
            var objects = new Object[_members.arraySize];
            for (var i = 0; i < _members.arraySize; i++)
            {
                objects[i] = _members.GetArrayElementAtIndex(i).objectReferenceValue;
            }

            Selection.objects = objects;
        }

        private void ResizeRoute(int pointCount)
        {
            var previousCount = _points.arraySize;
            _points.arraySize = pointCount;
            if (pointCount <= previousCount)
            {
                return;
            }

            var group = (PatrolGroupAuthoring) target;
            for (var i = previousCount; i < pointCount; i++)
            {
                _points.GetArrayElementAtIndex(i).vector3Value = group.transform.position + group.transform.right * DefaultPointSpacing * (i + 1);
            }
        }
    }
}
