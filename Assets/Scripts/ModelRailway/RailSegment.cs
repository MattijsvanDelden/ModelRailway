// Model railway in C# by Mattijs
using System;
using System.Collections.Generic;
using UnityEngine;



namespace ModelRailway
{

public class RailSegment
{
	public class ConnectedSegmentInfo
	{
		public RailSegment RailSegment;		// The connected segment
		public bool AsPrevious;				// If true, connected segment is connected as previous (i.e. to start of curve); if false, as next (i.e. to end of curve)
		public bool SameTDirection;			// Whether curves of the segments are connected head-to-tail/tail-to-head or head-to-head/tail-to-tail
		public bool Active;					// If true, there's a path to the connected segment (this is for points)
	};

	public static readonly ConnectedSegmentInfo NoConnectedSegmentInfo = new ConnectedSegmentInfo
	{
		RailSegment = null,
		AsPrevious = true,
		SameTDirection = true
	};

	private const float CurvePositionTolerance = 0.0001f;



	// The rail piece this segment belongs to
	public RailPiece RailPiece { get; private set; }



	// List containing information about all connections of this segment with other segments
	public readonly List<ConnectedSegmentInfo> ConnectedSegmentInfos = new List<ConnectedSegmentInfo>();



	public BezierCurve3 Curve { get; private set; }



	// Returns the transformation from local segment space to world space
	public Matrix4x4 WorldTransformation 
	{ 
		get { return RailPiece != null ? RailPiece.GameObject.transform.localToWorldMatrix : Matrix4x4.identity; }
	}

    

	public RailSegment(BezierCurve3 curve)
	{
		Curve = curve;
	}



	public RailSegment(BezierCurve3 curve, RailPiece railPiece)
	{
		RailPiece = railPiece;
		Curve = curve;
	}



	public RailSegment(RailSegment source, RailPiece railPiece, bool copyCurve)
	{
		RailPiece = railPiece;
		Curve = copyCurve ? new BezierCurve3(source.Curve) : source.Curve;
	}



	public void ConnectSegment(RailSegment segment, bool asPreviousSegment, bool segmentHasSameTDirection)
	{
		// Add segment to list of connected segments
		var segmentInfo = new ConnectedSegmentInfo
		{
			RailSegment = segment,
			AsPrevious = asPreviousSegment,
			SameTDirection = segmentHasSameTDirection
		};
		ConnectedSegmentInfos.Add(segmentInfo);
		
		// Add me to other segment's list of connected segments
		var otherSegmentInfo = new ConnectedSegmentInfo
		{
			RailSegment = this,
			AsPrevious = asPreviousSegment ? !segmentHasSameTDirection : segmentHasSameTDirection,
			SameTDirection = segmentHasSameTDirection
		};
		segment.ConnectedSegmentInfos.Add(otherSegmentInfo);
	}



	public void Detach()
	{
		foreach(ConnectedSegmentInfo connectedSegmentInfo in ConnectedSegmentInfos)
		{
			RailSegment connectedSegment = connectedSegmentInfo.RailSegment;
			int removeIndex = connectedSegment.GetConnectionIndex(this);
			connectedSegment.ConnectedSegmentInfos.RemoveAt(removeIndex);
		}
		ConnectedSegmentInfos.Clear();
	}



	private int GetConnectionIndex(RailSegment segment)
	{
		for (int k = 0 ; k < ConnectedSegmentInfos.Count ; k++)
		{
			if (ConnectedSegmentInfos[k].RailSegment == segment)
			{
				return k;
			}
		}
		throw new ArgumentException("Given segment is not connected");
	}



	public RailSegment MoveSignedDistance(float signedDistance, ref float t)
	{
		int dummyDirection = 1;
		return MoveSignedDistance(signedDistance, ref t, ref dummyDirection);
	}


