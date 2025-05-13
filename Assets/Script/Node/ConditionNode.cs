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

			// 다이아몬드 배경 및 테두리
			Handles.BeginGUI();

			Handles.color = new Color(0.2f, 0.4f, 0.9f, 0.6f);
			Handles.DrawAAConvexPolygon(top, right, bottom, left);

			Handles.color = Color.black;
			Handles.DrawLine(top, right);
			Handles.DrawLine(right, bottom);
			Handles.DrawLine(bottom, left);
			Handles.DrawLine(left, top);

			Handles.EndGUI();

			// 텍스트 스타일
			var labelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				normal = { textColor = Color.white },
				fontSize = 12,
				wordWrap = true
			};

			// ID 라벨 (다이아몬드 상단 ⅓)
			var sectionHeight = rect.height / 3f;
			var idRect = new Rect(rect.x, rect.y, rect.width, sectionHeight);
			GUI.Label(idRect, $"ID: {id}", labelStyle);

			// ==== 인풋 필드 위치 계산 ====
			var inputY = rect.y + sectionHeight * 2;
			var centerY = rect.center.y;
			var centerX = rect.center.x;

			// 다이아몬드 크기 반
			var diamondHalfWidth = rect.width / 2f;
			var diamondHalfHeight = rect.height / 2f;

			// 현재 위치에서 허용 가능한 최대 폭 계산
			var relativeY = Mathf.Abs((inputY + sectionHeight / 2f) - centerY);
			var ratio = Mathf.Clamp01(1f - (relativeY / diamondHalfHeight));
			var maxHalfWidth = diamondHalfWidth * ratio;

			// 입력 필드 위치 설정 (다이아몬드 내부에 꼭 맞도록)
			var inputWidth = Mathf.Max(20f, maxHalfWidth * 2f);
			var inputHeight = sectionHeight - 6f;
			var inputRect = new Rect(centerX - 50f, inputY - 30f, 100f, inputHeight);

			// 인풋 필드
			conditionDesc = GUI.TextField(inputRect, conditionDesc);

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