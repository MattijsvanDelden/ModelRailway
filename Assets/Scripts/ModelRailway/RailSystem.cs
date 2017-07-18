// Model railway in C# by Mattijs
using System.Collections.Generic;
using UnityEngine;



namespace ModelRailway
{
	public class RailSystem
	{
		private const float ConnectionEpsilon = 0.1f;

		public readonly List<RailSegment> RailSegments = new List<RailSegment>();



		public bool GetClosestConnector(
			Vector3 positionInWorldSpace, 
			RailPiece[] railPiecesToExclude, 
			out Vector3 closestPosition,
			out Vector3 closestDirection)
		{
			closestPosition = Vector3.zero;
			closestDirection = Vector3.right;

			if (RailSegments.Count == 0)
			{
				// There is no closest segment
				return false;
			}

			//float closestConnectorDistanceSquared = 1e30f;
			foreach (RailSegment railSegment in RailSegments)
			{
				// Is the current segment in the set of rail segments to skip?
				bool skipRailSegment = false;
				for (int k = 0; k < railPiecesToExclude.Length; k++)
				{
					if (railSegment.RailPiece == railPiecesToExclude[k])
					{
						skipRailSegment = true;
						break;
					}
				}
				if (skipRailSegment)
					continue;

/*
				Matrix transform = railSegment.RailPiece.Transform.GetWorldTransformation();
				Vector3 connectorPositionInWorldSpace = Vector3.Transform(railSegment.Curve.GetPosition(0), transform);
				float distanceSquared = (connectorPositionInWorldSpace - positionInWorldSpace).LengthSquared();
				if (distanceSquared < closestConnectorDistanceSquared)
				{
					closestConnectorDistanceSquared = distanceSquared;
					closestPosition = connectorPositionInWorldSpace;
					closestDirection = Vector3.TransformNormal(railSegment.Curve.GetTangent(0), transform);
				}
				connectorPositionInWorldSpace = Vector3.Transform(railSegment.Curve.GetPosition(1), transform);
				distanceSquared = (connectorPositionInWorldSpace - positionInWorldSpace).LengthSquared();
				if (distanceSquared < closestConnectorDistanceSquared)
				{
					closestConnectorDistanceSquared = distanceSquared;
					closestPosition = connectorPositionInWorldSpace;
					closestDirection = Vector3.TransformNormal(railSegment.Curve.GetTangent(1), transform);
				}
*/
			}
			return true;
		}



		public void AddRailSegment(RailSegment segment)
		{
			if (RailSegments.Contains(segment))
			{
				// If the segment is in the list of rail segments, then
				// it is already connected to other segments in the list
				// and also should not be added to the list again
				return;
			}

			// Segment should not be connected to anything
			Debug.Assert(segment.ConnectedSegmentInfos.Count== 0);

			// Try to connect the segment to connectors of segments in the system
			foreach (RailSegment systemSegment in RailSegments)
			{
/*
				Matrix systemTransform = systemSegment.WorldTransformation;
				Matrix transform = segment.WorldTransformation;

				Vector3 systemCurveStartInWorldSpace = Vector3.Transform(systemSegment.Curve.GetPosition(0), systemTransform);
				Vector3 systemCurveDirectionInWorldSpace = Vector3.TransformNormal(systemSegment.Curve.GetTangent(0), systemTransform);

				Vector3 curveStartInWorldSpace = Vector3.Transform(segment.Curve.GetPosition(0), transform);
				Vector3 curveDirectionInWorldSpace = Vector3.TransformNormal(segment.Curve.GetTangent(0), transform);
				
				if (systemCurveStartInWorldSpace.EqualsWithEpsilon(curveStartInWorldSpace, ConnectionEpsilon) &&
					Vector3.Dot(systemCurveDirectionInWorldSpace, -curveDirectionInWorldSpace) > 0.9f)
				{
					// Open connectors of both segments at t = 0 connect
					// I.E. tail-to-tail connection
					systemSegment.ConnectSegment(segment, true, false);
				}

				Vector3 curveEndInWorldSpace = Vector3.Transform(segment.Curve.GetPosition(1), transform);
				curveDirectionInWorldSpace = Vector3.TransformNormal(segment.Curve.GetTangent(1), transform);
				
				if (systemCurveStartInWorldSpace.EqualsWithEpsilon(curveEndInWorldSpace, ConnectionEpsilon) &&
					Vector3.Dot(systemCurveDirectionInWorldSpace, curveDirectionInWorldSpace) > 0.9f)
				{
					// system segment's connector at t = 0 connects with segment's connector at t = 1
					// I.E. tail-to-head connection
					systemSegment.ConnectSegment(segment, true, true);
				}

				Vector3 systemCurveEndInWorldSpace = Vector3.Transform(systemSegment.Curve.GetPosition(1), systemTransform);
				systemCurveDirectionInWorldSpace = Vector3.TransformNormal(systemSegment.Curve.GetTangent(1), systemTransform);

				curveStartInWorldSpace = Vector3.Transform(segment.Curve.GetPosition(0), transform);
				curveDirectionInWorldSpace = Vector3.TransformNormal(segment.Curve.GetTangent(0), transform);
	
				if (systemCurveEndInWorldSpace.EqualsWithEpsilon(curveStartInWorldSpace, ConnectionEpsilon) &&
					Vector3.Dot(systemCurveDirectionInWorldSpace, curveDirectionInWorldSpace) > 0.9f)
				{
					// system segment's connector at t = 1 connects with segment's connector at t = 1
					// I.E. head-to-tail connection
					systemSegment.ConnectSegment(segment, false, true);
				}

				curveEndInWorldSpace = Vector3.Transform(segment.Curve.GetPosition(1), transform);
				curveDirectionInWorldSpace = Vector3.TransformNormal(segment.Curve.GetTangent(1), transform);

				if (systemCurveEndInWorldSpace.EqualsWithEpsilon(curveEndInWorldSpace, ConnectionEpsilon) &&
					Vector3.Dot(systemCurveDirectionInWorldSpace, -curveDirectionInWorldSpace) > 0.9f)
				{
					// Open connectors of both segments at t = 1 connect
					// I.E. head-to-head connection
					systemSegment.ConnectSegment(segment, false, false);
				}
*/
			}

			RailSegments.Add(segment);
		}


		public void RemoveRailSegment(RailSegment segment)
		{
			if (!RailSegments.Contains(segment))
			{
				return;
			}
			segment.Detach();
			RailSegments.Remove(segment);
		}



		public void SetDefaultPath()
		{
			foreach (RailSegment segment in RailSegments)
			{
				bool nextSet = false;
				bool previousSet = false;
				foreach(RailSegment.ConnectedSegmentInfo connectedSegmentInfo in segment.ConnectedSegmentInfos)
				{
					if (connectedSegmentInfo.AsPrevious)
					{
						if (!previousSet)
						{
							connectedSegmentInfo.Active = true;
							previousSet = true;
						}
						else
						{
							connectedSegmentInfo.Active = false;
						}
					}
					else
					{
						if (!nextSet)
						{
							connectedSegmentInfo.Active = true;
							nextSet = true;
						}
						else
						{
							connectedSegmentInfo.Active = false;
						}
					}
				}
			}
		}
	}
}
