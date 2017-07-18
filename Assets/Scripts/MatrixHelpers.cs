using System;
using UnityEngine;


public static class MatrixHelpers
{
	public static Matrix4x4 CreateLookAt(Vector3 position, Vector3 direction, Vector3 up)
	{
		if (direction.magnitude == 0)
		{
			throw new ArgumentException("Direction length is zero", "direction");
		}
		if (up.magnitude == 0)
		{
			throw new ArgumentException("Up vector is zero", "up");
		}

		// Create binormal vector perpendicular to direction and up vectors
		Vector3 binormal = Vector3.Cross(direction, up);

		// Adjust up to be perpendicular to both direction and binormal
		up = Vector3.Cross(binormal, direction);

		var transform = new Matrix4x4();
		transform.SetRow(0, new Vector4(direction.x, direction.y, direction.z, 0)); 
		transform.SetRow(1, new Vector4(up.x, up.y, up.z, 0));
		transform.SetRow(2, new Vector4(binormal.x, binormal.y, binormal.z, 0));
		transform.SetRow(3, new Vector4(position.x, position.y, position.z, 1));

		return transform;
	}

}
