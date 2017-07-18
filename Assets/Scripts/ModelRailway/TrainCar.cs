using UnityEngine;
using System.Collections.Generic;



namespace ModelRailway
{

public class TrainCar
{
	public TrainCar(TrainCarDefinition definition)
	{
		m_definition = definition;
		m_visual	 = Object.Instantiate(m_definition.Visual);

		m_signedSpeed = 0;

		InitialiseAnimations();

		m_bogies = new Bogie[definition.Bogies.Length];
		m_fakeBogieIndex = -1;
		for (int bogieIndex = 0 ; bogieIndex < definition.Bogies.Length ; bogieIndex++)
		{
			/*
			m_bogies[bogieIndex] = new Bogie(bogieIndex, definition.Bogies[bogieIndex], GroupNode);
			if (m_bogies[bogieIndex].Transform == null)
			{
				Debug.Assert(m_fakeBogieIndex < 0);
				m_fakeBogieIndex = bogieIndex;
			}

			m_debugWheelsetMarkers[bogieIndex] = new TransformNode[2];
			for (int wheelsetIndex = 0; wheelsetIndex < 2; wheelsetIndex++)
			{
				var debugMarker = new TransformNode("DebugWheelsetMarker");
				debugMarker.AddChild(GeometryNode.CreateCube(0.25f, Colour.White, null));
				m_debugWheelsetMarkers[bogieIndex][wheelsetIndex] = debugMarker;
				debugMarker.Enabled = false;
				GroupNode.AddChild(debugMarker);
			}
			 * */
		}

	}



	public virtual void TimestepUpdate(float deltaTime)
	{
		foreach (ShapeAnimation animation in m_animations)
		{
			animation.TimeStepUpdate(deltaTime);
		}
	}



	public virtual void FramestepUpdate(float deltaTIme)
	{
		float normalizedTime = m_animations.Count > 0 ? GetWheelsetRotation(m_animations[0]) / (2 * Mathf.PI) : 0;  // TODO the wheelset that drives the animation is constant and can be set in the contructor
		foreach (ShapeAnimationNode animationNode in m_drivetrainAnimations)
		{
			animationNode.NormalizedTime = normalizedTime;
		}
		
		foreach (ShapeAnimation animation in m_animations)
		{
			animation.FrameStepUpdate();
		}
		
	}



	public void ToggleMirrors()
	{
		PlayAnimationOnce(m_mirrorAnimations, m_mirrorsOut);
		m_mirrorsOut = !m_mirrorsOut;
	}



	public void TogglePantos()
	{
		PlayAnimationOnce(m_pantoAnimations, m_pantosUp);
		m_pantosUp = !m_pantosUp;
	}



	public void ToggleWipers()
	{
		PlayAnimationOnce(m_wiperAnimations, m_wipersOn);
		m_wipersOn = !m_wipersOn;
	}



	//- PROTECTED ----------------------------------------------------------------------------------



	protected void AddLocalForce(float force)
	{
		m_localForce += force;
	}



	protected float	m_signedSpeed;



	//- PRIVATE ------------------------------------------------------------------------------------



	private void InitialiseAnimations()
	{
		// Clone animations
		foreach(ShapeAnimation animation in m_definition.Animations)
		{
			m_animations.Add(new ShapeAnimation(animation, m_visual.transform));
		}

		// Assign animation group types
		foreach(ShapeAnimation animation in m_animations)
		{
			foreach(ShapeAnimationNode node in animation.Nodes)
			{
				if (node.Name.StartsWith("WIPER"))
				{
					m_wiperAnimations.Add(node);
				}
				else if (node.Name.StartsWith("PANTOGRAPH"))
				{
					m_pantoAnimations.Add(node);
				}
				else if (node.Name.StartsWith("MIRROR"))
				{
					m_mirrorAnimations.Add(node);
				}
				else
				{
					m_drivetrainAnimations.Add(node);
				}
			}
		}
	}



	private void PlayAnimationOnce(List<ShapeAnimationNode> animationNodes, bool reverse)
	{
		foreach (ShapeAnimationNode node in animationNodes)
		{
			node.PlayWrapMode = ShapeAnimationNode.WrapMode.Once;
			node.Reverse = reverse;
			node.Playing = true;
		}
	}



