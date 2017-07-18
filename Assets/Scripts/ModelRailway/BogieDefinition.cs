using System;
using UnityEngine;



namespace ModelRailway
{

public class BogieDefinition : MonoBehaviour
{
	public WheelsetDefinition[] Wheelsets;



	public Transform TransformNode
	{
		get;
		private set;
	}

	
	
	public float DistanceToFront 		// Distance from bogie origin to front of train car
	{ 
		get; 
		private set; 	
	}



	public float DistanceToRear 
	{ 
		get; 
		private set; 
	}

	
	
	public float DistanceFromFirstToLastWheelset 
	{ 
		get; 
		private set; 
	}



	public Vector3 Translation 
	{ 
		get { return TransformNode.localPosition; } 
	}



	public bool Fixed 
	{ 
		get; 
		set; 
	}



	public float InitialWheelsetRotation { get; private set; }



	public BogieDefinition(float distanceToFront, float distanceToRear, Transform transformNode, float initialWheelsetRotation)
	{
		DistanceToFront					= distanceToFront;
		DistanceToRear					= distanceToRear;
		DistanceFromFirstToLastWheelset = 0;
		TransformNode					= transformNode;
		InitialWheelsetRotation			= initialWheelsetRotation;
		Fixed							= false;
	}



	public void CalculateDistanceFromFirstToLastWheelset()
	{
		DistanceFromFirstToLastWheelset = 
			Wheelsets.Length > 0 ? 
			Math.Abs(Wheelsets[Wheelsets.Length - 1].SignedDistanceToBogie - Wheelsets[0].SignedDistanceToBogie) : 0;
	}
}

}
