using UnityEngine;



namespace ModelRailway
{
	public enum WagonType
	{
		Engine,
		Tender,
		Freight,
		Carriage,
	}



	public class WagonDescription
	{
		public string		Name;
		public WagonType	Type;
		public string		ShapeFileName;
		public string		FreightAnimShapeFileName;
		public float		WheelRadius;
		public Vector3		Size;

		// Stuff not present in MSTS .wag files:

		public float		FrontCouplerOffset;
		public float		RearCouplerOffset;
		public float		InitialWheelRotationAngle;
	}
}
