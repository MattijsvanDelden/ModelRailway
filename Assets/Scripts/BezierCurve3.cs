using System;
using System.Collections.Generic;
using UnityEngine;



public class BezierCurve3
{
	public Vector3[] ControlPoints;

	private Vector3[] m_controlPointFirstDifferences;

	public float Length { get; private set; }



	public BezierCurve3(Vector3[] controlPoints)
	{
		SetControlPoints(controlPoints);
	}



	public BezierCurve3(BezierCurve3 source)
	{
		SetControlPoints((Vector3[]) source.ControlPoints.Clone());
	}



	public void SetControlPoints(Vector3[] controlPoints)
	{
		if (controlPoints.Length != 4)
		{
			throw new ArgumentException("argument should contain exactly 4 Vector3 elements", "controlPoints");
		}
		
		ControlPoints = controlPoints;

		CalculateDerivedParameters();
	}



	private void CalculateDerivedParameters()
	{
		m_controlPointFirstDifferences = new Vector3[ControlPoints.Length - 1];
		for (int k = 0 ; k < ControlPoints.Length - 1; k++)
		{
			m_controlPointFirstDifferences[k] = ControlPoints[k + 1] - ControlPoints[k];
		}

		Length = GetLength(0, 1);
	}



	public void SetControlPoint(int index, Vector3 position)
	{
		ControlPoints[index] = position;
		CalculateDerivedParameters();
	}



	public Vector3 GetControlPoint(int index)
	{
		if (index < 0 || index >= 4)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ControlPoints[index];
	}



	public Vector3 GetPosition(float t)
	{
		if (t == 0)
		{
			return ControlPoints[0];
		}
		if (t == 1)
		{
			return ControlPoints[3];
		}

		float v = 1.0f - t ;

		float tSquared = t * t ;
		float tCubed = tSquared * t ;
		float vSquared = v * v ;
		float vCubed = vSquared * v ;

		float a = vCubed ;
		float b = 3 * t * vSquared ;
		float c = 3 * tSquared * v ;
		float d = tCubed ;

		return new Vector3
		(
			ControlPoints [0].x * a + ControlPoints [1].x * b + ControlPoints [2].x * c + ControlPoints [3].x * d,
			ControlPoints [0].y * a + ControlPoints [1].y * b + ControlPoints [2].y * c + ControlPoints [3].y * d,
			ControlPoints [0].z * a + ControlPoints [1].z * b + ControlPoints [2].z * c + ControlPoints [3].z * d
		);
	}



	public Vector3 GetFirstDerivative(float t)
	{
		float v = 1.0f - t ;
		float tSquared = t * t ;
		float vSquared = v * v ;
		float tv = t * v ;

		return new Vector3
		(
			3 * (vSquared * m_controlPointFirstDifferences [0].x + 2 * tv * m_controlPointFirstDifferences [1].x + tSquared * m_controlPointFirstDifferences [2].x),
			3 * (vSquared * m_controlPointFirstDifferences [0].y + 2 * tv * m_controlPointFirstDifferences [1].y + tSquared * m_controlPointFirstDifferences [2].y),
			3 * (vSquared * m_controlPointFirstDifferences [0].z + 2 * tv * m_controlPointFirstDifferences [1].z + tSquared * m_controlPointFirstDifferences [2].z)
		);
	}



	public Vector3 GetTangent(float t)
	{
		return Vector3.Normalize(GetFirstDerivative(t));
	}



	private int GetDistanceApproximationRecurse(float maxDistanceSquared, IList<Vector3> points, float t1, float t2, int pointIndex)
	{
		int insertedPointCount = 0 ;
		float tMid = 0.5f * (t1 + t2);
		Vector3 segmentStart = GetPosition(t1);
		Vector3 segmentEnd = GetPosition(t2);
		Vector3 curvePosition = GetPosition(tMid);
		float distanceSquared = curvePosition.SquaredDistanceToLine(segmentStart, segmentEnd);
		if (distanceSquared > maxDistanceSquared)
		{
			points.Insert(pointIndex + 1, curvePosition);
			insertedPointCount++;

			insertedPointCount += GetDistanceApproximationRecurse(maxDistanceSquared, points, t1, tMid, pointIndex);
			insertedPointCount += GetDistanceApproximationRecurse(maxDistanceSquared, points, tMid, t2, pointIndex + insertedPointCount);
		}
		return insertedPointCount;
	}



	public List<Vector3> GetDistanceApproximation(float maxDistance)
	{
		var points = new List<Vector3>();
		float maxDistanceSquared = maxDistance * maxDistance;

		points.Add(GetPosition(0));
		points.Add(GetPosition(1));

		GetDistanceApproximationRecurse(maxDistanceSquared, points, 0, 1, 0);

		return points;
	}



