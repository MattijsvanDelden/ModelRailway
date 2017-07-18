using UnityEngine;



namespace ModelRailway
{

public class RailPiece
{
	public static int FirstUnusedID = 1;

	public readonly RailSegment[] RailSegments;			// Copies of the segments in the definition (i.e. each rail piece has unique segment instances)


	public GameObject			GameObject		{ get; private set; }

	public int					ID				{ get; set; }

	public RailPieceDefinition	Definition		{ get; private set; }

	public float				RailTopHeight	{ get { return Definition.RailTopHeight; } }



	protected RailPiece(RailPieceDefinition definition, bool copyCurves, bool copyGeometry)
	{
		ID			= FirstUnusedID++;
		Definition	= definition;
		GameObject	= new GameObject("Rail piece " + ID);

		// Copy rail segments. Cannot share segments of definition,
		// because of next and previous fields in rail segments that must 
		// be different for each rail piece instance.
		RailSegments = new RailSegment[definition.RailSegments.Length];
		for (int k = 0; k < RailSegments.Length; k++)
		{
			RailSegments[k] = new RailSegment(definition.RailSegments[k], this, copyCurves);
		}

		GameObject = (GameObject) Object.Instantiate(definition.GetPrefab());
		if (copyGeometry)
		{
		}
	}



	public void SetTransformation(Vector3 position, Quaternion rotation)
	{
		GameObject.transform.localPosition = position;
		GameObject.transform.localRotation = rotation;
	}



	public void SetPosition(Vector3 position)
	{
		GameObject.transform.localPosition = position;
	}


	
	public Vector3 GetPosition()
	{
		return GameObject.transform.localPosition;
	}



	public void AddRailSegmentsToRailSystem(RailSystem railSystem)
	{
		foreach(RailSegment segment in RailSegments)
		{
			railSystem.AddRailSegment(segment);
		}
	}



	public void RemoveSegmentsFromRailSystem(RailSystem railSystem)
	{
		foreach (RailSegment segment in RailSegments)
		{
			railSystem.RemoveRailSegment(segment);
		}
	}
}
}
