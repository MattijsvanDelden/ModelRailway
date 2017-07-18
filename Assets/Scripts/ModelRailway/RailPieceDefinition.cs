using System.Collections.Generic;
using UnityEngine;


namespace ModelRailway
{

public class RailPieceDefinition
{
	protected RailGeometryCreationParameters m_geometryDescription;

	protected GameObject GameObject { get; set; }

	public RailSegment[] RailSegments;

	public string Name { get; protected set; }

	public bool IsFlexible { get; protected set; }

	public float RailTopHeight { get { return m_geometryDescription.RailTopHeight; } }



	public RailPieceDefinition(string name, bool flexible, RailSegment[] railSegments, RailGeometryCreationParameters description)
	{
		m_geometryDescription	= description;
		RailSegments			= railSegments;
		Name					= name;
		IsFlexible				= flexible;

		GameObject = new GameObject(Name);
		GameObject.AddComponent<MeshFilter>().mesh = new Mesh();
		GameObject.AddComponent<MeshRenderer>().material = description.Material;
	}



	public void CreateGeometry(RailSegment[] railSegments)
	{
		var curveTangents = new List<Vector3>[railSegments.Length];
		var curvePositions = new List<Vector3>[railSegments.Length];
		int crossSectionVertexCount = m_geometryDescription.CrossSectionPositions.Length;
		int vertexCount = 0;
		int triangleCount = 0;
		for (int k = 0; k < railSegments.Length; k++)
		{
			BezierCurve3 curve = railSegments[k].Curve;
			curveTangents[k] = new List<Vector3>();
//			curvePositions[k] = curve.GetAngleApproximationWithTangents(0.0025f, curveTangents[k]);
			curvePositions[k]= curve.GetDistanceApproximationWithTangents(0.05f, curveTangents[k]);
			vertexCount += crossSectionVertexCount * curvePositions[k].Count;
			triangleCount += 2 * (curvePositions[k].Count - 1) * (crossSectionVertexCount - 1);
		}

		var positions	= new Vector3[vertexCount];
		var normals		= new Vector3[vertexCount];
		var uvs			= new Vector2[vertexCount];
		var indices		= new int[3 * triangleCount];

		int vertexIndex = 0;
		int indexIndex = 0;
		for (int k = 0; k < railSegments.Length; k++)
		{
			int processedVertexCount = vertexIndex;
			float currentU = 0;
			for (int crossSectionIndex = 0; crossSectionIndex < curvePositions[k].Count; crossSectionIndex++)
			{
				Vector3 point = curvePositions[k][crossSectionIndex];
				Vector3 tangent = curveTangents[k][crossSectionIndex];
				Matrix4x4 transform =  MatrixHelpers.CreateLookAt(point, tangent, Vector3.up);
				for (int crossSectionVertexIndex = 0; crossSectionVertexIndex < crossSectionVertexCount; crossSectionVertexIndex++)
				{
					Vector2 position2D = m_geometryDescription.CrossSectionPositions[crossSectionVertexIndex];
					var positionLocal = new Vector3(0, position2D.y, -position2D.x);
					Vector2 d ;
					if (crossSectionVertexIndex < crossSectionVertexCount - 1)
					{
						Vector2 nextPosition2D = m_geometryDescription.CrossSectionPositions[crossSectionVertexIndex + 1];
						if (nextPosition2D == position2D)
						{
							// Normal is 'plane' normal, i.e. only depends on line segment
							Vector2 previousPosition2D = m_geometryDescription.CrossSectionPositions[crossSectionVertexIndex - 1];
							d = position2D - previousPosition2D;
						}
						else
						{
							d = nextPosition2D - position2D;
							if (crossSectionVertexIndex > 0)
							{
								// Normal is 'vertex' normal, i.e. the average of the line segment and previous line segment
								Vector2 previousPosition2D = m_geometryDescription.CrossSectionPositions[crossSectionVertexIndex - 1];
								d += position2D - previousPosition2D;
							}
						}
					}
					else
					{
						d = position2D - m_geometryDescription.CrossSectionPositions[crossSectionVertexIndex - 1];
					}					
					var normalLocal = new Vector3(0, d.x, d.y);
					normalLocal.Normalize();

					Vector3 positionGlobal	= transform.MultiplyPoint(positionLocal);
					Vector3 normalGlobal	= transform.MultiplyVector(normalLocal);
					positions[vertexIndex]	= positionGlobal;
					normals[vertexIndex]	= normalGlobal;
					uvs[vertexIndex]		= new Vector2(currentU, m_geometryDescription.CrossSectionVs[crossSectionVertexIndex]);

					vertexIndex++;
				}
				if (crossSectionIndex < curvePositions[k].Count - 1)
				{
					Vector3 step = curvePositions[k][crossSectionIndex + 1] - curvePositions[k][crossSectionIndex];
					currentU += m_geometryDescription.DeltaUPerMetre * step.magnitude;
				}
			}
			for (int crossSectionIndex = 0 ; crossSectionIndex < curvePositions[k].Count - 1 ; crossSectionIndex++)
			{
				int startIndex = processedVertexCount + crossSectionIndex * m_geometryDescription.CrossSectionPositions.Length;
				for (int crossSectionVertexIndex = 0; crossSectionVertexIndex < crossSectionVertexCount - 1; crossSectionVertexIndex++)
				{
					indices[indexIndex + 0] = startIndex + crossSectionVertexIndex;
					indices[indexIndex + 1] = startIndex + crossSectionVertexIndex + 1;
					indices[indexIndex + 2] = startIndex + crossSectionVertexCount + crossSectionVertexIndex;

					indices[indexIndex + 3] = startIndex + crossSectionVertexIndex + 1;
					indices[indexIndex + 4] = startIndex + crossSectionVertexCount + crossSectionVertexIndex + 1;
					indices[indexIndex + 5] = startIndex + crossSectionVertexCount + crossSectionVertexIndex;

					indexIndex += 6;
				}
			}
		}

		Mesh mesh = GameObject.GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices	= positions;
		mesh.normals	= normals;
		mesh.uv			= uvs;
		mesh.triangles	= indices;
	}



	public GameObject GetPrefab()
	{
		// The mesh geometry is only created when the definition is 
		// 'used' for the first time (i.e. this method is called)

		if (!m_createdGeometry)
		{
			CreateGeometry(RailSegments);
			m_createdGeometry = true;
		}
		return GameObject;
	}



	private bool m_createdGeometry;
}

}
