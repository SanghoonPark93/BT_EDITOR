using UnityEngine;

namespace Core
{
	public static class ExtentionMethod
	{
		#region CORLOR

		public static Color ChangeR(this Color col, float r)
		{
			col.r = r;
			return col;
		}

		public static Color ChangeG(this Color col, float g)
		{
			col.g = g;
			return col;
		}

		public static Color ChangeB(this Color col, float b)
		{
			col.b = b;
			return col;
		}

		public static Color ChangeA(this Color col, float a)
		{
			col.a = a;
			return col;
		}

		#endregion

		#region CORLOR32

		public static Color32 ChangeR(this Color32 col, int r)
		{
			col.r = (byte)r;
			return col;
		}

		public static Color32 ChangeG(this Color32 col, int g)
		{
			col.g = (byte)g;
			return col;
		}

		public static Color32 ChangeB(this Color32 col, int b)
		{
			col.b = (byte)b;
			return col;
		}

		public static Color32 ChangeA(this Color32 col, int a)
		{
			col.a = (byte)a;
			return col;
		}

		#endregion

		#region VECTOR2

		public static Vector2 ChangeX(this Vector2 vec2, float x)
		{
			vec2.x = x;
			return vec2;
		}

		public static Vector2 ChangeY(this Vector2 vec2, float y)
		{
			vec2.y = y;
			return vec2;
		}

		public static Vector2 ChangeXY(this Vector2 vec2, float x, float y)
		{
			vec2.x = x;
			vec2.y = y;
			return vec2;
		}

		public static Vector2 OpX(this Vector2 vec2, float x)
		{
			vec2.x += x;
			return vec2;
		}

		public static Vector2 OpY(this Vector2 vec2, float y)
		{
			vec2.y += y;
			return vec2;
		}

		#endregion

		#region VECTOR3

		public static Vector3 OpX(this Vector3 v, float x)
		{
			return new Vector3(v.x + x, v.y, v.z);
		}

		public static Vector3 OpY(this Vector3 v, float y)
		{
			return new Vector3(v.x, v.y + y, v.z);
		}

		public static Vector3 OpZ(this Vector3 v, float z)
		{
			return new Vector3(v.x, v.y, v.z + z);
		}

		public static Vector3 OpXY(this Vector3 v, float x, float y)
		{
			return new Vector3(v.x + x, v.y + y, v.z);
		}

		public static Vector3 ChangeX(this Vector3 v, float x)
		{
			return new Vector3(x, v.y, v.z);
		}

		public static Vector3 ChangeY(this Vector3 v, float y)
		{
			return new Vector3(v.x, y, v.z);
		}

		public static Vector3 ChangeZ(this Vector3 v, float z)
		{
			return new Vector3(v.x, v.y, z);
		}

		public static Vector3 ChangeXY(this Vector3 v, float x, float y)
		{
			return new Vector3(x, y, v.z);
		}

		public static Vector3 ChangeXZ(this Vector3 v, float x, float z)
		{
			return new Vector3(x, v.y, z);
		}

		#endregion

		#region Quaternion

		public static Quaternion ChangeX(this Quaternion q, float x)
		{
			return new Quaternion(x, q.y, q.z, q.w);
		}

		public static Quaternion ChangeY(this Quaternion q, float y)
		{
			return new Quaternion(q.x, y, q.z, q.w);
		}

		public static Quaternion ChangeZ(this Quaternion q, float z)
		{
			return new Quaternion(q.x, q.y, z, q.w);
		}

		public static Quaternion ChangeXZ(this Quaternion q, float x, float z)
		{
			return new Quaternion(x, q.y, z, q.w);
		}

		#endregion
	}
}