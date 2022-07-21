#if !SQUAX_RELEASE_MODE

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;
using System.Reflection;

namespace Squax.Actions
{
    /// <summary>
    /// Action Container Editor.
    /// </summary>
    public class ActionContainerGraphEditor : EditorWindow
    {
        const float WINDOW_PADDING = 50f;

        /// <summary>
        /// Serialized Object.
        /// </summary>
        private SerializedObject serializedObject;

        /// <summary>
        /// List of actions.
        /// </summary>
        private List<SerializedObject> actions = new List<SerializedObject>();

        /// <summary>
        /// Currently selected action.
        /// </summary>
        private Action selectedAction = null;

        private bool isDirty = true;

        /// <summary>
        /// Window position.
        /// </summary>
        private Dictionary<Object, Rect> windowLookup = new Dictionary<Object, Rect>();

        /// <summary>
        /// Scroll position.
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// Selected tab.
        /// </summary>
        private int selectedTab = 0;

        /// <summary>
        /// Highlight action.
        /// </summary>
        private Action highlightAction;

        Vector2 maxWindowPosition;
        Vector2 minWindowPosition;

        private float accumTime = 0;
        private bool displayTitle = true;

        private bool hideSideBar = false;

        private GUIStyle smallLabelStyle;

        private bool startDrag = false;

        private Vector2 startMousePosition;

        private Vector2 contentScrollPosition;

        /// <summary>
        /// Open new window.
        /// </summary>
        /// <param name="serializedObject"></param>
        static public void Open(SerializedObject serializedObject, Action highlightAction = null)
        {
            var window = GetWindow<ActionContainerGraphEditor>(false, "Action Editor", true);

            window.Setup(serializedObject, highlightAction);

            var title = AssetDatabase.GetAssetPath(serializedObject.targetObject);
            window.titleContent = new GUIContent("Action Editor: " + ((string.IsNullOrEmpty(title) == false) ? title : ""), "");
        }

        /// <summary>
        /// Open new window by path.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="highlightAction"></param>
        static public void Open(string assetPath, Action highlightAction = null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach(var asset in assets)
            {
                if(asset is ActionContainer)
                {
                    Open(new SerializedObject(asset), highlightAction);

                    return;
                }
            }
        }

        void BuildList(Action node, List<Action> list)
        {
            if(list.Contains(node) == true)
            {
                return;
            }

            list.Add(node);

            foreach(var child in node.Out)
            {
                BuildList(child, list);
            }
        }

        public void Setup(SerializedObject serializedObject, Action highlightAction)
        {
            smallLabelStyle = new UnityEngine.GUIStyle(GUI.skin.button);
            smallLabelStyle.alignment = UnityEngine.TextAnchor.MiddleCenter;
            smallLabelStyle.fontStyle = FontStyle.Bold;
            smallLabelStyle.fontSize = 10;

            this.serializedObject = serializedObject;
            this.highlightAction = highlightAction;

            var actionContainer = serializedObject.targetObject as ActionContainer;

            if(highlightAction != null)
            {
                Action.ClearSelection();
                Action.AddToSelection(highlightAction);

                scrollPosition = new Vector2(highlightAction.size.x - (position.width * 0.5f), highlightAction.size.y - (position.height * 0.5f));
            }

            Action.AssetPath = AssetDatabase.GetAssetPath(serializedObject.targetObject);

            isDirty = true;
        }

        bool IsCulledByWindow(Vector2 scrollPosition, Rect size)
        {
            Rect bounds = new Rect(scrollPosition.x - size.width, scrollPosition.y - size.height, scrollPosition.x + position.width + size.width, scrollPosition.y + position.height + size.height);

            return bounds.Contains(new Vector2(size.x, size.y)) == false;
        }

        int selectedLayer = 0;
        int selectedLayerID = 0;

        private Rect windowSize;

