using System;
using UnityEngine;



namespace ModelRailway
{

class Bogie
{
	public Transform Transform
	{ 
		get; 
		private set; 
	}



	public BogieDefinition Definition
	{ 
		get; private set; 
	}



	public RailSegment FirstWheelsetSegment		
	{ 
		get; 
		private set; 
	}



	public float FirstWheelsetSegmentT;



	public int FirstWheelsetDirectionOfT;			// 1 if train car is facing same direction as segment (i.e. positive speed means increasing t), -1 otherwise



	public RailSegment LastWheelsetSegment
	{ 
		get; 
		private set; 
	}



	public float LastWheelsetSegmentT;



	public int LastWheelsetDirectionOfT;			// 1 if train car is facing same direction as segment (i.e. positive speed means increasing t), -1 otherwise



	public Bogie(int index, BogieDefinition definition, Transform trainCarGroup)
	{
		Definition = definition;

		// Get reference to bogie transformation node (if it exists)
		Transform = trainCarGroup.FindInTree("BOGIETRANSFORM" + index);

		// Get reference to wheelset transformation nodes
		m_wheelsetTransforms = new Transform[definition.Wheelsets.Length];
		for (int wheelsetIndex = 0 ; wheelsetIndex < definition.Wheelsets.Length ; wheelsetIndex++)
		{
			m_wheelsetTransforms[wheelsetIndex] = trainCarGroup.FindInTree("WHEELSETTRANSFORM" + index + wheelsetIndex);
			Debug.Assert(m_wheelsetTransforms[wheelsetIndex] != null);
		}

		m_wheelsetRotations = new float[definition.Wheelsets.Length];
		// Set initial random rotation of each wheelset for more realism. Note that
		// the seed value of the parameterless Random constructor is based on time,
		// which meant that we got the same rotation on different bogies, because the
		// bogies are initialised at the 'same' time.
		// Note that wheels can be driven by animations, in which case all wheels in the
		// animation get the same rotation (but the wheelset that drives the animation
		// still gets a random initial rotation)

		//var random = new Random(index);
		for (int wheelsetIndex = 0 ; wheelsetIndex < definition.Wheelsets.Length ; wheelsetIndex++)
		{
			m_wheelsetRotations[wheelsetIndex] = definition.InitialWheelsetRotation; //(float) (2 * Math.PI * random.NextDouble());
		}
	}



	public void TimestepUpdate(float deltaPosition)
	{
		if (FirstWheelsetSegment == null || LastWheelsetSegment == null)
		{
			return;
		}

		SavePreviousParameters();

		// Move first wheelset to travel signedSpeed * deltaTime
		float signedDistanceToMove = deltaPosition * FirstWheelsetDirectionOfT;
		float segmentT = FirstWheelsetSegmentT;
		int directionOfT = FirstWheelsetDirectionOfT;
		FirstWheelsetSegment = FirstWheelsetSegment.MoveSignedDistance(signedDistanceToMove, ref segmentT, ref directionOfT);
		if (FirstWheelsetSegment == null)
		{
			return;
		}
		FirstWheelsetSegmentT = segmentT;
		FirstWheelsetDirectionOfT = directionOfT;

		// Move last wheelset to keep up with first wheelset
		UpdateLastWheelsetPosition();
		if (LastWheelsetSegment == null)
		{
			return;
		}
		
		WheelsetsTimeStepUpdate(deltaPosition);
	}



	public void TimestepUpdate(Bogie previousBogie, float deltaPosition)
	{
		if (FirstWheelsetSegment == null || LastWheelsetSegment == null)
		{
			return;
		}

		SavePreviousParameters();

		// Move first wheelset to keep up with previous bogie
		Vector3 lastWheelsetPosition = previousBogie.LastWheelsetSegment.GetCurvePositionInWorldSpace(previousBogie.LastWheelsetSegmentT);
		Vector3 firstWheelsetPosition = FirstWheelsetSegment.GetCurvePositionInWorldSpace(FirstWheelsetSegmentT);
		float wantedDistance = DistanceFromLastWheelsetOfPreviousBogieToFirstWheelset(previousBogie);
		float actualDistance = (lastWheelsetPosition - firstWheelsetPosition).magnitude;
		float distanceToMove = FirstWheelsetDirectionOfT * (actualDistance - wantedDistance);
		FirstWheelsetSegment = FirstWheelsetSegment.MoveSignedDistance(distanceToMove, ref FirstWheelsetSegmentT, ref FirstWheelsetDirectionOfT);
		if (FirstWheelsetSegment == null)
		{
			return;
		}

		// Move last wheelset to keep up with first wheelset
		UpdateLastWheelsetPosition();
		if (LastWheelsetSegment == null)
		{
			return;
		}
		
		WheelsetsTimeStepUpdate(deltaPosition);
	}



