using UnityEngine;



public static class Vector3Helpers
{
	public static Vector3 GetClosestPointOnLine(this Vector3 point, Vector3 linePosition1, Vector3 linePosition2)
	{
		float dotPointLine1 = Vector3.Dot(point - linePosition1, linePosition2 - linePosition1);
		float dotPointLine2 = Vector3.Dot(point - linePosition2, linePosition1 - linePosition2);

		Vector3 closestPoint = linePosition1 + ((linePosition2 - linePosition1) * dotPointLine1) / (dotPointLine1 + dotPointLine2);

		return closestPoint;
	}



	public static float SquaredDistanceToLine(this Vector3 point, Vector3 linePosition1, Vector3 linePosition2)
	{
		Vector3 closestPointOnLine = point.GetClosestPointOnLine(linePosition1, linePosition2);

		float deltaX = point.x - closestPointOnLine.x;
		float deltaY = point.y - closestPointOnLine.y;
		float deltaZ = point.z - closestPointOnLine.z;

		return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
	}

}