        public void OnGUI()
        {
            if (serializedObject == null)
            {
                return;
            }

            serializedObject.Update();

            var actionContainer = serializedObject.targetObject as ActionContainer;

            // Do cleanup.
            foreach (var action in actions)
            {
                var instance = action.targetObject as Action;

                if (instance != null && instance.IsDelete == true)
                {
                    List<Action> copy = new List<Action>(instance.Out);
                    foreach (var node in copy)
                    {
                        instance.Unlink(node);
                    }

                    copy = new List<Action>(instance.In);
                    foreach (var node in copy)
                    {
                        instance.Unlink(node);
                    }

                    DestroyImmediate(instance, true);
                }
            }

            if (isDirty == true)
            {
                ReloadActionLibrary();
            }

            if (Action.HasSelection() && selectedAction != Action.FirstSelection())
            {
                selectedAction = Action.FirstSelection();

                GUI.FocusControl("reset");
            }

            float sidebarSize = 450f;

            if (hideSideBar == true)
            {
                sidebarSize = 0f;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    bool allowDraggingWindow = true;

                    // Middle panning.
                    if (UnityEngine.Event.current.type == EventType.MouseDrag && UnityEngine.Event.current.button == 2)
                    {
                        allowDraggingWindow = false;

                        scrollPosition = new Vector2(scrollPosition.x - UnityEngine.Event.current.delta.x, scrollPosition.y - UnityEngine.Event.current.delta.y);

                        UnityEngine.Event.current.Use();
                    }
                    else if (UnityEngine.Event.current.type == EventType.MouseDrag && UnityEngine.Event.current.button == 0 && UnityEngine.Event.current.alt == true)
                    {
                        if(Action.HasSelection())
                        {
                            allowDraggingWindow = false;

                            // Trace all outs.
                            var list = new List<Action>();

                            BuildList(Action.FirstSelection(), list);

                            foreach(var node in list)
                            {
                                node.size.x += UnityEngine.Event.current.delta.x;
                                node.size.y += UnityEngine.Event.current.delta.y;
                            }

                            UnityEngine.Event.current.Use();
                        }
                    }
                    else if (UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 1 && UnityEngine.Event.current.shift == true)
                    {
                        if (Action.HasSelection())
                        {
                            var mousePosition = UnityEngine.Event.current.mousePosition;

                            var spawnPosition = minWindowPosition + scrollPosition + mousePosition;

                            var action = Action.FirstSelection();
                            Action.ClearSelection();
                            Action.AddToSelection(Action.PasteAction(action, spawnPosition));

                            UnityEngine.Event.current.Use();

                            isDirty = true;
                        }
                    }
                    else if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0 && UnityEngine.Event.current.control == true)
                    {
                        startDrag = true;

                        var mousePosition = UnityEngine.Event.current.mousePosition;

                        startMousePosition = minWindowPosition + scrollPosition + mousePosition;
                    }
                    else if (UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 0 && UnityEngine.Event.current.control == true)
                    {
                        if(startDrag == true)
                        {
                            var mousePosition = UnityEngine.Event.current.mousePosition;

                            var endMousePosition = minWindowPosition + scrollPosition + mousePosition;

                            var delta = endMousePosition - startMousePosition;

                            float x = (endMousePosition.x < startMousePosition.x) ? endMousePosition.x : startMousePosition.x;
                            float y = (endMousePosition.y < startMousePosition.y) ? endMousePosition.y : startMousePosition.y;

                            // Select things within the box.
                            var bounds = new Rect(x, y, Mathf.Abs(delta.x), Mathf.Abs(delta.y));

                            Action.ClearSelection();

                            foreach (var action in actions)
                            {
                                var instance = action.targetObject as Action;

                                if (instance != null && bounds.Contains(new Vector2(instance.size.x, instance.size.y)))
                                {
                                    Action.AddToSelection(instance);
                                }
                            }
                        }

                        startDrag = false;
                    }

                    float topBorder = 0;
                    float sidePadding = 5f;

                    var skin = UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);

                    var bgStyle = skin.customStyles.First(s => s.name == "flow background");

                    GUI.Box(new Rect(0, topBorder, position.width - sidebarSize - sidePadding, position.height - topBorder), "", bgStyle);

                    windowSize = new Rect(0, topBorder, position.width - sidebarSize - sidePadding, position.height - topBorder);