	private int GetDistanceApproximationWithTangentsRecurse(float maxDistanceSquared, IList<Vector3> points, IList<Vector3> tangents, float t1, float t2, int pointIndex)
	{
		int insertedPointCount = 0 ;
		float tMid = 0.5f * (t1 + t2);
		Vector3 segmentStart = GetPosition(t1);
		Vector3 segmentEnd = GetPosition(t2);
		Vector3 curvePosition = GetPosition(tMid);
		float distanceSquared = curvePosition.SquaredDistanceToLine(segmentStart, segmentEnd);
		if (distanceSquared > maxDistanceSquared)
		{
			points.Insert(pointIndex + 1, curvePosition);
			tangents.Insert(pointIndex + 1, GetTangent(tMid));
			insertedPointCount++;

			insertedPointCount += GetDistanceApproximationWithTangentsRecurse(maxDistanceSquared, points, tangents, t1, tMid, pointIndex);
			insertedPointCount += GetDistanceApproximationWithTangentsRecurse(maxDistanceSquared, points, tangents, tMid, t2, pointIndex + insertedPointCount);
		}
		return insertedPointCount;
	}



	public List<Vector3> GetDistanceApproximationWithTangents(float maxDistance, IList<Vector3> tangents)
	{
		var points = new List<Vector3>();
		float maxDistanceSquared = maxDistance * maxDistance;

		points.Add(GetPosition(0));
		tangents.Add(GetTangent(0));
		points.Add(GetPosition(1));
		tangents.Add(GetTangent(1));

		GetDistanceApproximationWithTangentsRecurse(maxDistanceSquared, points, tangents, 0, 1, 0);

		return points;
	}



	private int GetAngleApproximationWithTangentsRecurse(float cosMaxAngle, IList<Vector3> points, IList<Vector3> tangents, float t1, float t2, int pointIndex)
	{
		int insertedPointCount = 0 ;
		float tMid = 0.5f * (t1 + t2);
		Vector3 segmentStart = GetPosition(t1);
		Vector3 segmentEnd = GetPosition(t2);
		Vector3 curvePosition = GetPosition(tMid);
		Vector3 curveTangent = GetTangent(tMid);
		Vector3 segmentTangent = Vector3.Normalize(segmentEnd - segmentStart);
		float cosAngle = Vector3.Dot(curveTangent, segmentTangent);
		if (cosAngle < cosMaxAngle)
		{
			points.Insert(pointIndex + 1, curvePosition);
			tangents.Insert(pointIndex + 1, GetTangent(tMid));
			insertedPointCount++;

			insertedPointCount += GetAngleApproximationWithTangentsRecurse(cosMaxAngle, points, tangents, t1, tMid, pointIndex);
			insertedPointCount += GetAngleApproximationWithTangentsRecurse(cosMaxAngle, points, tangents, tMid, t2, pointIndex + insertedPointCount);
		}
		return insertedPointCount;
	}



	public List<Vector3> GetAngleApproximationWithTangents(float maxAngle, IList<Vector3> tangents)
	{
		var points = new List<Vector3>();

		points.Add(GetPosition(0));
		tangents.Add(GetTangent(0));
		points.Add(GetPosition(1));
		tangents.Add(GetTangent(1));

		float cosMaxAngle = (float) Math.Cos(maxAngle);
		GetAngleApproximationWithTangentsRecurse(cosMaxAngle, points, tangents, 0, 1, 0);

		return points;
	}



	public float GetTime(float position, int maximumIterationCount, float tolerance)
	{
		if (position <= 0)
		{
			return 0;
		}
		if (position >= Length)
		{
			return 1;
		}

		// initial guess for Newton's method
		float t = position / Length;

		for (int i = 0; i < maximumIterationCount; i++)
		{
			float difference = GetLength(0, t) - position;
			if (Math.Abs(difference) < tolerance)
			{
				return t;
			}

			t -= difference / GetSpeed(t);
			if (t < 0)
			{
				t = 0;
			}
			else if (t > 1)
			{
				t = 1;
			}
		}

		// Newton's method failed.  If this happens, increase iterations or
		// tolerance or integration accuracy.
		throw new Exception("GetTime failed to onverge");
	}



	public float GetSpeed(float t)
	{
		return GetFirstDerivative(t).magnitude;
	}



	public float GetLength(float startT, float endT)
	{
		// Note: code was nicked from WildMagic version 0.1, MgcIntegrate.cpp
		// This calculates a Romberg integral
		float fH = endT - startT;
		const int order = 5;
		float[,] romberg = new float[2, order];

		romberg [0, 0] = 0.5f * fH * (GetSpeed(startT) + GetSpeed(endT));
		for (int i0 = 2, iP0 = 1; i0 <= order; i0++, iP0 *= 2)
		{
			// Approximations via the trapezoid rule
			float fSum = 0;
			for (int i1 = 1; i1 <= iP0; i1++)
			{
				fSum += GetSpeed(startT + fH * (i1 - 0.5f));
			}

			// Richardson extrapolation
			romberg[1, 0] = 0.5f * (romberg[0, 0] + fH * fSum) ;
			for (int i2 = 1, iP2 = 4; i2 < i0; i2++, iP2 *= 4)
			{
				romberg[1, i2] = (iP2 * romberg[1, i2 - 1] - romberg[0, i2 - 1]) / (iP2 - 1);
			}

			for (int i1 = 0; i1 < i0; i1++)
			{
				romberg[0, i1] = romberg[1, i1];
			}
			fH *= 0.5f;
		}

		return romberg[0, order - 1];
	}
}
