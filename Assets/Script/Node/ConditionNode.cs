using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace BT
{
    public class ConditionNode : Node
    {
        private Func<bool> _condition;
        public string MethodName { get; set; }

        public ConditionNode()
        {
            _typeName = nameof(ConditionNode);
            _nodeType = BTType.CONDITION;
        }

        public override void Bind(AI ai)
        {
            base.Bind(ai);
            _condition = null;
            if (ai == null) return;
            if (string.IsNullOrWhiteSpace(MethodName))
            {
                if (GetType() == typeof(ConditionNode))
                    throw new InvalidOperationException("Condition method is missing.");
                return;
            }
            var method = ai.GetType().GetMethod(MethodName,
                BindingFlags.Instance | BindingFlags.Public, null,
                Type.EmptyTypes, null);
            if (method == null || method.ReturnType != typeof(bool))
                throw new InvalidOperationException($"Invalid condition method: {MethodName}");
            _condition = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), ai, method);
        }

        public virtual bool CheckCondition() => _condition != null && _condition();

        public override BtState GetState()
        {
            if (!CheckCondition())
            {
                Reset();
                return BtState.FAILUER;
            }
            foreach (var child in _childs)
            {
                var state = child.Tick();
                if (state == BtState.FAILUER) Reset();
                if (state != BtState.SUCCESS) return state;
            }
            return BtState.SUCCESS;
        }

#if UNITY_EDITOR
        public override void DrawDescription()
        {
            var drawn = GUI.Window(id, rect, windowId =>
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Condition");
                MethodName = EditorGUILayout.TextField("Method", MethodName ?? "");
                EditorGUILayout.EndVertical();
                GUI.DragWindow();
            }, id.ToString());
            SetRect(drawn.x, drawn.y);
        }
#endif
    }
}