                    scrollPosition = GUI.BeginScrollView(windowSize, scrollPosition,
                        new Rect(minWindowPosition.x, minWindowPosition.y, maxWindowPosition.x, maxWindowPosition.y));
                    {
                        maxWindowPosition = Vector2.zero;
                        minWindowPosition = Vector2.zero;

                        int totalCulled = 0;

                        BeginWindows();
                        {
                            int index = 0;
                            foreach (var action in actions)
                            {
                                var instance = action.targetObject as Action;

                                if (instance != null)
                                {
                                    if (instance.size.x < 0)
                                    {
                                        instance.size.x = 0;
                                    }

                                    if (instance.size.y < 0)
                                    {
                                        instance.size.y = 0;
                                    }

                                    maxWindowPosition.x = Mathf.Max(maxWindowPosition.x, instance.size.x + instance.Dimensions.x * 2f + WINDOW_PADDING);
                                    maxWindowPosition.y = Mathf.Max(maxWindowPosition.y, instance.size.y + instance.Dimensions.y * 2f + WINDOW_PADDING);

                                    minWindowPosition.x = Mathf.Min(0, instance.size.x - WINDOW_PADDING);
                                    minWindowPosition.y = Mathf.Min(0, instance.size.y - WINDOW_PADDING);

                                    if (IsCulledByWindow(scrollPosition, instance.size))
                                    {
                                        ++totalCulled;
                                        continue;
                                    }

                                    if (instance.IsVisible == false)
                                    {
                                        continue;
                                    }

                                    GUI.color = Color.white * 0.9f;

                                    if(Application.isPlaying == true)
                                    {
                                        switch(instance.CurrentState)
                                        {
                                            case Action.State.Idle:
                                                break;

                                            case Action.State.EndNextFrame:
                                                GUI.color = Color.yellow * 0.9f;
                                                break;

                                            case Action.State.Running:
                                                GUI.color = Color.green * 0.9f;
                                                break;

                                            case Action.State.Disabled:
                                                GUI.color = Color.red * 0.9f;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        if (Action.ContainsSelection(instance))
                                        {
                                            GUI.SetNextControlName("reset");
                                            GUI.color = Color.white * 0.9f;
                                        }
                                        else
                                        {
                                            if (instance.IsDefaultEnabled == false)
                                            {
                                                GUI.color = Color.red * 0.75f;
                                            }
                                            else
                                            {
                                                if (instance.IsStartupAction == true)
                                                {
                                                    GUI.color = Color.yellow;
                                                }
                                                else
                                                {
                                                    GUI.color = Color.white * 0.75f;
                                                }
                                            }
                                        }
                                    }

                                    isDirty |= instance.DrawWindow(index, displayTitle ? instance.ID : null, allowDraggingWindow);

                                    // Draw curves.
                                    int counter = 0;
                                    foreach(var node in instance.Out)
                                    {
                                        if (Action.HighlightOutAction == node && Action.ContainsSelection(instance))
                                        {
                                            if(DrawCurve(instance.size, node.size, new Color(0.3f, 0.5f, 1f), counter, instance.Out.Count, true, instance, node) == true)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (DrawCurve(instance.size, node.size, (Action.ContainsSelection(instance)) ? new Color(1f, 0.6f, 0f) : Color.white, counter, instance.Out.Count, (Action.ContainsSelection(instance)), instance, node) == true)
                                            {
                                                break;
                                            }
                                        }

                                        ++counter;
                                    }

                                    GUI.color = Color.white;
                                }

                                ++index;
                            }
                        }
                        EndWindows();

                        if (startDrag == true)
                        {
                            var mousePosition = UnityEngine.Event.current.mousePosition;

                            var endMousePosition = mousePosition;

                            var delta = endMousePosition - startMousePosition;

                            float x = (endMousePosition.x < startMousePosition.x) ? endMousePosition.x : startMousePosition.x;
                            float y = (endMousePosition.y < startMousePosition.y) ? endMousePosition.y : startMousePosition.y;

                            // Select things within the box.
                            var bounds = new Rect(x, y, Mathf.Abs(delta.x), Mathf.Abs(delta.y));

                            GUI.Box(bounds, GUIContent.none);
                        }
                    }
                    GUI.EndScrollView();
                }
                GUILayout.EndVertical();

                if (hideSideBar == true)
                {
                    if(GUILayout.Button("Show") == true)
                    {
                        selectedTab = 0;
                        hideSideBar = false;
                    }
                }
                else
                {
                    GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(sidebarSize), GUILayout.ExpandHeight(true));
                    {
                        if (GUILayout.Button("Hide") == true)
                        {
                            selectedTab = 0;
                            hideSideBar = true;
                        }

                        contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition, false, false, GUILayout.ExpandHeight(true));
                        {
                            if (Action.HasSelection())
                            {
                                Action.FirstSelection().InspectorGUI();
                            }
                        }
                        GUILayout.EndScrollView();

                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();

