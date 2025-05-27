///스크립트 생성 일자 - 2025 - 03 - 05
///스크립트 담당자 - 최현규
///스크립트 생성 버전 - 0.1

using System;
using System.Collections.Generic;
using System.Linq;
using TempNamespace.Character.Controller.Physics;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TempNamespace.Character.Controller
{
	[CustomEditor(typeof(PhysicsBasedController), editorForChildClasses: true)]
	public class CharacterControllerEditor : Editor
	{
		#region Enums
		protected static readonly List<GUIContent> tabNames =
		new List<GUIContent>{
			new GUIContent("Physics", "Character's Physics Setting"),
			new GUIContent("Settings", "Character's Movement Setting"),
			new GUIContent("States", "Movement States"),
			new GUIContent("Actions", "Actions"),
			new GUIContent("Stances", "Animation States"),
			new GUIContent("Debug", "Show Debug Datas")
		};
		public enum Tabs
		{
			Physics, Settings, States, Actions, Stances, Debug, DefaultTabCount
		}
		#endregion

		#region Constants		
		public const string kDefaultStatePath = "Assets/Datas/MoveState";

		#endregion

		#region Fields
		/// <summary>
		/// Character Movement Reference
		/// </summary>
		PhysicsBasedController movement = null;
		Editor characterPhysicsEditor = null;
		int currentTabIndex = 0;

		//State
		private ReorderableList _reorderableStatesList;
		private SerializedProperty _statesList;
		private Editor _stateDetailEditor;
		//

		//Stance
		private ReorderableList _reorderableStancesList;
		private SerializedProperty _stancesList;
		private Editor _stanceDetailEditor;
		//

		
		//Actions
		private ReorderableList _reorderableActionsList;
		private SerializedProperty _actionsList;
		private Editor _actionDetailEditor;
		//
		#endregion

		#region Properties
		#endregion

		
		#region Initialize / Delete
		void OnEnable()
		{
			GenerateTabNames();
			movement = (PhysicsBasedController)target;

			_statesList = serializedObject.FindProperty("_moveStates");
			_reorderableStatesList = new ReorderableList(serializedObject, _statesList,
				true, true, true, true)
			{
				drawHeaderCallback = OnDrawStateListHeader,
				drawElementCallback = OnDrawStateListElement,
				onAddCallback = OnAddStateList,
				onRemoveCallback = OnRemoveStateList
			};

			_stancesList = serializedObject.FindProperty("_characterStances");
			_reorderableStancesList = new ReorderableList(serializedObject, _stancesList,
				true, true, true, true)
			{
				drawHeaderCallback = OnDrawStanceListHeader,
				drawElementCallback = OnDrawStanceListElement,
				onAddCallback = OnAddStanceList,
				onRemoveCallback = OnRemoveStanceList
			};

			_actionsList = serializedObject.FindProperty("_characterActions");
			_reorderableActionsList = new ReorderableList(serializedObject, _actionsList,
				true, true, true, true)
			{
				drawHeaderCallback = OnDrawActionListHeader,
				drawElementCallback = OnDrawActionListElement,
				onAddCallback = OnAddActionList,
				onRemoveCallback = OnRemoveActionList
			};
		}

        /// <summary>
        /// Custom Tab을 사용한다면 이 함수를 override해서 tabNames에 값 추가
        /// </summary>
        protected virtual void GenerateTabNames()
        {
        }

        void OnDisable()
		{
			if(characterPhysicsEditor != null)
				DestroyImmediate(characterPhysicsEditor);
		}
		#endregion
		
		#region Draw Inspector
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if(characterPhysicsEditor == null)
				characterPhysicsEditor = CreateEditor(movement.physicsSystem);

			EditorGUI.BeginChangeCheck();
			
			currentTabIndex = GUILayout.SelectionGrid(currentTabIndex, tabNames.ToArray(), 4);
			
			switch((Tabs)currentTabIndex)
			{
				case Tabs.Physics:
					DrawMovementSystem();
					break;
				case Tabs.Settings:
					DrawSettings();
					break;					
				case Tabs.States:
					DrawMovementStates();
					break;
				case Tabs.Actions:
					DrawMovementActions();
					break;
				case Tabs.Stances:
					DrawMovementStances();
					break;
				case Tabs.Debug:
					DrawDebugDatas();
					break;
			}

			if(EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(movement);
			}
		}

        #region Movement System
        void DrawMovementSystem()
		{
			characterPhysicsEditor.OnInspectorGUI();
		}
		#endregion
		#region Settings
        private void DrawSettings()
        {	
			DrawDefaultInspector();			          
        }
		#endregion
		#region Movement States Listup
		bool drawDetailState = false;
		void DrawMovementStates()
		{
			var initialState = serializedObject.FindProperty("_initialMoveState");
			var prevObject = initialState.objectReferenceValue;
			EditorGUILayout.PropertyField(initialState);
			if(prevObject != initialState.objectReferenceValue)
			{
				bool needAdd = true;
				for(int i = 0; i < _statesList.arraySize; ++i)
				{
					if(_statesList.GetArrayElementAtIndex(i).objectReferenceValue == initialState.objectReferenceValue)
					{
						needAdd = false;
						_reorderableStatesList.Select(i);
						break;
					}
				}
				if(needAdd)
				{
					_statesList.InsertArrayElementAtIndex(0);
					_statesList.GetArrayElementAtIndex(0).objectReferenceValue = initialState.objectReferenceValue;
					_reorderableStatesList.Select(0);
				}

				serializedObject.ApplyModifiedProperties();
			}
			

			_reorderableStatesList.DoLayoutList();
			HandleDragAndDrop<MoveState>(_statesList);
			int idx = _reorderableStatesList.index;

			EditorGUILayout.Space(25);

			if(idx != -1)
			{
				if(movement.MoveStates[idx] != null)
				{
					drawDetailState = GUILayout.Toggle(drawDetailState, "Show Detail", EditorStyles.foldoutHeader);
				}
				else
				{
					drawDetailState = false;
				}
				GUILayout.Space(-20);
				using(new EditorGUI.DisabledGroupScope(true))
				{
					EditorGUILayout.ObjectField("		", movement.MoveStates[idx], 
						typeof(MoveState), false);
				}

				if(drawDetailState)
				{
					CreateCachedEditor(movement.MoveStates[idx], null, ref _stateDetailEditor);

					_stateDetailEditor.OnInspectorGUI();
					_stateDetailEditor.serializedObject.ApplyModifiedProperties();
				}
			}
		}
        private void OnDrawStateListHeader(Rect rc)
		{
			EditorGUI.LabelField(rc, "Movement System Checks States Up To Down");
		}        
		private void OnDrawStateListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
			var state = movement.MoveStates[index];
			if(state == null)
			{
				movement.MoveStates[index] = (MoveState)EditorGUI.ObjectField(rect, movement.MoveStates[index], typeof(MoveState), false);
			}
			else
			{
				var rcObject = rect;
				rcObject.x += 25;
				rcObject.xMax -= 25;				
				movement.MoveStates[index] = (MoveState)EditorGUI.ObjectField(rcObject, movement.MoveStates[index], typeof(MoveState), false);
			}
        }
		private void OnAddStateList(ReorderableList list)
		{
			_statesList.InsertArrayElementAtIndex(0);
			_statesList.GetArrayElementAtIndex(0).objectReferenceValue = null;
			_statesList.serializedObject.ApplyModifiedProperties();
		}
        private void OnRemoveStateList(ReorderableList list)
        {
			var initialState = serializedObject.FindProperty("_initialMoveState");
			if(initialState.objectReferenceValue == _statesList.GetArrayElementAtIndex(list.index).objectReferenceValue)
				initialState.objectReferenceValue = null;
			_statesList.DeleteArrayElementAtIndex(list.index);
			_statesList.serializedObject.ApplyModifiedProperties();
			_reorderableStatesList.index = -1;
        }
		#endregion

		#region Movement Action List
		bool drawDetailActions = false;
        private void DrawMovementActions()
        {
			_reorderableActionsList.DoLayoutList();
			HandleDragAndDrop<CharacterAction>(_actionsList);
			int idx = _reorderableActionsList.index;
			EditorGUILayout.Space(25);

			if(idx != -1)
			{
				if(movement.CharacterActions[idx] != null)
				{
					drawDetailActions = GUILayout.Toggle(drawDetailActions, "Show Detail", EditorStyles.foldoutHeader);
				}
				else
				{
					drawDetailActions = false;
				}
				GUILayout.Space(-20);
				using(new EditorGUI.DisabledGroupScope(true))
				{
					EditorGUILayout.ObjectField("		", movement.CharacterActions[idx], typeof(CharacterAction), false);
				}
				if(drawDetailActions)
				{
					CreateCachedEditor(movement.CharacterActions[idx], null, ref _actionDetailEditor);

					_actionDetailEditor.OnInspectorGUI();
					_actionDetailEditor.serializedObject.ApplyModifiedProperties();
				}
			}
        }
		
        private void OnDrawActionListHeader(Rect rc)
        {
			EditorGUI.LabelField(rc, "Movement System Updates Actions Up To Down");

        }
        private void OnDrawActionListElement(Rect rc, int index, bool isActive, bool isFocused)
        {
			var state = movement.CharacterActions[index];
			if(state == null)
			{
				movement.CharacterActions[index] = (CharacterAction)EditorGUI.ObjectField(rc, movement.CharacterActions[index], typeof(CharacterAction), false);
			}
			else
			{
				var rcObject = rc;
				rcObject.x += 25;
				rcObject.xMax -= 25;				
				movement.CharacterActions[index] = (CharacterAction)EditorGUI.ObjectField(rcObject, movement.CharacterActions[index], typeof(CharacterAction), false);
			}
        }
        private void OnAddActionList(ReorderableList list)
        {
            _actionsList.InsertArrayElementAtIndex(0);
			_actionsList.GetArrayElementAtIndex(0).objectReferenceValue = null;
			_actionsList.serializedObject.ApplyModifiedProperties();
        }
        private void OnRemoveActionList(ReorderableList list)
        {
			_actionsList.DeleteArrayElementAtIndex(list.index);
			_actionsList.serializedObject.ApplyModifiedProperties();
			_reorderableActionsList.index = -1;
        }

		#endregion
		
		#region Movement Stance List
		bool drawDetailStances = false;
		private void DrawMovementStances()
        {
			_reorderableStancesList.DoLayoutList();
			HandleDragAndDrop<CharacterStance>(_stancesList);
			int idx = _reorderableStancesList.index;
			EditorGUILayout.Space(25);
			
			if(idx != -1)
			{
				if(movement.CharacterStances[idx] != null)
				{
					drawDetailStances = GUILayout.Toggle(drawDetailStances, "Show Detail", EditorStyles.foldoutHeader);
				}
				else
				{
					drawDetailStances = false;
				}

				GUILayout.Space(-20);
				using(new EditorGUI.DisabledGroupScope(true))
				{
					EditorGUILayout.ObjectField("		", movement.CharacterStances[idx], typeof(CharacterStance), false);
				}

				if(drawDetailStances)
				{
					CreateCachedEditor(movement.CharacterStances[idx], null, ref _stanceDetailEditor);

					_stanceDetailEditor.OnInspectorGUI();
					_stanceDetailEditor.serializedObject.ApplyModifiedProperties();
				}
			}
        }
        private void OnDrawStanceListHeader(Rect rc)
		{
			EditorGUI.LabelField(rc, "Movement System Checks Stances Up To Down");
		}
		private void OnDrawStanceListElement(Rect rc, int index, bool isActive, bool isFocused)
        {
			var state = movement.CharacterStances[index];
			if(state == null)
			{
				movement.CharacterStances[index] = (CharacterStance)EditorGUI.ObjectField(rc, movement.CharacterStances[index], typeof(CharacterStance), false);
			}
			else
			{				
				var rcObject = rc;
				rcObject.x += 25;
				rcObject.xMax -= 25;			
				movement.CharacterStances[index] = (CharacterStance)EditorGUI.ObjectField(rcObject, movement.CharacterStances[index], typeof(CharacterStance), false);
			}
        }
		private void OnAddStanceList(ReorderableList list)
		{
			_stancesList.InsertArrayElementAtIndex(0);
			_stancesList.GetArrayElementAtIndex(0).objectReferenceValue = null;
			_stancesList.serializedObject.ApplyModifiedProperties();
		}
        private void OnRemoveStanceList(ReorderableList list)
        {
			_stancesList.DeleteArrayElementAtIndex(list.index);
			_stancesList.serializedObject.ApplyModifiedProperties();
			_reorderableStancesList.index = -1;
        }
		#endregion

		#region Handle Drag
		
		/// <summary>
		/// Drag And Drop으로 등록 처리
		/// </summary>
		
        private void HandleDragAndDrop<T>(SerializedProperty property) where T : UnityEngine.Object
        {
			Event currentEvent = Event.current;

			if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
			{
				if (DragAndDrop.objectReferences.Length > 0)
				{
					bool validDrag = false;

					foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
					{
						if (obj is T)
						{
							validDrag = true;
							break;
						}
					}

					if (validDrag)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					}
					else
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					}
				}
			}
			if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is T)
                    {
						var element = obj as T;
												
                        property.serializedObject.Update();
						
						property.InsertArrayElementAtIndex(0);
						property.GetArrayElementAtIndex(0).objectReferenceValue = element;

						property.serializedObject.ApplyModifiedProperties();

                    }
                }
                currentEvent.Use();
            }
        }
		#endregion

		#region Debug Datas
        private void DrawDebugDatas()
        {
			if(!Application.isPlaying)
			{
				GUILayout.Label("Display Datas on Play Mode");
				return;
			}

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				GUILayout.Label("Ground Collision", EditorStyles.miniBoldLabel);

				FindGroundResult currentGround = movement.physicsSystem.CurrentGround;
				GUILayout.Label("Found Ground: " + currentGround.hitGround);
				GUILayout.Label("Is Walkable Ground: " + currentGround.isWalkable);
				GUILayout.Label("Was Raycast: " + currentGround.isRaycastResult);
				GUILayout.Label("Distance For Ground: " + currentGround.GetDistanceToGround());
			}
			
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				GUILayout.Label("Current State", EditorStyles.miniBoldLabel);
				if(movement.CurrentState == null)
				{
					GUILayout.Label("Current State: NULL");
				}
				else
				{
					GUILayout.Label("Current State: " + movement.CurrentState.StateName);
				}
			}
			
        }
		#endregion

		#endregion
	}
}