	private float GetWheelsetRotation(ShapeAnimation animation)
	{
		float time = Time.time;
		return 2 * Mathf.PI * (time - (int) time);
/*
		foreach (ShapeAnimationNode node in animation.Nodes)
		{
			if (node.Name.StartsWith("WHEELSETTRANSFORM"))
			{
				foreach (Bogie bogie in m_bogies)
				{
					float angle;
					if (bogie.GetWheelsetRotation(node.Name, out angle))
					{
						return angle;
					}
				}
			}
		}
		return 0;
*/
	}



	private readonly TrainCarDefinition			m_definition;
	private readonly GameObject					m_visual;
	private readonly List<ShapeAnimation>		m_animations			= new List<ShapeAnimation>();
	private readonly List<ShapeAnimationNode>	m_drivetrainAnimations	= new List<ShapeAnimationNode>();
	private readonly List<ShapeAnimationNode>	m_wiperAnimations		= new List<ShapeAnimationNode>();	// TODO why not use a ShapeAnimation?
	private readonly List<ShapeAnimationNode>	m_pantoAnimations		= new List<ShapeAnimationNode>();
	private readonly List<ShapeAnimationNode>	m_mirrorAnimations		= new List<ShapeAnimationNode>();
	private readonly Bogie[]					m_bogies;
	private readonly int						m_fakeBogieIndex;


