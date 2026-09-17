#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace BT
{
    public class SelectorNode : Node
    {
        private Node _runningChild;

        public SelectorNode()
        {
            _typeName = nameof(SelectorNode);
            _nodeType = BTType.SELECTOR;
        }

        public override BtState GetState()
        {
            foreach (var child in _childs)
            {
                var state = child.Tick();
                if (state == BtState.FAILUER) continue;
                if (state == BtState.RUNNING)
                {
                    if (_runningChild != null && _runningChild != child)
                        _runningChild.Reset();
                    _runningChild = child;
                    return state;
                }
                Reset();
                return state;
            }
            Reset();
            return BtState.FAILUER;
        }

        public override void Reset()
        {
            _runningChild = null;
            base.Reset();
        }
#if UNITY_EDITOR
        protected override float width => 150f;
        protected override float height => 90f;

        public override void DrawBackground()
        {
            var r = rect;
            var points = new[]
            {
                new Vector3(r.center.x, r.y + 1),
                new Vector3(r.xMax - 1, r.center.y),
                new Vector3(r.center.x, r.yMax - 1),
                new Vector3(r.x + 1, r.center.y)
            };
            Handles.color = new Color(0.18f, 0.29f, 0.45f, 1f);
            Handles.DrawAAConvexPolygon(points);
            Handles.color = new Color(0.45f, 0.72f, 1f, 1f);
            Handles.DrawAAPolyLine(2f, new[] { points[0], points[1], points[2], points[3], points[0] });
            Handles.color = Color.white;
        }

        public override void DrawDescription()
        {
            var drawn = GUI.Window(id, rect, windowId =>
            {
                var label = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                GUI.Label(new Rect(20f, 22f, rect.width - 40f, 26f), "SELECTOR", label);
                GUI.Label(new Rect(25f, 46f, rect.width - 50f, 22f), "1 ? 2 ? 3", label);
                GUI.DragWindow();
            }, GUIContent.none, GUIStyle.none);
            SetRect(drawn.x, drawn.y);
        }
#endif
    }
}