            // Right mouse click
            if (UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 1)
            {
                var mousePosition = UnityEngine.Event.current.mousePosition;

                var spawnPosition = minWindowPosition + scrollPosition + mousePosition;

                var menuList = new List<Action.RightClickMenuItem>();

                // Use reflection to get all classes tagged with action attribute.
                foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (System.Type type in assembly.GetTypes())
                    {
                        var attribs = type.GetCustomAttributes(typeof(ActionAttribute), false);
                        if (attribs != null && attribs.Length > 0)
                        {
                            ActionAttribute actionAttribute = attribs[0] as ActionAttribute;

                            if(actionAttribute != null)
                            {
                                menuList.Add(
                                            new Action.RightClickMenuItem(actionAttribute.Path, (object userData) =>
                                            {
                                                Action.ClearSelection();
                                                Action.AddToSelection(Action.AddAction(spawnPosition, type));

                                                isDirty = true;
                                            }));
                            }
                        }
                    }
                }
                if (Action.CopyAction != null)
                {
                    menuList.Add(
                                new Action.RightClickMenuItem("Paste Action", (object userData) =>
                                {
                                    Action.ClearSelection();
                                    Action.AddToSelection(Action.PasteAction(Action.CopyAction, spawnPosition));

                                    isDirty = true;
                                }));
                }


                DisplayMenu(menuList.ToArray());

                UnityEngine.Event.current.Use();
            }

            autoRepaintOnSceneChange = true;

            var tooltip = GUI.tooltip;

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        static Texture2D up = null;
        static Texture2D right = null;
        static Texture2D down = null;
        static Texture2D left = null;

        static Texture2D trigger = null;

        bool DrawCurve(Rect start, Rect end, Color color, int index, int inputCounter, bool drawLinkButton, Action currentAction, Action outAction)
        {
            float inOffset = -10f;
            float outOffset = 10f;

            float lineWidth = 3f;

            bool modified = false;

            Vector2 delta = (new Vector2(end.x + end.width * 0.5f, end.y + end.height * 0.5f) - new Vector2(start.x + start.width * 0.5f, start.y + start.height * 0.5f));

            float distance = delta.magnitude;

            bool useStraightLine = distance < 4f;

            Vector2 direction = delta.normalized;

            float angle = 180f + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (down == null)
            {
                down = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Gizmos/GlyphIcons/glyphicons-213-down-arrow.png", typeof(Texture2D)) as Texture2D;
            }

            if (right == null)
            {
                right = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Gizmos/GlyphIcons/glyphicons-212-right-arrow.png", typeof(Texture2D)) as Texture2D;
            }

            if (up == null)
            {
                up = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Gizmos/GlyphIcons/glyphicons-214-up-arrow.png", typeof(Texture2D)) as Texture2D;
            }

            if (left == null)
            {
                left = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Gizmos/GlyphIcons/glyphicons-211-left-arrow.png", typeof(Texture2D)) as Texture2D;
            }

            var handleSize = 30f;

            if(angle > 135 && angle <= 225)
            {
                // Right.
                var startPos = new Vector3(start.x + start.width, start.y + start.height * 0.5f + inOffset, 0f);

                var endPos = new Vector3(end.x, end.y + end.height * 0.5f - outOffset, 0f);

                var startTan = startPos + Vector3.right * handleSize;

                var endTan = endPos + Vector3.left * handleSize;

                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, lineWidth);

                GUI.color = color;

                if (drawLinkButton)
                {
                    modified = DrawButton(index, new Vector2(startPos.x, startPos.y), new Vector2(endPos.x, endPos.y), currentAction, outAction);
                }

                Rect arrow = end;

                arrow.width = 16f;
                arrow.height = 16f;
                arrow.x -= 8f;
                arrow.y -= 8f;

                arrow.y += end.height * 0.5f - outOffset;

                GUI.Label(arrow, right);
            }
            else if (angle > 225 && angle <= 315)
            {
                // Down.
                var startPos = new Vector3(start.x + start.width * 0.5f - inOffset, start.y + start.height, 0f);

                var endPos = new Vector3(end.x + end.width * 0.5f + outOffset, end.y, 0f);

                var startTan = startPos + Vector3.up * handleSize;

                var endTan = endPos + Vector3.down * handleSize;

                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, lineWidth);

