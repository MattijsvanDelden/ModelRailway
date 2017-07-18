using System;
using UnityEngine;



namespace ModelRailway
{

public class RailGeometryCreationParameters
{
	public Material				Material				{ get; private set; }

	public float				RailTopHeight			{ get; private set; }

	public readonly Vector2[]	CrossSectionPositions;

	public readonly float[]		CrossSectionVs;

	public float				DeltaUPerMetre			{ get; private set; }



	public RailGeometryCreationParameters(Material material, Vector2[] crossSectionPositions, float[] crossSectionVs, float deltaUPerMetre, float railTopHeight)
	{
		if (crossSectionPositions.Length != crossSectionVs.Length)
		{
			throw new ArgumentException("crossSectionPositions and crossSectionVs must be the same length");
		}
		Material				= material;
		CrossSectionPositions	= crossSectionPositions;
		CrossSectionVs			= crossSectionVs;
		RailTopHeight			= railTopHeight;
		DeltaUPerMetre			= deltaUPerMetre;
	}
}

}
