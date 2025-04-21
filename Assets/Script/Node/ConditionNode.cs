using UnityEditor;
using UnityEngine;

namespace BT
{
	public class ConditionNode : Node
    {
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
			Vector2 center = rect.center;
			Vector2 top = new Vector2(center.x, rect.yMin);
			Vector2 bottom = new Vector2(center.x, rect.yMax);
			Vector2 left = new Vector2(rect.xMin, center.y);
			Vector2 right = new Vector2(rect.xMax, center.y);

			Handles.BeginGUI();

			// 다이아몬드 배경
			Handles.color = new Color(0.2f, 0.4f, 0.9f, 0.6f);
			Handles.DrawAAConvexPolygon(top, right, bottom, left);

			// 테두리
			Handles.color = Color.black;
			Handles.DrawLine(top, right);
			Handles.DrawLine(right, bottom);
			Handles.DrawLine(bottom, left);
			Handles.DrawLine(left, top);

			Handles.EndGUI();

			// 텍스트			
			GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				normal = { textColor = Color.white },
				fontSize = 12,
				wordWrap = true
			};
			GUI.Label(rect, $"{id}\n{nodeType}\n{"hp < 0"}", labelStyle);

			// 드래그 처리
			var e = Event.current;
			if(e.type == EventType.MouseDrag && rect.Contains(e.mousePosition))
			{
				SetRect(new Rect(rect.position + e.delta, rect.size));
				Event.current.Use();
			}
		}
	}
}