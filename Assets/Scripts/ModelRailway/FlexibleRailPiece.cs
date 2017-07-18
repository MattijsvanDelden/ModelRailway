// Model railway in C# by Mattijs

using UnityEngine;
using Debug=System.Diagnostics.Debug;



namespace ModelRailway
{

public class FlexibleRailPiece : RailPiece
{
	public FlexibleRailPiece(RailPieceDefinition definition, Vector3[] controlPoints) : base(definition, true, true)
	{
		if (controlPoints != null)
		{
			Debug.Assert(controlPoints.Length == 4);
			Debug.Assert(RailSegments.Length == 1);
			UpdateRailSegment(0, controlPoints);
		}
	}



	public void UpdateRailSegment(int index, Vector3[] controlPoints)
	{
		RailSegment segment = RailSegments[index];
		segment.Curve.SetControlPoints(controlPoints);

		Definition.CreateGeometry(RailSegments);
	}
}

}
