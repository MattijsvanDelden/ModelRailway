using UnityEngine;



[System.Serializable]
public class ShapeAnimationController
{
	public int[]		KeyIndexes;
	public Quaternion[] RotationKeyFrames;
	public Vector3[]	TranslationKeyFrames;
	public bool			AbsoluteRotation;



	public ShapeAnimationController(int keyFrameCount, bool translation, bool rotation)
	{
		KeyIndexes				= new int[keyFrameCount];
		RotationKeyFrames		= rotation ? new Quaternion[keyFrameCount] : null;
		TranslationKeyFrames	= translation ? new Vector3[keyFrameCount] : null;
	}



	public void UpdateTransform(Transform transformNode, float normalizedTime)
	{
		float fraction;
		int keyFrameIndex = GetKeyframeIndex(normalizedTime, out fraction);

		if (TranslationKeyFrames != null && TranslationKeyFrames.Length > 0)
		{
			transformNode.localPosition = (1 - fraction) * TranslationKeyFrames[keyFrameIndex] + fraction * TranslationKeyFrames[keyFrameIndex + 1];
		}

		if (RotationKeyFrames != null && RotationKeyFrames.Length > 0)
		{
			Quaternion q = Quaternion.Slerp(RotationKeyFrames[keyFrameIndex], RotationKeyFrames[keyFrameIndex + 1], fraction);
			if (AbsoluteRotation)
			{
				Vector3 translation = transformNode.localPosition;
				transformNode.localRotation = q;
				transformNode.localPosition = translation;
			}
			else
			{
				transformNode.localRotation *= q;
			}
		}
	}



	//- PRIVATE -------------------------------------------------------------------------------------------



	private int GetKeyframeIndex(float normalizedTime, out float fraction)
	{
		if (normalizedTime >= 1)
		{
			normalizedTime = 0.99999f;
		}
		float keyFrameCount = KeyIndexes[KeyIndexes.Length - 1];
		int keyFrameIndex = 0;
		while (KeyIndexes[keyFrameIndex] / keyFrameCount  <= normalizedTime)
		{
			keyFrameIndex++;
		}
		fraction = normalizedTime - KeyIndexes[keyFrameIndex - 1] / keyFrameCount;
		fraction *= keyFrameCount / (KeyIndexes[keyFrameIndex] - KeyIndexes[keyFrameIndex - 1]);
		return keyFrameIndex - 1;
	}
}