	private void UpdateLastWheelsetPosition()
	{
		if (Definition.Wheelsets.Length == 1)
		{
			LastWheelsetSegment = FirstWheelsetSegment;
			LastWheelsetSegmentT = FirstWheelsetSegmentT;
			LastWheelsetDirectionOfT = FirstWheelsetDirectionOfT;
			return;
		}
		Vector3 firstWheelsetPosition = FirstWheelsetSegment.GetCurvePositionInWorldSpace(FirstWheelsetSegmentT);
		Vector3 lastWheelsetPosition = LastWheelsetSegment.GetCurvePositionInWorldSpace(LastWheelsetSegmentT);
		float wantedDistance = Definition.DistanceFromFirstToLastWheelset;
		float actualDistance = (firstWheelsetPosition - lastWheelsetPosition).magnitude;
		float distanceToMove = LastWheelsetDirectionOfT * (actualDistance - wantedDistance);
		LastWheelsetSegment = LastWheelsetSegment.MoveSignedDistance(distanceToMove, ref LastWheelsetSegmentT, ref LastWheelsetDirectionOfT);
	}



	public void FramestepUpdate(Transform[] debugWheelsetMarkers)
	{
		if (FirstWheelsetSegment == null || LastWheelsetSegment == null)
		{
			return;
		}

		Vector3 firstWheelsetPosition = FirstWheelsetSegment.GetCurvePositionInWorldSpace(FirstWheelsetSegmentT);
		if (debugWheelsetMarkers != null)
		{
			debugWheelsetMarkers[0].localPosition = firstWheelsetPosition + new Vector3(0, FirstWheelsetSegment.RailPiece.RailTopHeight, 0);
		}

		Vector3 firstToLastWheelsetDirection;
		if (Definition.Wheelsets.Length == 1)
		{
			firstToLastWheelsetDirection = -FirstWheelsetDirectionOfT * FirstWheelsetSegment.GetCurveTangentInWorldSpace(FirstWheelsetSegmentT);
		}
		else
		{
			Vector3 lastWheelsetPosition = LastWheelsetSegment.GetCurvePositionInWorldSpace(LastWheelsetSegmentT);
			if (debugWheelsetMarkers != null)
			{
				debugWheelsetMarkers[1].localPosition = lastWheelsetPosition + new Vector3(0, FirstWheelsetSegment.RailPiece.RailTopHeight, 0);
			}

			firstToLastWheelsetDirection = lastWheelsetPosition - firstWheelsetPosition;
			firstToLastWheelsetDirection.Normalize();
		}

		Vector3 bogiePosition = firstWheelsetPosition + Definition.Wheelsets[0].SignedDistanceToBogie * firstToLastWheelsetDirection;
		bogiePosition.y += Definition.Translation.y + FirstWheelsetSegment.RailPiece.RailTopHeight;

		float angle = -Mathf.Atan2(-firstToLastWheelsetDirection.z, -firstToLastWheelsetDirection.x);
		Transform.localPosition = bogiePosition;
		Transform.localRotation = Quaternion.Euler(0, angle, 0);

		for (int wheelsetIndex = 0 ; wheelsetIndex < m_wheelsetTransforms.Length ; wheelsetIndex++)
		{
			Vector3 wheelsetPositionInBogieSpace = m_wheelsetTransforms[wheelsetIndex].localPosition;
			m_wheelsetTransforms[wheelsetIndex].localRotation = Quaternion.Euler(0, 0, -m_wheelsetRotations[wheelsetIndex]);
			m_wheelsetTransforms[wheelsetIndex].localPosition = wheelsetPositionInBogieSpace;
		}
	}



	public void PutOnTrack(RailSegment firstWheelsetSegment, float firstWheelsetT, bool sameDirectionAsSegment)
	{
		FirstWheelsetSegment = firstWheelsetSegment;
		FirstWheelsetSegmentT = firstWheelsetT;
		FirstWheelsetDirectionOfT = sameDirectionAsSegment ? 1 : -1;

		PutLastWheelsetOnTrack();
	}



