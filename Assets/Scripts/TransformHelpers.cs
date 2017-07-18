using UnityEngine;



public static class TransformHelpers
{
	public static Transform FindInTree(this Transform transform, string name)
	{
		if (transform.name == name)
		{
			return transform;
		}
		foreach (Transform child in transform)
		{
			Transform foundTransform = child.FindInTree(name);
			if (foundTransform != null)
			{
				return foundTransform;
			}
		}

		return null;
	}
}
