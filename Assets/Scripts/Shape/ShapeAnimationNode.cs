using UnityEngine;


[System.Serializable]
public class ShapeAnimationNode
{
	public enum WrapMode
	{
		Once,
		Loop,
	}



	public bool							Playing;
	public WrapMode						PlayWrapMode;
	public bool							Reverse;
	public string						Name;
	public float						Length = 2;
	public ShapeAnimationController[]	Controllers;



	public float NormalizedTime { private get; set; }


	
	public ShapeAnimationNode(string name, float length)
	{
		PlayWrapMode = WrapMode.Loop;
		Name = name;
	}



	public void FrameStepUpdate()
	{
		foreach (ShapeAnimationController controller in Controllers)
		{
			controller.UpdateTransform(m_transformNode, NormalizedTime);
		}
	}



	public void SetTransform(Transform topNode)
	{
		m_transformNode = topNode.FindInTree(Name);
		Debug.Assert(m_transformNode != null, Name);
	}



	public void TimeStepUpdate(float deltaTime)
	{
		if (Playing)
		{
			if (Reverse)
			{
				NormalizedTime -= deltaTime / Length;
				if (NormalizedTime < 0)
				{
					if (PlayWrapMode == WrapMode.Once)
					{
						NormalizedTime = 0;
						Playing = false; 
					}
					else
					{
						while (NormalizedTime < 0)
						{
							NormalizedTime += 1;
						}
					}
				}
			}
			else
			{
				NormalizedTime += deltaTime / Length;
				if (NormalizedTime >= 1)
				{
					if (PlayWrapMode == WrapMode.Once)
					{
						NormalizedTime = 1;
						Playing = false; 
					}
					else
					{
						while (NormalizedTime >= 1)
						{
							NormalizedTime -= 1;
						}
					}
				}
			}
		}
	}


	private Transform m_transformNode;
}