                GUI.color = color;

                if (drawLinkButton)
                {
                    modified = DrawButton(index, new Vector2(startPos.x, startPos.y), new Vector2(endPos.x, endPos.y), currentAction, outAction);
                }

                Rect arrow = end;

                arrow.width = 16f;
                arrow.height = 16f;
                arrow.x -= 8f;
                arrow.y -= 8f;

                arrow.x += end.width * 0.5f + outOffset;

                GUI.Label(arrow, down);
            }
            else if (angle > 45 && angle <= 135f)
            {
                // Up.
                var startPos = new Vector3(start.x + start.width * 0.5f + inOffset, start.y, 0f);

                var endPos = new Vector3(end.x + end.width * 0.5f - outOffset, end.y + end.height, 0f);

                var startTan = startPos + Vector3.down * handleSize;

                var endTan = endPos + Vector3.up * handleSize;

                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, lineWidth);

                GUI.color = color;

                if (drawLinkButton)
                {
                    modified = DrawButton(index, new Vector2(startPos.x, startPos.y), new Vector2(endPos.x, endPos.y), currentAction, outAction);
                }

                Rect arrow = end;

                arrow.width = 16f;
                arrow.height = 16f;
                arrow.x -= 8f;
                arrow.y -= 8f;

                arrow.x += end.width * 0.5f - outOffset;
                arrow.y += end.height;

                GUI.Label(arrow, up);
            }
            else
            {
                // Left.
                var startPos = new Vector3(start.x, start.y + start.height * 0.5f - inOffset, 0f);

                var endPos = new Vector3(end.x + end.width, end.y + end.height * 0.5f + outOffset, 0f);

                var startTan = startPos + Vector3.left * handleSize;

                var endTan = endPos + Vector3.right * handleSize;

                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, lineWidth);

                GUI.color = color;

                if (drawLinkButton)
                {
                    modified = DrawButton(index, new Vector2(startPos.x, startPos.y), new Vector2(endPos.x, endPos.y), currentAction, outAction);
                }

                Rect arrow = end;

                arrow.width = 16f;
                arrow.height = 16f;
                arrow.x -= 8f;
                arrow.y -= 8f;

                arrow.y += end.height * 0.5f + outOffset;
                arrow.x += end.width;

                GUI.Label(arrow, left);
            }

            return modified;
        }

        bool DrawButton(int index, Vector2 start, Vector2 end, Action currentAction, Action outAction)
        {
            var midPoint = start + (end - start) * 0.5f;

            if (GUI.Button(new Rect(midPoint.x - 8, midPoint.y - 8, 24, 24), "" + (index + 1), EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Break Connection", "Do you wish to break the connection? This can not be undone.", "Ok, break", "Cancel"))
                {
                    outAction.Unlink(currentAction);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Display a context menu.
        /// </summary>
        /// <param name="options"></param>
        private void DisplayMenu(Action.RightClickMenuItem[] options)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var menuItem in options)
            {
                menu.AddItem(new GUIContent(menuItem.Text), false, menuItem.Function, 0);
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Reload the action library.
        /// </summary>
        private void ReloadActionLibrary()
        {
            var assetPath = AssetDatabase.GetAssetPath(serializedObject.targetObject);

            if (string.IsNullOrEmpty(assetPath) == true)
            {
                return;
            }

            selectedAction = null;

            actions.Clear();

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            var list = serializedObject.FindProperty("actions");

            list.ClearArray();

            var actionContainer = serializedObject.targetObject as ActionContainer;
            
            foreach(var asset in assets)
            {
                if(asset != serializedObject.targetObject)
                {
                    var actualAction = asset as Action;

                    if (actualAction == null)
                    {
                        continue;
                    }

                    list.InsertArrayElementAtIndex(list.arraySize);

                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = asset;

                    asset.name = actualAction.Title + " [" + actualAction.ID + "]";

                    var action = new SerializedObject(asset);

                    actions.Add(action);
                }
            }

            isDirty = false;
        }
    }
}

#endif
