using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class ShapeAnimation
{
	public ShapeAnimationNode[] Nodes;

	public float Length;



	public ShapeAnimation(float length)
	{
		Debug.Assert(length > 0);
		Length = length;
	}



	public ShapeAnimation(ShapeAnimation source, Transform topNode)
	{
		Length = source.Length;
		Nodes = new ShapeAnimationNode[source.Nodes.Length];

		// Nodes cannot be shared because of their m_transform member
		for (int k = 0; k < source.Nodes.Length; k++)
		{	
			var sourceNode = source.Nodes[k];
			var node = new ShapeAnimationNode(sourceNode.Name, Length);
			node.Controllers = new ShapeAnimationController[sourceNode.Controllers.Length];
			for (int l = 0; l < sourceNode.Controllers.Length; l++)
			{
				// Controllers can be shared
				node.Controllers[l] = sourceNode.Controllers[l];
			}
			Nodes[k] = node;
		}
		SetTransforms(topNode);
	}



	public ShapeAnimationNode AddNode(string name)
	{
		var node = new ShapeAnimationNode(name, Length);
		var nodes = Nodes != null ? new List<ShapeAnimationNode>(Nodes) : new List<ShapeAnimationNode>();
		nodes.Add(node);
		Nodes = nodes.ToArray();
		return node;
	}



	public void TimeStepUpdate(float deltaTime)
	{
		foreach (ShapeAnimationNode node in Nodes)
		{
			node.TimeStepUpdate(deltaTime);
		}
	}


	
	public void FrameStepUpdate()
	{
		foreach (ShapeAnimationNode node in Nodes)
		{
			node.FrameStepUpdate();
		}
	}



	public void SetTransforms(Transform topNode)
	{
		foreach (ShapeAnimationNode node in Nodes)
		{
			node.SetTransform(topNode);
		}
	}
}
