using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace BT
{
    public class ActionNode : Node
    {
        private Func<bool, BtState> _action;
        private bool _isFirstTurn = true;
        private int _runningChild;

        public string MethodName { get; set; }

        public ActionNode()
        {
            _typeName = nameof(ActionNode);
            _nodeType = BTType.NONE;
        }

        public override void Bind(AI ai)
        {
            base.Bind(ai);
            _action = null;
            _isFirstTurn = true;
            if (ai == null) return;
            if (string.IsNullOrWhiteSpace(MethodName))
            {
                if (GetType() == typeof(ActionNode))
                    throw new InvalidOperationException("Action method is missing.");
                return;
            }
            var method = ai.GetType().GetMethod(MethodName,
                BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(bool) }, null);
            if (method == null || method.ReturnType != typeof(BtState))
                throw new InvalidOperationException($"Invalid action method: {MethodName}");
            _action = (Func<bool, BtState>)Delegate.CreateDelegate(
                typeof(Func<bool, BtState>), ai, method);
        }

        public override BtState GetState()
        {
            if (_action == null)
            {
                Reset();
                return BtState.FAILUER;
            }
            var state = _action(_isFirstTurn);
            _isFirstTurn = false;
            if (state == BtState.FAILUER)
            {
                Reset();
                return state;
            }
            for (var i = _runningChild; i < _childs.Count; i++)
            {
                var childState = _childs[i].Tick();
                if (childState == BtState.RUNNING)
                {
                    _runningChild = i;
                    return childState;
                }
                if (childState == BtState.FAILUER)
                {
                    Reset();
                    return childState;
                }
            }
            if (state == BtState.SUCCESS) Reset();
            return state;
        }

        public override void Reset()
        {
            _isFirstTurn = true;
            _runningChild = 0;
            base.Reset();
        }

#if UNITY_EDITOR
        public override void DrawDescription()
        {
            var drawn = GUI.Window(id, rect, windowId =>
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("ACTION", EditorStyles.boldLabel);
                MethodName = EditorGUILayout.TextField(MethodName ?? string.Empty);
                EditorGUILayout.EndVertical();
                GUI.DragWindow(new Rect(0f, 0f, rect.width, 24f));
            }, id.ToString());
            SetRect(drawn.x, drawn.y);
        }
#endif
    }
}