	public RailSegment MoveSignedDistance(float signedDistance, ref float t, ref int directionAlongT)
	{
		// Move over the rail segment for the given distance, going to a next or previous
		// segment if the distance to move is bigger than the distance to the end of the curve of the segment.
		// If it is, subtract the remaining length on the curve from the distance to travel, set the new
		// segment as the current segment, and repeat.
		// The distance to move is signed indicating the direction to move over the segment(s). Positive means
		// in the same direction as curve parameter t (going from 0 to 1).
		// Segments can be connected in different ways (head to head, head to tail, etc). Variable XXX
		// is used to keep track of the direction on the curve. If true, we are moving from current curve parameter
		// t = <current> to t = 0. If false, we move towards t = 1

		float remainingDistance = Math.Abs(signedDistance);
		bool movePositiveInitial = signedDistance > 0;
		bool movePositive = movePositiveInitial;
		RailSegment currentSegment = this;

		while (remainingDistance > 0)
		{
			float curvePosition = currentSegment.Curve.GetLength(0, t);

			if (movePositive)
			{
				// Moving over curve towards t = 1 (positive direction)
				float remainingCurveLength = Curve.Length - curvePosition;
				if (remainingDistance <= remainingCurveLength)
				{
					// More curve remaining than distance to move. Update time and done
					curvePosition += remainingDistance;
					t = currentSegment.Curve.GetTime(curvePosition, 32, CurvePositionTolerance);
					remainingDistance = 0;
				}
				else
				{
					// More distance to move than curve length remaining. 
					// Move to end of curve, set next curve as current (if it exists), and continue
					remainingDistance -= remainingCurveLength;
					ConnectedSegmentInfo connectedSegmentInfo = GetActiveConnectedSegment(false);
					if (connectedSegmentInfo.RailSegment != null)
					{
						if (connectedSegmentInfo.SameTDirection)
						{
							t = 0;
						}
						else
						{
							movePositive = false;
							t = 1;
						}
						currentSegment = connectedSegmentInfo.RailSegment;
					}
					else
					{
						// No active next segment: end of track and done
						currentSegment = null;
						remainingDistance = 0;
						t = 1;
					}
				}
			}
			else
			{
				// Moving over curve towards t = 0 (negative direction)
				if (remainingDistance <= curvePosition)
				{
					// More curve remaining than distance to move. Update time and done
					curvePosition -= remainingDistance;
					t = currentSegment.Curve.GetTime(curvePosition, 32, CurvePositionTolerance);
					remainingDistance = 0;
				}
				else
				{
					// More distance to move than curve length remaining. 
					// Move to start of curve, set previous curve as current (if it exists), and continue
					remainingDistance -= curvePosition;
					ConnectedSegmentInfo connectedSegmentInfo = GetActiveConnectedSegment(true);
					if (connectedSegmentInfo.RailSegment != null)
					{
						if (connectedSegmentInfo.SameTDirection)
						{
							t = 1;
						}
						else
						{
							movePositive = true;
							t = 0;
						}
						currentSegment = connectedSegmentInfo.RailSegment;
					}
					else
					{
						// No active previous segment: end of track and done
						currentSegment = null;
						remainingDistance = 0;
						t = 1;
					}
				}
			}
		}
		if (movePositive != movePositiveInitial)
		{
			directionAlongT = -directionAlongT;
		}
		return currentSegment;
	}



	private ConnectedSegmentInfo GetActiveConnectedSegment(bool previous)
	{
		foreach(ConnectedSegmentInfo connectedSegmentInfo in ConnectedSegmentInfos)
		{
			if (connectedSegmentInfo.AsPrevious == previous && connectedSegmentInfo.Active)
			{
				return connectedSegmentInfo;
			}
		}
		return NoConnectedSegmentInfo;
	}



	public Vector3 GetConnectorPositionInWorldSpace(bool atCurveStart)
	{
		Vector3 connectorPosition = Curve.GetPosition(atCurveStart ? 0 : 1);
		return RailPiece.GameObject.transform.localToWorldMatrix.MultiplyPoint(connectorPosition);
	}


	
	public Vector3 GetConnectorTangentInWorldSpace(bool atCurveStart)
	{
		Vector3 connectorDirection = Curve.GetTangent(atCurveStart ? 0 : 1);
		if (atCurveStart)
		{
			connectorDirection = -connectorDirection;
		}
		return RailPiece.GameObject.transform.localToWorldMatrix.MultiplyVector(connectorDirection);
	}



	public Vector3 GetCurvePositionInWorldSpace(float curveT)
	{
		Vector3 position = Curve.GetPosition(curveT);
		return RailPiece.GameObject.transform.localToWorldMatrix.MultiplyPoint(position);
	}



	public Vector3 GetCurveTangentInWorldSpace(float curveT)
	{
		Vector3 tangent = Curve.GetTangent(curveT);
		return RailPiece.GameObject.transform.localToWorldMatrix.MultiplyVector(tangent);
	}



	public bool AllConnectedSegmentsAreFlexible(bool previousSegments)
	{
		foreach (ConnectedSegmentInfo connectedSegmentInfo in ConnectedSegmentInfos)
		{
			if (connectedSegmentInfo.AsPrevious != previousSegments)
			{
				// Only check connected segments at specified segment endpoint
				continue;
			}
			if (!connectedSegmentInfo.RailSegment.RailPiece.Definition.IsFlexible)
			{
				return false;
			}
		}
		return true;
	}



	public int GetConnectionCount(bool previousSegments)
	{
		int count = 0;
		foreach (ConnectedSegmentInfo connectedSegmentInfo in ConnectedSegmentInfos)
		{
			if (connectedSegmentInfo.AsPrevious == previousSegments)
			{
				count++;
			}
		}
		return count;
	}



