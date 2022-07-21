using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    /// <summary>
    /// Base class for actions; all actions should derive from this.
    /// </summary>
    public class Action : ScriptableObject
    {
        public enum State
        {
            Idle,
            Running,
            EndNextFrame,
            Disabled
        }

        /// <summary>
        /// Is visible on graph editor?
        /// </summary>
        [SerializeField]
        private bool isVisible = true;

        /// <summary>
        /// Current internal state. 
        /// </summary>
        private State state;

        /// <summary>
        /// Editor position.
        /// </summary>
        [SerializeField]
        public Rect size;

        /// <summary>
        /// Enabled on start.
        /// </summary>
        [SerializeField]
        private bool enabled = true;

        /// <summary>
        /// Execute on startup.
        /// </summary>
        [SerializeField]
        private bool startUp = false;

        /// <summary>
        /// Action ID.
        /// </summary>
        [SerializeField]
        private string id = "";

        /// <summary>
        /// Wait for all inputs before starting.
        /// </summary>
        [SerializeField]
        bool waitForAllInputs;

        /// <summary>
        /// Total start requests.
        /// </summary>
        protected List<Action> startRequests = new List<Action>();

        /// <summary>
        /// An optional parameter for passing in some context.
        /// </summary>
        public GameObject Target { get; set; }

        /// <summary>
        /// Start requests accessor.
        /// </summary>
        protected List<Action> StartRequests
        {
            get
            {
                if (startRequests == null)
                {
                    startRequests = new List<Action>();
                }

                return startRequests;
            }
        }

        public string ID
        {
            get
            {
                return id;
            }
        }

        public bool IsStartupAction
        {
            get
            {
                return startUp;
            }
        }

        public bool IsDefaultEnabled
        {
            get
            {
                return enabled;
            }
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
            }
        }

        /// <summary>
        /// Current state of the action.
        /// </summary>
        public State CurrentState
        {
            get
            {
                return state;
            }
            protected set
            {
                state = value;
            }
        }

        public bool HasEnded
        {
            get
            {
                return state == State.EndNextFrame;
            }
        }

        public List<Action> In
        {
            get
            {
                return inNodes;
            }
        }

        public List<Action> Out
        {
            get
            {
                return outNodes;
            }
        }

        /// <summary>
        /// A list of nodes that can start this action.
        /// </summary>
        [SerializeField]
        protected List<Action> inNodes = new List<Action>();

        /// <summary>
        /// List of nodes to go to next.
        /// </summary>
        [SerializeField]
        protected List<Action> outNodes = new List<Action>();

        /// <summary>
        /// Called by Action Controller.
        /// </summary>
        /// <returns></returns>
        public bool Enter(Action parent = null)
        {
            if (state == State.Idle)
            {
                if (parent != null && StartRequests.Contains(parent) == false)
                {
                    startRequests.Add(parent);
                }

                if (parent != null && waitForAllInputs == true && StartRequests.Count != In.Count)
                {
                    return false;
                }

                state = State.Running;

                OnStart();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Called by Action Controller.
        /// </summary>
        public void Update()
        {
            if (state != State.Running)
            {
                return;
            }

            OnUpdate();
        }

        /// <summary>
        /// Called by Action Controller.
        /// </summary>
        public void FixedUpdate()
        {
            if (state != State.Running)
            {
                return;
            }

            OnFixedUpdate();
        }

        /// <summary>
        /// Called by Action Controller.
        /// </summary>
        public void LateUpdate()
        {
            if (state != State.Running)
            {
                return;
            }

            OnLateUpdate();
        }

        /// <summary>
        /// Called by Action Controller.
        /// </summary>
        /// <returns></returns>
        public List<Action> End()
        {
            if (state == State.EndNextFrame)
            {
                StartRequests.Clear();

                var next = OnEnd();

                // {ass on teh shared target.
                if(next != null)
                {
                    foreach(var item in next)
                    {
                        item.Target = Target;
                    }
                }

                state = State.Idle;

                return next;
            }

            return null;
        }

        /// <summary>
        /// Callback for interruptions.
        /// </summary>
        public void Interrupt()
        {
            if (state != State.Running)
            {
                return;
            }

            OnInterrupt();
        }

        /// <summary>
        /// On Enter callback.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// On Update callback.
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// On Fixed Update callback.
        /// </summary>
        protected virtual void OnFixedUpdate()
        {
        }

        /// <summary>
        /// On Late Update callback.
        /// </summary>
        protected virtual void OnLateUpdate()
        {
        }

        /// <summary>
        /// On Interrupt callback.
        /// </summary>
        protected virtual void OnInterrupt()
        {
            // Default is just to idle.
            CurrentState = State.Idle;
        }

        /// <summary>
        /// On end callback; decide which nodes to travel to next.
        /// </summary>
        /// <returns></returns>
        protected virtual List<Action> OnEnd()
        {
            return outNodes;
        }

        /// <summary>
        /// Enable; can start.
        /// </summary>
        public void Enable()
        {
            CurrentState = State.Idle;
        }

        /// <summary>
        /// Disable; prevents this node from being started.
        /// </summary>
        public void Disable()
        {
            CurrentState = State.Disabled;
        }

        public virtual string BaseWindowSkin
        {
            get
            {
                return "flow node 0";
            }
        }

        /// <summary>
        /// Has a data error; designer error.
        /// </summary>
        protected bool hasDataError = false;

        /// <summary>
        /// Reset internal state.
        /// </summary>
        internal void Reset()
        {
            if (enabled == true)
            {
                // Enabled on start.
                CurrentState = State.Idle;
            }
            else
            {
                // Disabled on start.
                CurrentState = State.Disabled;
            }

            // Clean up any nodes.
            if (In != null)
            {
                In.RemoveAll(action => action == null);
            }

            if (Out != null)
            {
                Out.RemoveAll(action => action == null);
            }
        }

        internal void CleanUp()
        {
            OnCleanUp();
        }

        internal virtual void OnCleanUp()
        {
        }

#if !SQUAX_RELEASE_MODE
        static public List<Action> SelectedAction = new List<Action>();

        static public Action FirstSelection() { return SelectedAction[0]; }

        static public bool ContainsSelection(Action item) { return SelectedAction.Contains(item); }

        static public bool HasSelection() { return SelectedAction.Count > 0; }

        static public void ClearSelection() { SelectedAction.Clear(); }

        static public void AddToSelection(Action item) { if (SelectedAction.Contains(item) == true) return; SelectedAction.Add(item); }

        public static string AssetPath = "";

        public static Action CopyAction = null;

        public static Action HighlightOutAction = null;

        public static int CurrentLayerID = 0;

        private bool allowDragging = false;

        public class RightClickMenuItem
        {
            public UnityEditor.GenericMenu.MenuFunction2 Function { get; private set; }
            public string Text { get; private set; }

            public RightClickMenuItem(string text, UnityEditor.GenericMenu.MenuFunction2 function)
            {
                Function = function;
                Text = text;
            }
        }

        public static bool WindowInfoEnabled = true;


        /// <summary>
        /// Dirty flag.
        /// </summary>
        protected bool isDirty = false;

        /// <summary>
        /// Marked for deletion.
        /// </summary>
        protected bool isDelete = false;

        public bool IsDelete
        {
            get
            {
                return isDelete;
            }
        }

        /// <summary>
        /// Title used in tool.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return "Action";
            }
        }

        public virtual Vector2 Dimensions
        {
            get
            {
                return new Vector2(60, 60);
            }
        }

        /// <summary>
        /// The editor serialised object.
        /// </summary>
        protected SerializedObject serializedObject;

        virtual public void OnEnable()
        {
            serializedObject = new SerializedObject(this);

            SetPosition(new Vector2(size.x, size.y));

            Reset();
        }

        public void SetPosition(Vector2 position)
        {
            size = new Rect(position.x, position.y, Dimensions.x, Dimensions.y);
        }

        protected void Link(Action target)
        {
            if (target.inNodes.Contains(this) == false)
            {
                target.inNodes.Add(this);
            }

            if (outNodes.Contains(target) == false)
            {
                outNodes.Add(target);
            }
        }

        public void Unlink(Action target)
        {
            if (target == null)
            {
                return;
            }

            inNodes.Remove(target);

            if (target.inNodes.Contains(this) == true)
            {
                target.inNodes.Remove(this);
            }

            if (target.outNodes.Contains(this) == true)
            {
                target.outNodes.Remove(this);
            }
        }

        /// <summary>
        /// Display a context menu.
        /// </summary>
        /// <param name="options"></param>
        protected void DisplayMenu(RightClickMenuItem[] options)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var menuItem in options)
            {
                menu.AddItem(new GUIContent(menuItem.Text), false, menuItem.Function, 0);
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Window function for rendering.
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowFunction(int windowID)
        {
            if (UnityEngine.Event.current.type == EventType.MouseDown)
            {
                if (UnityEngine.Event.current.shift == true && ContainsSelection(this) == false && HasSelection() == true)
                {
                    // Attempting a connect.
                    FirstSelection().Link(this);
                }

                ClearSelection();

                AddToSelection(this);
            }
            else if (UnityEngine.Event.current.type == EventType.MouseUp || UnityEngine.Event.current.type == EventType.MouseMove)
            {
                GUI.FocusWindow(windowID);
            }

            // Right mouse click
            if (UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 1)
            {
                OnRightClickMenu();

                UnityEngine.Event.current.Use();
            }

            if (WindowInfoEnabled == true)
            {
                OnWindowGUI(windowID);
            }

            if (allowDragging == true)
            {
                GUI.DragWindow();
            }
        }

        /// <summary>
        /// Client window draw callback.
        /// </summary>
        /// <param name="windowID"></param>
        virtual protected void OnWindowGUI(int windowID)
        {
        }

        /// <summary>
        /// Right click within window callback.
        /// </summary>
        virtual protected void OnRightClickMenu()
        {
            var menuList = GenerateDefaultMenuItems();

            DisplayMenu(menuList.ToArray());
        }

        protected List<RightClickMenuItem> GenerateDefaultMenuItems()
        {
            var menuList = new List<RightClickMenuItem>() {
                            new RightClickMenuItem("Delete", (object userData) =>
                            {
                                if(EditorUtility.DisplayDialog("Confirm delete?", "This can not be undone.", "Ok, delete", "Cancel"))
                                {
                                    Action.SelectedAction.Clear();

                                    isDelete = true;
                                    isDirty = true;
                                }
                            }),
                            new RightClickMenuItem("Disconnect", (object userData) =>
                            {
                                List<Action> copy = new List<Action>(Out);
                                foreach (var node in copy)
                                {
                                    Unlink(node);
                                }

                                copy = new List<Action>(In);
                                foreach (var node in copy)
                                {
                                    Unlink(node);
                                }

                                Out.Clear();
                                In.Clear();

                                isDirty = true;
                            })
                        };

            // Copy action.
            menuList.Add(new RightClickMenuItem("Copy Action", (object userData) =>
            {
                CopyAction = this;
            }));

            if (Application.isPlaying == true)
            {
                // Add play mode.
                menuList.Add(new RightClickMenuItem("Play", (object userData) =>
                {
                    ActionController.Instance.StartJob(this);
                }));

            }

            return menuList;
        }

        /// <summary>
        /// Add an action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="position"></param>
        static public Action AddAction<T>(Vector2 position) where T : Action
        {
            if (string.IsNullOrEmpty(AssetPath) == true)
            {
                return null;
            }

            var action = ScriptableObject.CreateInstance<T>();

            action.SetPosition(position);

            AssetDatabase.AddObjectToAsset(action, AssetPath);

            AssetDatabase.SaveAssets();

            return action;
        }

        static public Action AddAction(Vector2 position, Type type)
        {
            if (string.IsNullOrEmpty(AssetPath) == true)
            {
                return null;
            }

            var action = ScriptableObject.CreateInstance(type) as Action;

            action.SetPosition(position);

            AssetDatabase.AddObjectToAsset(action, AssetPath);

            AssetDatabase.SaveAssets();

            return action;
        }

        /// <summary>
        /// Add an action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="position"></param>
        static public Action PasteAction(Action clone, Vector2 position)
        {
            if (string.IsNullOrEmpty(AssetPath) == true)
            {
                return null;
            }

            var action = GameObject.Instantiate(clone) as Action;

            action.Out.Clear();
            action.In.Clear();

            action.SetPosition(position);

            AssetDatabase.AddObjectToAsset(action, AssetPath);

            AssetDatabase.SaveAssets();

            return action;
        }

        /// <summary>
        /// Window entry drawing code.
        /// </summary>
        /// <param name="windowID"></param>
        /// <returns></returns>
        public bool DrawWindow(int windowID, string overrideTitle = null, bool dragEnabled = false)
        {
            allowDragging = dragEnabled;

            var skin = UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);

            var styleName = BaseWindowSkin;

            if (SelectedAction.Contains(this))
                styleName += " on";

            var style = skin.customStyles.First(s => s.name == styleName);

            size = GUI.Window(windowID, size, WindowFunction, ((string.IsNullOrEmpty(overrideTitle) == false) ? overrideTitle : Title), style);

            bool dirty = isDirty;
            isDirty = false;

            return dirty;
        }

        /// <summary>
        /// On inspector GUI.
        /// </summary>
        public void InspectorGUI()
        {
            serializedObject.Update();

            // Print out some helper info for the nodes script.
            MonoScript monoScript = MonoScript.FromScriptableObject(this);
            string assetPath = AssetDatabase.GetAssetPath(monoScript);

            GUILayout.Label(assetPath);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Open Script in Editor") == true)
                {
                    AssetDatabase.OpenAsset(monoScript);
                }

                if (GUILayout.Button("Ping Script") == true)
                {
                    EditorGUIUtility.PingObject(monoScript);
                }

                if (GUILayout.Button("Ping Node") == true)
                {
                    EditorGUIUtility.PingObject(this);
                }
            }
            GUILayout.EndHorizontal();

            OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        public enum ActionStartupState
        {
            Enabled,
            Disabled
        }

        /// <summary>
        /// Inspector GUI callback
        /// </summary>
        virtual protected void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), new GUIContent("Designer Tag"));

                EditorGUILayout.LabelField("In Nodes: " + In.Count + " Out Nodes: " + Out.Count);

                var enabledProperty = serializedObject.FindProperty("enabled");

                GUI.backgroundColor = enabledProperty.boolValue ? (Color.green * 0.75f) : (Color.red * 0.75f);

                var startupState = (ActionStartupState)EditorGUILayout.EnumPopup("Default State", enabledProperty.boolValue ? ActionStartupState.Enabled : ActionStartupState.Disabled);

                enabledProperty.boolValue = (startupState == ActionStartupState.Enabled);

                GUI.backgroundColor = Color.white;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startUp"), new GUIContent("Start Up Action"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForAllInputs"), new GUIContent("Wait for All Nodes"));

                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Properties", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
        }
#endif
    }
}
