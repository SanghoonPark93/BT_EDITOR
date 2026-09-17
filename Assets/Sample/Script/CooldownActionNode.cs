using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BT.Sample
{
    [Serializable]
    public class CooldownActionNode : ActionNode
    {
        [SerializeField, Min(0f)] private float _cooldownSeconds = 1f;
        [NonSerialized] private float _nextAllowedTime;

        public CooldownActionNode()
        {
            _typeName = nameof(CooldownActionNode);
        }

        public override void Bind(AI ai)
        {
            if (ai != null && string.IsNullOrWhiteSpace(MethodName))
                throw new InvalidOperationException("Cooldown action method is missing.");
            base.Bind(ai);
            _nextAllowedTime = 0f;
        }

        public override BtState GetState()
        {
            if (Time.time < _nextAllowedTime)
                return BtState.FAILUER;

            var state = base.GetState();
            if (state == BtState.SUCCESS)
                _nextAllowedTime = Time.time + _cooldownSeconds;
            return state;
        }

#if UNITY_EDITOR
        public override void DrawDescription()
        {
            var drawn = GUI.Window(id, rect, _ =>
            {
                GUI.Label(new Rect(8f, 7f, rect.width - 16f, 20f),
                    "COOLDOWN ACTION", EditorStyles.boldLabel);
                MethodName = EditorGUI.TextField(new Rect(8f, 31f, rect.width - 16f, 20f),
                    MethodName ?? string.Empty);
                GUI.Label(new Rect(8f, 58f, 30f, 20f), "Sec");
                _cooldownSeconds = Mathf.Max(0f, EditorGUI.FloatField(
                    new Rect(38f, 58f, rect.width - 46f, 20f), _cooldownSeconds));
                GUI.DragWindow(new Rect(0f, 0f, rect.width, 24f));
            }, id.ToString());
            SetRect(drawn.x, drawn.y);
        }
#endif
    }
}
