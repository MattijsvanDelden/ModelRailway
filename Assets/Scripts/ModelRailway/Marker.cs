using UnityEngine;



namespace ModelRailway
{

public class Marker
{
	public GameObject	GameObject			{ get; private set; }

	public RailSegment	RailSegment			{ get; set; }

	public int			ControlPointIndex	{ get; set; }



	public Marker(GameObject gameObject)
	{
		GameObject = gameObject;
	}
}

}
