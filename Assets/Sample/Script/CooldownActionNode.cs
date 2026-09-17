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
            GUI.Box(rect, id.ToString());
            GUI.Label(new Rect(rect.x + 5f, rect.y + 22f, rect.width - 10f, 20f),
                "COOLDOWN ACTION", EditorStyles.boldLabel);
            MethodName = EditorGUI.TextField(new Rect(rect.x + 5f, rect.y + 44f,
                rect.width - 10f, 20f), MethodName ?? string.Empty);
            GUI.Label(new Rect(rect.x + 5f, rect.y + 68f, 32f, 20f), "Sec");
            _cooldownSeconds = Mathf.Max(0f, EditorGUI.FloatField(
                new Rect(rect.x + 40f, rect.y + 68f, rect.width - 45f, 20f),
                _cooldownSeconds));
        }
#endif
    }
}
