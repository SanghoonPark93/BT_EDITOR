#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace BT
{
    public class SequenceNode : Node
    {
        private int _runningChild;

        public SequenceNode()
        {
            _typeName = nameof(SequenceNode);
            _nodeType = BTType.SEQUENCE;
        }

        public override BtState GetState()
        {
            for (var i = _runningChild; i < _childs.Count; i++)
            {
                var state = _childs[i].Tick();
                if (state == BtState.RUNNING)
                {
                    _runningChild = i;
                    return state;
                }
                if (state == BtState.FAILUER)
                {
                    Reset();
                    return state;
                }
            }
            Reset();
            return BtState.SUCCESS;
        }

        public override void Reset()
        {
            _runningChild = 0;
            base.Reset();
        }
#if UNITY_EDITOR
        protected override float width => 160f;
        protected override float height => 76f;

        public override void DrawBackground()
        {
            var r = rect;
            var inset = 18f;
            var points = new[]
            {
                new Vector3(r.x + inset, r.y + 1),
                new Vector3(r.xMax - inset, r.y + 1),
                new Vector3(r.xMax - 1, r.center.y),
                new Vector3(r.xMax - inset, r.yMax - 1),
                new Vector3(r.x + inset, r.yMax - 1),
                new Vector3(r.x + 1, r.center.y)
            };
            Handles.color = new Color(0.16f, 0.34f, 0.26f, 1f);
            Handles.DrawAAConvexPolygon(points);
            Handles.color = new Color(0.47f, 0.83f, 0.58f, 1f);
            Handles.DrawAAPolyLine(2f, new[] { points[0], points[1], points[2], points[3], points[4], points[5], points[0] });
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
                GUI.Label(new Rect(12f, 10f, rect.width - 24f, 26f), "SEQUENCE", label);
                GUI.Label(new Rect(12f, 36f, rect.width - 24f, 22f), "1 → 2 → 3", label);
                GUI.DragWindow();
            }, GUIContent.none, GUIStyle.none);
            SetRect(drawn.x, drawn.y);
        }
#endif
    }
}