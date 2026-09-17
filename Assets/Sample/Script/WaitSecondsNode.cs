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
            GUI.Box(rect, id.ToString());
            GUI.Label(new Rect(rect.x + 5f, rect.y + 22f, rect.width - 10f, 20f),
                "WAIT", EditorStyles.boldLabel);
            GUI.Label(new Rect(rect.x + 5f, rect.y + 44f, 58f, 20f), "Seconds");
            _seconds = Mathf.Max(0f, EditorGUI.FloatField(
                new Rect(rect.x + 65f, rect.y + 44f, rect.width - 70f, 20f),
                _seconds));
        }
#endif
    }
}
