// Model railway in C# by Mattijs

using UnityEngine;



namespace ModelRailway
{
	public class WheelsetDefinition : MonoBehaviour
	{
		public readonly float		Radius;
		public readonly Transform	TransformNode;
		public float				SignedDistanceToBogie;		// Distance along X axis from wheelset centre to bogie centre (i.e. positive ditance means that wheelset centre is closer to front of train car than bogie centre)



		public WheelsetDefinition(float radius, float signedDistanceToBogie, Transform transformNode)
		{
			Radius					= radius;
			SignedDistanceToBogie	= signedDistanceToBogie;
			TransformNode			= transformNode;
		}
	}
}
