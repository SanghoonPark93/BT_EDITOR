using UnityEditor;
using UnityEngine;

namespace BT
{
	public class ConditionNode : Node
    {
		public string conditionDesc;

		public ConditionNode()
		{
			nodeType = BTType.CONDITION;
		}

		public virtual bool CheckCondition() 
		{
			return true;
		}

		public override BtState GetState()
		{			
			return CheckCondition() ? BtState.SUCCESS : BtState.FAILUER;
		}

		public override void DrawDescription()
		{
			var center = rect.center;
			var top = new Vector2(center.x, rect.yMin);
			var bottom = new Vector2(center.x, rect.yMax);
			var left = new Vector2(rect.xMin, center.y);
			var right = new Vector2(rect.xMax, center.y);

			// ���̾Ƹ�� ��� �� �׵θ�
			Handles.BeginGUI();

			Handles.color = new Color(0.2f, 0.4f, 0.9f, 0.6f);
			Handles.DrawAAConvexPolygon(top, right, bottom, left);

			Handles.color = Color.black;
			Handles.DrawLine(top, right);
			Handles.DrawLine(right, bottom);
			Handles.DrawLine(bottom, left);
			Handles.DrawLine(left, top);

			Handles.EndGUI();

			// �ؽ�Ʈ ��Ÿ��
			var labelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				normal = { textColor = Color.white },
				fontSize = 12,
				wordWrap = true
			};

			// ID �� (���̾Ƹ�� ��� ��)
			var sectionHeight = rect.height / 3f;
			var idRect = new Rect(rect.x, rect.y, rect.width, sectionHeight);
			GUI.Label(idRect, $"ID: {id}", labelStyle);

			// ==== ��ǲ �ʵ� ��ġ ��� ====
			var inputY = rect.y + sectionHeight * 2;
			var centerY = rect.center.y;
			var centerX = rect.center.x;

			// ���̾Ƹ�� ũ�� ��
			var diamondHalfWidth = rect.width / 2f;
			var diamondHalfHeight = rect.height / 2f;

			// ���� ��ġ���� ��� ������ �ִ� �� ���
			var relativeY = Mathf.Abs((inputY + sectionHeight / 2f) - centerY);
			var ratio = Mathf.Clamp01(1f - (relativeY / diamondHalfHeight));
			var maxHalfWidth = diamondHalfWidth * ratio;

			// �Է� �ʵ� ��ġ ���� (���̾Ƹ�� ���ο� �� �µ���)
			var inputWidth = Mathf.Max(20f, maxHalfWidth * 2f);
			var inputHeight = sectionHeight - 6f;
			var inputRect = new Rect(centerX - 50f, inputY - 30f, 100f, inputHeight);

			// ��ǲ �ʵ�
			conditionDesc = GUI.TextField(inputRect, conditionDesc);

			// �巡�� ó��
			var e = Event.current;
			if(e.type == EventType.MouseDrag && rect.Contains(e.mousePosition))
			{
				SetRect(new Rect(rect.position + e.delta, rect.size));
				Event.current.Use();
			}
		}
	}
}