	public void PutOnTrack(Bogie previousBogie)
	{
		Debug.Assert(previousBogie != null);

		float distance = DistanceFromLastWheelsetOfPreviousBogieToFirstWheelset(previousBogie) * -previousBogie.LastWheelsetDirectionOfT;

		FirstWheelsetSegmentT = previousBogie.LastWheelsetSegmentT;
		FirstWheelsetDirectionOfT = previousBogie.LastWheelsetDirectionOfT;
		FirstWheelsetSegment = previousBogie.LastWheelsetSegment.MoveSignedDistance(distance, ref FirstWheelsetSegmentT, ref FirstWheelsetDirectionOfT);

		PutLastWheelsetOnTrack();
	}


	
	// TODO this is a constant for the train car (i.e. can be in definition)
	private float DistanceFromLastWheelsetOfPreviousBogieToFirstWheelset(Bogie previousBogie)
	{
		return Definition.DistanceToFront - Definition.Wheelsets[0].SignedDistanceToBogie -
			previousBogie.Definition.DistanceToFront +
			previousBogie.Definition.Wheelsets[previousBogie.Definition.Wheelsets.Length - 1].SignedDistanceToBogie;
	}



	private void SavePreviousParameters()
	{
		m_previousFirstWheelsetSegment = FirstWheelsetSegment;
		m_previousFirstWheelsetSegmentT = FirstWheelsetSegmentT;
		m_previousFirstWheelsetDirectionOfT = FirstWheelsetDirectionOfT;
		m_previousLastWheelsetSegment = LastWheelsetSegment;
		m_previousLastWheelsetSegmentT = LastWheelsetSegmentT;
		m_previousLastWheelsetDirectionOfT = LastWheelsetDirectionOfT;
	}



	public void RestorePreviousParameters()
	{
		FirstWheelsetSegment = m_previousFirstWheelsetSegment;
		FirstWheelsetSegmentT = m_previousFirstWheelsetSegmentT;
		FirstWheelsetDirectionOfT = m_previousFirstWheelsetDirectionOfT;
		LastWheelsetSegment = m_previousLastWheelsetSegment;
		LastWheelsetSegmentT = m_previousLastWheelsetSegmentT;
		LastWheelsetDirectionOfT = m_previousLastWheelsetDirectionOfT;
	}


	
	private void PutLastWheelsetOnTrack()
	{
		float distance = Definition.DistanceFromFirstToLastWheelset * -FirstWheelsetDirectionOfT;
		LastWheelsetSegmentT = FirstWheelsetSegmentT;
		LastWheelsetDirectionOfT = FirstWheelsetDirectionOfT;
		LastWheelsetSegment = FirstWheelsetSegment.MoveSignedDistance(distance, ref LastWheelsetSegmentT, ref LastWheelsetDirectionOfT);
	}



	private void WheelsetsTimeStepUpdate(float deltaPosition)
	{
		const float Limit = (float) (2 * Math.PI);
		for (int wheelsetIndex = 0 ; wheelsetIndex < m_wheelsetTransforms.Length ; wheelsetIndex++)
		{
			m_wheelsetRotations[wheelsetIndex] += deltaPosition / Definition.Wheelsets[wheelsetIndex].Radius;
			if (m_wheelsetRotations[wheelsetIndex] >= Limit)
			{
				m_wheelsetRotations[wheelsetIndex] -= Limit;
			}
			else if (m_wheelsetRotations[wheelsetIndex] < 0)
			{
				m_wheelsetRotations[wheelsetIndex] += Limit;
			}
		}
	}



	public bool GetWheelsetRotation(string wheelsetName, out float angle)
	{
		angle = 0;
		int index = 0;
		foreach (Transform transform in m_wheelsetTransforms)
		{
			if (transform.name == wheelsetName)
			{
				angle = m_wheelsetRotations[index];
				return true;
			}
			index++;
		}
		return false;
	}



	//- PRIVATE -------------------------------------------------------------------------------------



	private readonly Transform[]	m_wheelsetTransforms;
	private readonly float[]		m_wheelsetRotations;
	private RailSegment				m_previousFirstWheelsetSegment;
	private float					m_previousFirstWheelsetSegmentT;
	private int						m_previousFirstWheelsetDirectionOfT;
	private RailSegment				m_previousLastWheelsetSegment;
	private float					m_previousLastWheelsetSegmentT;
	private int						m_previousLastWheelsetDirectionOfT;							

}

}
