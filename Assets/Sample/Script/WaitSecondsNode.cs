using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BT.Sample
{
    [Serializable]
    public class WaitSecondsNode : Node
    {
        [SerializeField, Min(0f)] private float _seconds = 1f;
        [NonSerialized] private float _elapsed;

        public override BtState GetState()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < _seconds) return BtState.RUNNING;
            Reset();
            return BtState.SUCCESS;
        }

        public override void Reset()
        {
            _elapsed = 0f;
            base.Reset();
        }

#if UNITY_EDITOR
        public override void DrawDescription()
        {
            var drawn = GUI.Window(id, rect, _ =>
            {
                EditorGUILayout.LabelField("WAIT", EditorStyles.boldLabel);
                _seconds = EditorGUILayout.FloatField("Seconds", _seconds);
                GUI.DragWindow(new Rect(0f, 0f, rect.width, 22f));
            }, id.ToString());
            SetRect(drawn.x, drawn.y);
        }
#endif
    }
}
