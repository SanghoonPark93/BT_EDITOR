using UnityEngine;

namespace Core
{
	public class Singleton<Type> where Type : new()
	{
		protected static Type _instance;

		public static Type Instance
		{
			get
			{
				if(_instance == null)
					_instance = new Type();

				return _instance;
			}
		}
	}

	public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T _instance;

		protected static void CreateInstance(string resourceName)
		{
			_instance = GameObject.Find(resourceName).GetComponent<T>();

			if(_instance != null)
				return;

			var res = Resources.Load<T>(resourceName);

			var obj		= (res == null) ? new GameObject(resourceName): Instantiate(res).gameObject;
			_instance	= (res == null) ? obj.AddComponent<T>() : obj.GetComponent<T>();
		}
	}
}