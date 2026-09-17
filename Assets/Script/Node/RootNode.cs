#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace BT
{
    public class RootNode : SelectorNode
    {
#if UNITY_EDITOR
        protected override float width => 100f;
        protected override float height => 50f;

        public override void DrawBackground()
        {
            EditorGUI.DrawRect(rect, new Color(0.34f, 0.24f, 0.44f, 1f));
            var r = rect;
            Handles.color = new Color(0.78f, 0.58f, 0.94f, 1f);
            Handles.DrawAAPolyLine(2f, new[]
            {
                new Vector3(r.x, r.y), new Vector3(r.xMax, r.y),
                new Vector3(r.xMax, r.yMax), new Vector3(r.x, r.yMax),
                new Vector3(r.x, r.y)
            });
            Handles.color = Color.white;
        }

        public override void DrawDescription()
        {
            var label = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            GUI.Label(rect, "ROOT", label);
        }
#endif
        public RootNode()
        {
            _typeName = nameof(RootNode);
            _nodeType = BTType.ROOT;
        }
    }
}
