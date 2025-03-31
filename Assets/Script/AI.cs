using BT.Util;
using UnityEngine;

namespace BT
{
    public abstract class AI : MonoBehaviour
	{
		protected RootNode _btRoot;
		protected BtState _stateCache;

		/// <summary>
		/// 업데이트문을 멈추고싶을경우 true
		/// </summary>
		protected bool _isStop;

		/// <summary>
		/// 업데이트문에 딜레이를 주고싶을 경우 사용
		/// </summary>
		private float _delayUpdate = -1f;
		private float _delayDef;

		#region TestAction

		public abstract BtState HpCheck(bool isFirstTurn);

		public abstract BtState Death(bool isFirstTurn);

		public abstract BtState Hit(bool isFirstTurn);

		public abstract BtState Detector(bool isFirstTurn);

		public abstract BtState Attack(bool isFirstTurn);

		public abstract BtState Move(bool isFirstTurn);

		public abstract BtState Idle(bool isFirstTurn);

		#endregion

		public virtual void Initialize(string jsonName)
		{
			var controller = Utils.GetJson<NodeController>(jsonName);

			if (controller.Root != null)
			{
				_btRoot = new RootNode();
				_btRoot.SetData(controller, controller.Root, this);
			}
		}

		protected virtual void Update()
		{
			if (_isStop || _btRoot == null)
				return;

			if (_delayUpdate > -1)
			{
				_delayUpdate -= Time.deltaTime;

				if (_delayUpdate > 0)
					return;

				_delayUpdate = _delayDef;
			}

			UpdateBody();
		}

		protected virtual void UpdateBody()
		{
			_btRoot.GetState();
		}

		protected void SetDelayTime(float delay)
		{
			_delayUpdate = delay;
			_delayDef = delay;
		}
	}
}