	public void UpdateConnectedShapeControlPoints(Vector3 directionInWorldSpace, bool previousSegments)
	{
/*
		if (previousSegments)
		{
			foreach (ConnectedSegmentInfo connectedSegmentInfo in ConnectedSegmentInfos)
			{
				if (!connectedSegmentInfo.AsPrevious)
				{
					continue;
				}
				RailSegment previousSegment = connectedSegmentInfo.RailSegment;
				Matrix previousTransform = previousSegment.RailPiece.Transform.GetWorldTransformation();
				Matrix previousTransformInverse = Matrix.Invert(previousTransform);
				Vector3 directionInPreviousSegmentSpace = Vector3.TransformNormal(directionInWorldSpace, previousTransformInverse);
				directionInPreviousSegmentSpace.Normalize();
				if (connectedSegmentInfo.SameTDirection)
				{
					float length = (previousSegment.Curve.ControlPoints[2] - previousSegment.Curve.ControlPoints[3]).Length();
					previousSegment.Curve.SetControlPoint(2, previousSegment.Curve.ControlPoints[3] + directionInPreviousSegmentSpace * length);
				}
				else
				{
					float length = (previousSegment.Curve.ControlPoints[1] - previousSegment.Curve.ControlPoints[0]).Length();
					previousSegment.Curve.SetControlPoint(1, previousSegment.Curve.ControlPoints[0] + directionInPreviousSegmentSpace * length);
				}

				// Update (by recreating) the 'previous' flexible rail piece geometry
				RailPiece previousRailPiece = previousSegment.RailPiece;
				GeometrySet previousGeometrySet = previousRailPiece.GameObject.GeometrySet;
				previousGeometrySet.Geometries.Clear();
				previousGeometrySet.Geometries.Add(previousRailPiece.Definition.CreateGeometry(previousRailPiece.RailSegments));
			}
		}
		else
		{
			foreach (ConnectedSegmentInfo connectedSegmentInfo in ConnectedSegmentInfos)
			{
				if (connectedSegmentInfo.AsPrevious)
				{
					continue;
				}

				RailSegment nextSegment = connectedSegmentInfo.RailSegment;

				Matrix nextTransform = nextSegment.RailPiece.Transform.GetWorldTransformation();
				Matrix nextTransformInverse = Matrix.Invert(nextTransform);
				Vector3 directionInNextSegmentSpace = Vector3.TransformNormal(directionInWorldSpace, nextTransformInverse);
				directionInNextSegmentSpace.Normalize();

				if (connectedSegmentInfo.SameTDirection)
				{
					float length = (nextSegment.Curve.ControlPoints[1] - nextSegment.Curve.ControlPoints[0]).Length();
					nextSegment.Curve.SetControlPoint(1, nextSegment.Curve.ControlPoints[0] + directionInNextSegmentSpace * length);
				}
				else
				{
					float length = (nextSegment.Curve.ControlPoints[2] - nextSegment.Curve.ControlPoints[3]).Length();
					nextSegment.Curve.SetControlPoint(2, nextSegment.Curve.ControlPoints[3] + directionInNextSegmentSpace * length);
				}

				// Update (by recreating) the 'next' flexible rail piece geometry
				RailPiece nextRailPiece = nextSegment.RailPiece;
				GeometrySet nextGeometrySet = nextRailPiece.GameObject.GeometrySet;
				nextGeometrySet.Geometries.Clear();
				nextGeometrySet.Geometries.Add(nextRailPiece.Definition.CreateGeometry(nextRailPiece.RailSegments));
			}
		}
*/
	}



	public int GetActiveConnectionIndex(bool previous)
	{
		int index = 0;
		foreach(ConnectedSegmentInfo info in ConnectedSegmentInfos)
		{
			if (info.AsPrevious != previous)
			{
				continue;
			}
			if (info.Active)
			{
				return index;
			}
			index++;
		}
		return -1;
	}



	public ConnectedSegmentInfo GetConnectionInfo(bool previous, int index)
	{
		int currentIndex = 0;
		foreach(ConnectedSegmentInfo info in ConnectedSegmentInfos)
		{
			if (info.AsPrevious != previous)
			{
				continue;
			}
			if (currentIndex == index)
			{
				return info;
			}
			currentIndex++;
		}
		return null;
	}



	public ConnectedSegmentInfo GetActiveConnectionInfo(bool previous)
	{
		foreach(ConnectedSegmentInfo info in ConnectedSegmentInfos)
		{
			if (info.Active && info.AsPrevious == previous)
			{
				return info;
			}
		}
		return null;
	}



	public void SetActiveConnection(bool previous, int index)
	{
		ConnectedSegmentInfo activeInfo = GetActiveConnectionInfo(previous);
		ConnectedSegmentInfo wantedInfo = GetConnectionInfo(previous, index);
		activeInfo.Active = false;
		wantedInfo.Active = true;
	}
}
}