	private bool	m_pantosUp;
	private bool	m_wipersOn;
	private bool	m_mirrorsOut;
	private float	m_localForce;
}


/*

using System;
using System.Collections.Generic;
using UnityEngine;



namespace ModelRailway
{

public enum CoupleType
{
	Full,
	PushOnly,
	None,
}



public class TrainCar
{
	private static int m_nextFreeID = 1;

	private readonly List<ShapeAnimation> m_animations = new List<ShapeAnimation>();

	private readonly List<ShapeAnimationNode> m_driveTrainAnimations = new List<ShapeAnimationNode>();

	private readonly List<ShapeAnimationNode> m_wiperAnimations = new List<ShapeAnimationNode>();

	private readonly List<ShapeAnimationNode> m_pantoAnimations = new List<ShapeAnimationNode>();

	private readonly List<ShapeAnimationNode> m_mirrorAnimations = new List<ShapeAnimationNode>();


	private int ID { get; set; }


	public Transform GroupNode { get; private set; }


	public TrainCar Next 
	{ 
		get { return Reversed ? m_nextAtFront : m_nextAtRear; }
		private set 
		{ 
			if (Reversed) 
			{
				m_nextAtFront = value; 
			}
			else 
			{
				m_nextAtRear = value; 
			} 
		}
	}

	public TrainCar Previous 
	{ 
		get { return Reversed ? m_nextAtRear : m_nextAtFront; }
		private set 
		{ 
			if (Reversed) 
			{
				m_nextAtRear = value; 
			}
			else 
			{
				m_nextAtFront = value; 
			} 
		}
	}

	public CoupleType NextCoupleType
	{ 
		get { return Reversed ? m_coupleTypeAtFront : m_coupleTypeAtRear; }
		private set 
		{ 
			if (Reversed) 
			{
				m_coupleTypeAtFront = value; 
			}
			else 
			{
				m_coupleTypeAtRear = value; 
			} 
		}
	}

	public CoupleType PreviousCoupleType 
	{ 
		get { return Reversed ? m_coupleTypeAtRear : m_coupleTypeAtFront; }
		private set 
		{ 
			if (Reversed) 
			{
				m_coupleTypeAtRear = value; 
			}
			else 
			{
				m_coupleTypeAtFront = value; 
			} 
		}
	}

	public bool ShowDebugInfo
	{
		set 
		{ 
			foreach (Transform[] transformNodes in m_debugWheelsetMarkers)
			{
				foreach (Transform transformNode in transformNodes)
				{
					transformNode.gameObject.SetActive(value);
				}
			}
			m_debugFrontCouplerMarker.gameObject.SetActive(value);
			m_debugRearCouplerMarker.gameObject.SetActive(value);
		}
	}


	private RailSegment FrontSegment				{ get { return m_bogies[0].FirstWheelsetSegment; } }
	private float		FrontSegmentT				{ get { return m_bogies[0].FirstWheelsetSegmentT; } }
	private int			FrontSegmentTDirection		{ get { return m_bogies[0].FirstWheelsetDirectionOfT; } }

	public Vector3 DirectionInWorldSpace
	{
		get 
		{  
			Vector3 direction = GetFrontPositionInWorldSpace() - GetRearPositionInWorldSpace();
			direction.Normalize();
			return direction;
		}
	}

	public Matrix4x4 BodyTransform { get { return m_bodyTransformNode.localToWorldMatrix; } }



	public readonly TrainCarDefinition Definition;
	private readonly Transform		m_bodyTransformNode;

	private readonly Transform[][]	m_debugWheelsetMarkers;
	private readonly Transform		m_debugFrontCouplerMarker;
	private readonly Transform		m_debugRearCouplerMarker;
	private readonly Material		m_debugFrontCouplerMarkerMaterial;
	private readonly Material		m_debugRearCouplerMarkerMaterial;


	protected float			m_mass;

	private TrainCar		m_nextAtFront;
	private	CoupleType		m_coupleTypeAtFront;
	private TrainCar		m_nextAtRear;
	private CoupleType		m_coupleTypeAtRear;
	public readonly bool	Reversed;



	public TrainCar(TrainCarDefinition definition)
	{
		if (!definition.IsLoaded)
		{
			throw new ArgumentException("Cannot create TrainCar instance of unloaded definition");
		}

		m_mass = 0.2f;
		m_nextAtFront = null;
		m_nextAtRear = null;
		Reversed = false;
		ID = m_nextFreeID++;
		Definition = definition;
		GroupNode = definition.GroupNode.Copy(false, false) as GroupNode;
		Debug.Assert(GroupNode != null);
		m_bodyTransformNode = GroupNode.Find<TransformNode>("BODYTRANSFORM");
		Debug.Assert(m_bodyTransformNode != null);

		m_bogies = new Bogie[definition.Bogies.Count];

		m_debugWheelsetMarkers		= new TransformNode[definition.Bogies.Count][];
		
		m_debugFrontCouplerMarker	= new TransformNode("DebugFrontCouplerMarker");
		m_debugRearCouplerMarker	= new TransformNode("DebugRearCouplerMarker");
		GeometryNode frontGeometryNode= GeometryNode.CreateCube(0.25f, Colour.Grey50Percent, null);
		m_debugFrontCouplerMarker.AddChild(frontGeometryNode);
		GeometryNode rearGeometryNode= GeometryNode.CreateCube(0.25f, Colour.Grey50Percent, null);
		m_debugRearCouplerMarker.AddChild(rearGeometryNode);
		m_debugFrontCouplerMarkerMaterial = frontGeometryNode.MaterialTable.Materials[0];
		m_debugRearCouplerMarkerMaterial = rearGeometryNode.MaterialTable.Materials[0];
		GroupNode.AddChild(m_debugFrontCouplerMarker);
		GroupNode.AddChild(m_debugRearCouplerMarker);
		m_debugFrontCouplerMarker.Enabled = false;
		m_debugRearCouplerMarker.Enabled = false;

		m_fakeBogieIndex = -1;
		for (int bogieIndex = 0 ; bogieIndex < definition.Bogies.Count ; bogieIndex++)
		{
			m_bogies[bogieIndex] = new Bogie(bogieIndex, definition.Bogies[bogieIndex], GroupNode);
			if (m_bogies[bogieIndex].Transform == null)
			{
				Debug.Assert(m_fakeBogieIndex < 0);
				m_fakeBogieIndex = bogieIndex;
			}

			m_debugWheelsetMarkers[bogieIndex] = new TransformNode[2];
			for (int wheelsetIndex = 0; wheelsetIndex < 2; wheelsetIndex++)
			{
				var debugMarker = new TransformNode("DebugWheelsetMarker");
				debugMarker.AddChild(GeometryNode.CreateCube(0.25f, Colour.White, null));
				m_debugWheelsetMarkers[bogieIndex][wheelsetIndex] = debugMarker;
				debugMarker.Enabled = false;
				GroupNode.AddChild(debugMarker);
			}
		}

		// Clone animations
		foreach(ShapeAnimation animation in definition.Animations)
		{
			m_animations.Add(new ShapeAnimation(animation, GroupNode));
		}

		foreach(ShapeAnimation animation in m_animations)
		{
			foreach(ShapeAnimationNode node in animation.Nodes)
			{
				if (node.Name.StartsWith("WIPER"))
				{
					m_wiperAnimations.Add(node);
				}
				else if (node.Name.StartsWith("PANTOGRAPH"))
				{
					m_pantoAnimations.Add(node);
				}
				else if (node.Name.StartsWith("MIRROR"))
				{
					m_mirrorAnimations.Add(node);
				}
				else
				{
					m_driveTrainAnimations.Add(node);
				}
			}
		}
	}



	public void PutOnTrack(RailSegment frontSegment, float frontSegmentT, bool sameDirectionasSegment)
	{
		if (m_bogies.Length == 0)
		{
			return;
		}

		m_bogies[0].PutOnTrack(frontSegment, frontSegmentT, sameDirectionasSegment);

		for (int bogieIndex = 1; bogieIndex < m_bogies.Length; bogieIndex++)
		{
			m_bogies[bogieIndex].PutOnTrack(m_bogies[bogieIndex - 1]);
		}
	}



	public void PutOnTrackAfter(TrainCar previousCar)
	{
		Debug.Assert(previousCar.FrontSegmentTDirection == 1); // TODO implement other case

		float		distanceToMove = -(Definition.FrontCouplerOffset + Definition.Bogies[0].DistanceToFront - Definition.Bogies[0].Wheelsets[0].SignedDistanceToBogie - previousCar.Definition.RearCouplerOffset + previousCar.Definition.Length - previousCar.Definition.Bogies[0].DistanceToFront + previousCar.Definition.Bogies[0].Wheelsets[0].SignedDistanceToBogie);
		float		segmentT = previousCar.FrontSegmentT;
		int			tDirection = previousCar.FrontSegmentTDirection;
		RailSegment segment = previousCar.FrontSegment.MoveSignedDistance(distanceToMove, ref segmentT, ref tDirection);
		PutOnTrack(segment, segmentT, tDirection == previousCar.FrontSegmentTDirection);
	}



	public void ToggleWipers()
	{
		foreach (ShapeAnimationNode node in m_wiperAnimations)
		{
			if (m_wipersOn)
			{
				node.PlayWrapMode = ShapeAnimationNode.WrapMode.Once;
			}
			else
			{
				node.Playing = true;
				node.PlayWrapMode = ShapeAnimationNode.WrapMode.Loop;
			}
		}
		m_wipersOn = !m_wipersOn;
	}



	public void TogglePantographs()
	{
		foreach (ShapeAnimationNode node in m_pantoAnimations)
		{
			node.PlayWrapMode = ShapeAnimationNode.WrapMode.Once;
			node.Reverse = m_pantosUp;
			node.Playing = true;
		}
		m_pantosUp = !m_pantosUp;
		
	}



	public void ToggleMirrors()
	{
		foreach (ShapeAnimationNode node in m_mirrorAnimations)
		{
			node.PlayWrapMode = ShapeAnimationNode.WrapMode.Once;
			node.Reverse = m_mirrorsOut;
			node.Playing = true;
		}
		m_mirrorsOut = !m_mirrorsOut;
		
	}



	public void Decouple(bool fromPrevious, bool partial)
	{
		if (fromPrevious)
		{
			if (Previous == null)
			{
				return;
			}
			if (partial)
			{
				PreviousCoupleType = CoupleType.PushOnly;
				Previous.NextCoupleType = CoupleType.PushOnly;
			}
			else
			{
				Previous.Next = null;
				Previous.NextCoupleType = CoupleType.None;
				Previous = null;
				PreviousCoupleType = CoupleType.None;
			}
		}
		else
		{
			if (Next == null)
			{
				return;
			}
			if (partial)
			{
				NextCoupleType = CoupleType.PushOnly;
				Next.PreviousCoupleType = CoupleType.PushOnly;
			}
			else
			{
				Next.Previous = null;
				Next.PreviousCoupleType = CoupleType.None;
				Next = null;
				NextCoupleType = CoupleType.None;
			}
		}
	}



	public void Couple(bool toPrevious, TrainCar otherTrainCar, bool otherToPrevious)
	{
		CoupleTo(otherTrainCar, toPrevious);
		otherTrainCar.CoupleTo(this, otherToPrevious);
	}



	private void CoupleTo(TrainCar otherTrainCar, bool toPrevious)
	{
		// Only updates this traincar, not the other
		if (toPrevious)
		{
			Debug.Assert(Previous == null);
			Previous = otherTrainCar;
			PreviousCoupleType = CoupleType.Full;
		}
		else
		{
			Debug.Assert(Next == null);
			Next = otherTrainCar;
			NextCoupleType = CoupleType.Full;
		}
	}


	public virtual void TimeStepUpdate(float deltaTime)
	{
		// Rolling resistance force
		const float RollingResistanceFactor = 0.02f;
		float	rollingResistanceForce = 0;
		if (m_velocity != 0)
		{
			rollingResistanceForce = -Math.Sign(m_velocity) * RollingResistanceFactor;
		}
		AddLocalForce(rollingResistanceForce);


		float acceleration = m_localForce / m_mass;
		float deltaPosition = m_velocity * deltaTime + 0.5f * acceleration * deltaTime * deltaTime;
		float velocity = m_velocity + acceleration * deltaTime;

		// Insert frame of standing still when speed changes sign
		if (velocity < 0 && m_velocity > 0 || velocity > 0 && m_velocity < 0)
		{
			m_velocity = 0 ;
		}
		else
		{
			m_velocity = velocity;
		}

		bool crossedEndOfTrack = false;
		m_bogies[0].TimeStepUpdate(deltaPosition);
		if (m_bogies[0].FirstWheelsetSegment == null || m_bogies[0].LastWheelsetSegment == null)
		{
			crossedEndOfTrack = true;
		}
		for (int bogieIndex = 1; bogieIndex < m_bogies.Length; bogieIndex++)
		{
			Bogie bogie = m_bogies[bogieIndex];
			bogie.TimeStepUpdate(m_bogies[bogieIndex - 1], deltaPosition);
			if (bogie.FirstWheelsetSegment == null || bogie.LastWheelsetSegment == null)
			{
				crossedEndOfTrack = true;
			}
		}
		if (crossedEndOfTrack)
		{
			for (int bogieIndex = 0; bogieIndex < m_bogies.Length; bogieIndex++)
			{
				m_bogies[bogieIndex].RestorePreviousParameters();
				Debug.Assert(m_bogies[bogieIndex].LastWheelsetSegment != null);
			}
			m_velocity = 0;
		}

		foreach (ShapeAnimation animation in m_animations)
		{
			animation.TimeStepUpdate(deltaTime);
		}

		m_localForce = 0;
	}



	public void FrameStepUpdate()
	{
		int fixedBogieIndex = -1;
		int index = 0;
		foreach (Bogie bogie in m_bogies)
		{
			bogie.FrameStepUpdate(m_debugWheelsetMarkers[index]);
//				bogie.FrameStepUpdate(null);
			if (bogie.Definition.Fixed)
			{
				fixedBogieIndex = index;
			}
			index++;
		}
		if (m_bogies.Length == 1)
		{
			fixedBogieIndex = 0;
		}

		if (fixedBogieIndex >= 0)
		{
			Matrix translation = Matrix.CreateTranslation(-m_bogies[fixedBogieIndex].Definition.Translation);
			m_bodyTransformNode.Transform = translation * m_bogies[fixedBogieIndex].Transform.Transform;
		}
		else
		{
			Bogie firstBogie = m_bogies[0];
			Bogie lastBogie = m_bogies[m_bogies.Length - 1];
			Vector3 firstBogiePosition = firstBogie.Transform.Translation;
			Vector3 lastBogiePosition = lastBogie.Transform.Translation;

			Vector3 firstToLastBogieDirection = lastBogiePosition - firstBogiePosition;
			firstToLastBogieDirection.Normalize();

			Vector3 trainCarPosition = firstBogiePosition + firstToLastBogieDirection * firstBogie.Definition.Translation.X;
			trainCarPosition.Y -= firstBogie.Definition.Translation.Y;

			Matrix transform = Matrix.CreateRotationY((float) -Math.Atan2(-firstToLastBogieDirection.Z, -firstToLastBogieDirection.X));
			transform.Translation = trainCarPosition;
			m_bodyTransformNode.Transform = transform;
		}

		float normalizedTime = m_animations.Count > 0 ? GetWheelsetRotation(m_animations[0]) / (float) (2 * Math.PI) : 0;  // TODO the wheelset that drives the animation is constant and can be set in the contructor
		foreach (ShapeAnimationNode animationNode in m_driveTrainAnimations)
		{
			animationNode.NormalizedTime = normalizedTime;
		}
		foreach (ShapeAnimation animation in m_animations)
		{
			animation.FrameStepUpdate();
		}

		Debug.Assert(!Reversed);
		Colour frontColour = Previous == null ? Colour.Grey50Percent : PreviousCoupleType == CoupleType.Full ? Colour.PureGreen : Colour.Cyan;
		Colour rearColour = Next == null ? Colour.Grey50Percent : NextCoupleType == CoupleType.Full ? Colour.PureGreen : Colour.Cyan;
		m_debugFrontCouplerMarkerMaterial.Ambient = frontColour;
		m_debugFrontCouplerMarkerMaterial.Diffuse = frontColour;
		m_debugRearCouplerMarkerMaterial.Ambient = rearColour;
		m_debugRearCouplerMarkerMaterial.Diffuse = rearColour;
		m_debugFrontCouplerMarker.Translation = Vector3.UnitY + GetFrontCouplerPositionInWorldSpace();
		m_debugRearCouplerMarker.Translation = Vector3.UnitY + GetRearCouplerPositionInWorldSpace();
	}



	public bool ContainsNode(GeometryNode node)
	{
		return GroupNode.IsAncestorOf(node);
	}



	public void AddCouplerForces(bool atFront)
	{
		TrainCar otherCar = (atFront ? Previous : Next);
		
		if (otherCar == null)
		{
			return;
		}

		if (otherCar.ID < ID)
			return;
		
		bool	otherAtFront = (otherCar.Previous == this ? true : false);
		Vector3	frontInWorldSpace = GetFrontCouplerPositionInWorldSpace();
		Vector3	rearInWorldSpace = GetRearCouplerPositionInWorldSpace();
		Vector3	directionInWorldSpace = frontInWorldSpace - rearInWorldSpace; 
		Vector3	springAttach1 = atFront ? frontInWorldSpace : rearInWorldSpace;
		Vector3	springAttach2 = otherAtFront ? otherCar.GetFrontCouplerPositionInWorldSpace() : otherCar.GetRearCouplerPositionInWorldSpace();
		float	springLength = (springAttach2 - springAttach1).Length();
		bool	otherCarInSameDirection	= atFront ^ otherAtFront;

		if (Vector3.Dot(directionInWorldSpace, springAttach2 - springAttach1) < 0)
		{
			springLength = -springLength;
		}

		float deltaVelocity = m_velocity;
		if (otherCarInSameDirection)
		{
			deltaVelocity -= otherCar.m_velocity;
		}
		else
		{
			deltaVelocity += otherCar.m_velocity;
		}

		CoupleType coupleType = atFront ? m_coupleTypeAtFront : m_coupleTypeAtRear;
		CoupleType otherCoupleType = otherAtFront ? otherCar.m_coupleTypeAtFront : otherCar.m_coupleTypeAtRear;
		Debug.Assert(coupleType == otherCoupleType);
		const float A = 2.5f;
		float springForce	= springLength * A;
		float damperForce	= -deltaVelocity * (A / 4);

		float totalForce = 0;
		if (Math.Sign (springForce + damperForce) == Math.Sign (springForce))
		{
			totalForce = springForce + damperForce;
		}

		if (coupleType == CoupleType.PushOnly)
		{
			if (atFront && totalForce > 0 || !atFront && totalForce < 0)
			{
				totalForce = 0;
			}
		}
		
		AddLocalForce (totalForce);
		otherCar.AddLocalForce (otherCarInSameDirection ? -totalForce : totalForce);
	}



	public Vector3 GetPreviousCouplerPositionInWorldSpace()
	{
		return Reversed ? GetRearCouplerPositionInWorldSpace() : GetFrontCouplerPositionInWorldSpace();
	}

		
	
	public Vector3 GetNextCouplerPositionInWorldSpace()
	{
		return Reversed ? GetFrontCouplerPositionInWorldSpace() : GetRearCouplerPositionInWorldSpace();
	}


	private Vector3 GetFrontCouplerPositionInWorldSpace()
	{
		Bogie firstBogie = m_bogies[0];
		Vector3 firstWheelsetPosition = firstBogie.FirstWheelsetSegment.GetCurvePositionInWorldSpace(firstBogie.FirstWheelsetSegmentT);
		Vector3 firstWheelsetDirection = firstBogie.FirstWheelsetSegment.GetCurveTangentInWorldSpace(firstBogie.FirstWheelsetSegmentT);
		if (firstBogie.FirstWheelsetDirectionOfT == -1)
		{
			firstWheelsetDirection = -firstWheelsetDirection;
		}
		return firstWheelsetPosition + (firstBogie.Definition.DistanceToFront - firstBogie.Definition.Wheelsets[0].SignedDistanceToBogie + Definition.FrontCouplerOffset) * firstWheelsetDirection;
		
	}


	private Vector3 GetRearCouplerPositionInWorldSpace()
	{
		Bogie lastBogie = m_bogies[m_bogies.Length - 1];
		Vector3 lastWheelsetPosition = lastBogie.LastWheelsetSegment.GetCurvePositionInWorldSpace(lastBogie.LastWheelsetSegmentT);
		Vector3 lastWheelsetDirection = lastBogie.LastWheelsetSegment.GetCurveTangentInWorldSpace(lastBogie.LastWheelsetSegmentT);
		if (lastBogie.LastWheelsetDirectionOfT == -1)
		{
			lastWheelsetDirection = -lastWheelsetDirection;
		}
		return lastWheelsetPosition - (lastBogie.Definition.DistanceToRear + lastBogie.Definition.Wheelsets[lastBogie.Definition.Wheelsets.Count - 1].SignedDistanceToBogie - Definition.RearCouplerOffset) * lastWheelsetDirection;
	}
		
		

	private Vector3 GetRearPositionInWorldSpace()
	{
		Bogie lastBogie = m_bogies[m_bogies.Length - 1];
		Vector3 lastWheelsetPosition = lastBogie.LastWheelsetSegment.GetCurvePositionInWorldSpace(lastBogie.LastWheelsetSegmentT);
		Vector3 lastWheelsetDirection = lastBogie.LastWheelsetSegment.GetCurveTangentInWorldSpace(lastBogie.LastWheelsetSegmentT);
		if (lastBogie.LastWheelsetDirectionOfT == -1)
		{
			lastWheelsetDirection = -lastWheelsetDirection;
		}
		return lastWheelsetPosition - (lastBogie.Definition.DistanceToRear + lastBogie.Definition.Wheelsets[lastBogie.Definition.Wheelsets.Count - 1].SignedDistanceToBogie) * lastWheelsetDirection;
	}



	private Vector3 GetFrontPositionInWorldSpace()
	{
		Bogie firstBogie = m_bogies[0];
		Vector3 firstWheelsetPosition = firstBogie.FirstWheelsetSegment.GetCurvePositionInWorldSpace(firstBogie.FirstWheelsetSegmentT);
		Vector3 firstWheelsetDirection = firstBogie.FirstWheelsetSegment.GetCurveTangentInWorldSpace(firstBogie.FirstWheelsetSegmentT);
		if (firstBogie.FirstWheelsetDirectionOfT == -1)
		{
			firstWheelsetDirection = -firstWheelsetDirection;
		}
		return firstWheelsetPosition + (firstBogie.Definition.DistanceToFront - firstBogie.Definition.Wheelsets[0].SignedDistanceToBogie) * firstWheelsetDirection;
	}



	private bool m_wipersOn;
	private bool m_pantosUp;
	private bool m_mirrorsOut;
}

}

*/

}