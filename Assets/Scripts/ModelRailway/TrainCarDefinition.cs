using System.Collections.Generic;
using UnityEngine;



namespace ModelRailway
{

public class TrainCarDefinition : MonoBehaviour
{
	public GameObject		Visual;
	public ShapeAnimation[]	Animations;
	public bool				IsEngine;
	
	public BogieDefinition[] Bogies;	// Bogies, ordered from front to back of train car (i.e. along negative train car X axis)
}



/*

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;



namespace ModelRailway
{
	public class TrainCarDefinition
	{
		public string Name { get; private set; }

		public string FileName { get; private set; }

		public GameObject GroupNode { get; protected set; }

		public Bounds BoundingBox { get; private set; }

		public float Length { get; protected set; }

		public float OriginToFrontX { get; protected set; }

		public float LowestY { get; protected set; }

		public bool IsLoaded { get { return GroupNode != null; } }

		public float WheelRadius { get; protected set; }

		public float EngineWheelRadius { get; protected set; }

		public float FrontCouplerOffset { get; protected set; }

		public float RearCouplerOffset { get; protected set; }

		public List<ShapeAnimation> Animations { get; protected set; }



		public TrainCarDefinition(string name, string fileName, bool engine)
		{
			Name = name;
			FileName = fileName;
			Engine = engine;
		}



		public bool Load()
		{
			if (!File.Exists(FileName))
			{
				Debug.LogError("File '" + FileName + "' does not exist");
				return false; 
			}

			WagonDescription wagonDescription = null;
			EngineDescription engineDescription = null;
			var loader = new WagonEngineDescriptionLoader();
			if (FileName.EndsWith(".eng", true, CultureInfo.CurrentCulture) || FileName.EndsWith(".wag", true, CultureInfo.CurrentCulture))
			{
				loader.Load(FileName, out wagonDescription, out engineDescription);
			}

			string shapeFileName = wagonDescription != null ? Path.GetDirectoryName(FileName) + "/" + wagonDescription.ShapeFileName : FileName;
			Node node;

//			try
//			{

				List<ShapeAnimation> animations;
				node = Node.Load(shapeFileName, out animations);
				Animations = animations;

//			}
//			catch (Exception)
//			{
//				XNAGame.Error("Exception loading file '{0}'", shapeFileName);
//				return false;
//			}

			if (node == null)
			{
				Debug.LogError("Error loading file '" + shapeFileName + "'");
				return false;
			}

			Node freightNode = null;
			if (wagonDescription != null && wagonDescription.FreightAnimShapeFileName != null)
			{
				string freightAnimShapeFileName = Path.GetDirectoryName(FileName) + "/" + wagonDescription.FreightAnimShapeFileName;
				try
				{
					List<ShapeAnimation> freightAnimations;
					freightNode = Node.Load(freightAnimShapeFileName, out freightAnimations);
					Debug.Assert(freightAnimations == null || freightAnimations.Count == 0);
				}
				catch (Exception)
				{
					Debug.LogError("Exception loading file '" + freightAnimShapeFileName + "'");
					return false;
				}
			}

			EngineWheelRadius	= engineDescription != null ? engineDescription.WheelRadius : 0;
			WheelRadius			= wagonDescription != null ? wagonDescription.WheelRadius : 0;
			FrontCouplerOffset	= wagonDescription != null ? wagonDescription.FrontCouplerOffset : 0;
			RearCouplerOffset	= wagonDescription != null ? wagonDescription.RearCouplerOffset : 0;

			float initialWheelsetRotation = wagonDescription != null ? wagonDescription.InitialWheelRotationAngle : 0;

			var transform = node as Transform;
			if (transform == null)
			{
				Debug.LogError("Train car definition has no transform");
				return false;
			}
			const string NewName = "BODYTRANSFORM";
			UpdateAnimationNames(transform.name, NewName);
			transform.name = NewName;

			// New groupnode as top node
			GroupNode = new GroupNode(Name);
			GroupNode.AddChild(transform);
			m_bodyTransform = transform;
//			Debug.Assert(transform.Transform.Translation == Vector3.Zero);

			if (freightNode != null)
			{
				var freightTransform = freightNode as Transform;
				if (freightTransform == null)
				{
					Debug.LogError("Train car definition's freight anim has no transform (ignoring freight anim)");
				}
				else
				{
					//Debug.Assert(freightTransform.Transform.Translation == Vector3.Zero);
					while (freightTransform.Children.Count > 0)
					{
						m_bodyTransform.AddChild(freightTransform.Children[0]);
					}
				}
			}

			// Make sure the car is pointed in the right direction (front towards +X).
			RotateDefinitionToCorrectOrientation();
			GroupNode.CalculateBoundingSphere();

			BoundingBox = transform.CalculateBoundingBox();

			OriginToFrontX = BoundingBox.Max.X;
			Length = BoundingBox.Max.X - BoundingBox.Min.X;

			// Get lowest Y coordinate on train car, to 
			// enable positioning it exactly on rail tops
			LowestY = BoundingBox.Min.Y;

			CreateBogieDefinitions(OriginToFrontX, initialWheelsetRotation);
			if (Bogies.Count == 0)
			{
				Debug.LogError("Train car has no bogies");
				GroupNode = null;
				return false;
			}

			// Make bogies siblings of body (Train Simulator has them as children
			// of the train car body)
			for (int k = 0; k < Bogies.Count; k++)
			{
				GroupNode.AddChild(transform.Find<Transform>("BOGIETRANSFORM" + k, false));
			}

			// Remove unwanted and unnecessary animations
			foreach (ShapeAnimation animation in Animations)
			{
				for (int nodeIndex = 0; nodeIndex < animation.Nodes.Count;)
				{
					ShapeAnimationNode animationNode = animation.Nodes[nodeIndex];
					if (animationNode.Controllers.Count == 0)
					{
						animation.Nodes.RemoveAt(nodeIndex);
						continue;
					}
					if (animationNode.Name.StartsWith("WHEELSETTRANSFORM"))
					{
						for (int controllerIndex = 0; controllerIndex < animationNode.Controllers.Count;)
						{
							ShapeAnimationController controller = animationNode.Controllers[controllerIndex];
							if (controller is ShapeAnimationLinearController)
							{
								animationNode.Controllers.RemoveAt(controllerIndex);
							}
							else
							{
								controllerIndex++;
							}
						}
					}
					nodeIndex++;
				}
			}

			return true;
		}



		private void CreateBogieDefinitions(float trainCarFrontX, float initialWheelsetRotation)
		{
			List<Transform> bogieTransforms = new List<Transform>();
			for (int bogieIndex = 0 ; bogieIndex < 10 ; bogieIndex++)
			{
				Transform bogieTransform = GetBogieTransform(bogieIndex);
				if (bogieTransform == null)
				{
					break;
				}
				if (!BogieHasWheels(bogieTransform))
				{
					continue;
				}
				bogieTransform.Transform.Translation -= new Vector3(0, LowestY, bogieTransform.Transform.Translation.Z);
				float distanceToFront = trainCarFrontX - bogieTransform.Transform.Translation.X;
				var bogieDefinition = new BogieDefinition(distanceToFront, Length - distanceToFront, bogieTransform, initialWheelsetRotation);

				CreateBogieWheelsetDefinitions(bogieDefinition, bogieTransform, bogieIndex);
				bogieDefinition.CalculateDistanceFromFirstToLastWheelset();

				if (WheelRadius != 0 && bogieDefinition.Wheelsets.Count > 0)
				{
					// TODO only if not driven by animations
					bogieDefinition.Transform.Translation -= new Vector3(0, bogieDefinition.Wheelsets[0].Radius - WheelRadius, 0);
				}

				// Insert bogie into list of bogies. This list is sorted from front to
				// back of the train car (i.e. sorted along negative X axis direction)
				// Note that we do not use List.Sort because we cannot easily create a
				// comparison function (the transform is not part of the bogie def)
				AddToSortedBogieList(bogieTransform, bogieDefinition, bogieTransforms);
			}

			Transform fixedBogieTransform;
			BogieDefinition fixedBogieDefinition = CreateFixedBogieDefinition(trainCarFrontX, out fixedBogieTransform, initialWheelsetRotation);
			if (fixedBogieDefinition != null)
			{
				fixedBogieDefinition.CalculateDistanceFromFirstToLastWheelset();
				AddToSortedBogieList(fixedBogieTransform, fixedBogieDefinition, bogieTransforms);
			}

			// Now that we have a sorted list of bogie definitions and wheelsets, set correct names
			for (int bogieIndex = 0 ; bogieIndex < Bogies.Count ; bogieIndex++)
			{
				BogieDefinition bogie = Bogies[bogieIndex];
				string newName = "BOGIETRANSFORM" + bogieIndex;
				UpdateAnimationNames(bogie.Transform.Name, newName);
				bogie.Transform.Name = newName;
				for (int wheelsetIndex = 0 ; wheelsetIndex < bogie.Wheelsets.Count ; wheelsetIndex++)
				{
					Transform Transform = bogie.Wheelsets[wheelsetIndex].Transform;
					newName = "WHEELSETTRANSFORM" + bogieIndex + wheelsetIndex;
					UpdateAnimationNames(Transform.Name, newName);
					Transform.Name = newName;
				}
			}
		}




		private void UpdateAnimationNames(string oldName, string newName)
		{
			foreach (ShapeAnimation animation in Animations)
			{
				foreach (ShapeAnimationNode animationNode in animation.Nodes)
				{
					if (animationNode.Name == oldName)
					{
						animationNode.Name = newName;
					}
				}
			}
		}



		private static bool BogieHasWheels(Transform bogieTransform)
		{
			return bogieTransform.Find<Transform>("WHEEL*", false) == null ? false : true;
		}



		private void AddToSortedBogieList(Transform bogieTransform, BogieDefinition bogie, List<Transform> bogieTransforms)
		{
			int bogieInsertIndex = Bogies.Count;
			for (int otherBogieIndex = 0 ; otherBogieIndex < Bogies.Count ; otherBogieIndex++)
			{
				if (bogieTransform.Transform.Translation.X > bogieTransforms[otherBogieIndex].Transform.Translation.X)
				{
					bogieInsertIndex = otherBogieIndex;
					break;
				}
			}
			Bogies.Insert(bogieInsertIndex, bogie);
			bogieTransforms.Insert(bogieInsertIndex, bogieTransform);
		}



		private Transform GetBogieTransform(int bogieIndex)
		{
			string bogieTransformName = String.Format("BOGIE*{0}*", bogieIndex + 1);
			Transform bogieTransform = m_bodyTransform.Find<Transform>(bogieTransformName, false);
			if (bogieTransform == null)
			{
				return null;

				// No bogie transform node. is there a meta model?
//				string bogieMetaName = String.Format("meta_BOGIE*{0}*", bogieIndex + 1);
//				GeometryNode bogieMeta = Transform.Find<GeometryNode>(bogieMetaName);
//				Debug.Assert(bogieMeta != null, "Meta geometry must exist if bogie has no transform");
//				bogieGroup.TranslateGeometry(-bogieMeta.BoundingSphere.Mid);

				// Create bogie transform node and insert
//				bogieTransform = new Transform(String.Format("bogie{0}Transform", bogieIndex));
//				Transform.ReplaceChild(bogieTransform, bogieGroup);
//				bogieTransform.AddChild(bogieGroup);
//				bogieTransform.Transform.Translation = bogieMeta.BoundingSphere.Mid;

			}
			return bogieTransform;
		}



		private void AddFixedBogieWheelsets(Node node, List<WheelsetDefinition> wheelsets)
		{
			foreach (Node childNode in node.Children)
			{
				if (!childNode.Name.ToUpper().StartsWith("WHEEL") || !(childNode is Transform))
				{
					// Wheelsets do not have to be direct children of body xform, so recurse down
					if (!childNode.Name.ToUpper().StartsWith("BOGIE"))
					{
						AddFixedBogieWheelsets(childNode, wheelsets);		
					}
					continue;
				}

				Transform wheelsetTransform = childNode as Transform;
				if (wheelsetTransform.Translation.Y > LowestY + 1.5f)
				{
					// Wheels in the air? Ignore. At least one model 'uses' wheels to show
					// spinning fans in its roof (G2000-RTB.s)
					continue;
				}

				var wheelsetDefinition = new WheelsetDefinition(GetWheelsetRadius(wheelsetTransform), wheelsetTransform.Translation.X, wheelsetTransform);

				// Insert wheelset into list of wheelsets. This list is sorted from front to
				// back of the train car (i.e. sorted along negative X axis direction)
				// Note that we do not use List.Sort because we cannot easily create a
				// comparison function (the transform is not part of the wheelset def)
				AddToSortedWheelsetList(wheelsets, wheelsetDefinition);
			}
		}
		


		private BogieDefinition CreateFixedBogieDefinition(float trainCarFrontX, out Transform bogieTransform, float initialWheelsetRotation)
		{
			var wheelsets = new List<WheelsetDefinition>();

			AddFixedBogieWheelsets(m_bodyTransform, wheelsets);

			if (wheelsets.Count == 0)
			{
				// Train car has no 'fixed' bogie
				bogieTransform = null;
				return null;
			}

			// Set bogie position to be between the first and last wheels, so that fixed bogie
			// sorts properly with respect to other bogies
			Vector3 bogieTranslation = 0.5f * (wheelsets[0].Transform.Translation + wheelsets[wheelsets.Count - 1].Transform.Translation);
			bogieTranslation.Y = 0;
			bogieTranslation.Z = 0;

			bogieTransform = new Transform { Translation = bogieTranslation };
			m_bodyTransform.AddChild(bogieTransform);

			float distanceToFront = trainCarFrontX - bogieTranslation.X;
			BogieDefinition fixedBogie = new BogieDefinition(distanceToFront, Length - distanceToFront, bogieTransform, initialWheelsetRotation) 
			{
				Fixed = true, 
				Wheelsets = wheelsets
			};

			for (int wheelsetIndex = 0 ; wheelsetIndex < fixedBogie.Wheelsets.Count ; wheelsetIndex++)
			{
				Transform Transform = wheelsets[wheelsetIndex].Transform;
				if (Transform.Parent != m_bodyTransform)
				{
					// Wheelset is not direct child of body. There might be extra transform nodes
					// between the body transform and the wheelset transform;
					Node node = Transform.Parent;
					Matrix extraTransform = Matrix.Identity;
					while (node != m_bodyTransform)
					{
						Transform t = node as Transform;
						if (t != null)
						{
							extraTransform *= t.Transform;
						}
						node = node.Parent;
					}
					Transform.Transform *= extraTransform;
				}
				Transform.Translation -= bogieTranslation;
				fixedBogie.Wheelsets[wheelsetIndex].SignedDistanceToBogie -= bogieTranslation.X;
				bogieTransform.AddChild(Transform);
			}

			if (fixedBogie.Wheelsets.Count > 0)
			{
				// TODO only if not driven by animations
				float radius = EngineWheelRadius != 0 ? EngineWheelRadius : WheelRadius;
				fixedBogie.Transform.Translation -= new Vector3(0, fixedBogie.Wheelsets[0].Radius - radius , 0);
			}

			return fixedBogie;
		}



		private static void CreateBogieWheelsetDefinitions(BogieDefinition bogie, Node bogieGroup, int bogieIndex)
		{
			foreach (Node child in bogieGroup.Children)
			{
				if (child.Name.StartsWith("wheel", true, CultureInfo.CurrentCulture))
				{
					Debug.Assert(child is Transform);
					Transform wheelsetTransform = child as Transform;
					Debug.Assert(wheelsetTransform != null);

					if (child.Children.Count == 0)
					{
						// File 'us2bnsfsd40.s' has an empty wheelset (just a transform, no geometry)
						continue;
					}

					var wheelsetDefinition = new WheelsetDefinition(
						GetWheelsetRadius(wheelsetTransform),
					    wheelsetTransform.Transform.Translation.X, 
						wheelsetTransform);

					// Insert wheelset into list of wheelsets. This list is sorted from front to
					// back of the train car (i.e. sorted along negative X axis direction)
					// Note that we do not use List.Sort because we cannot easily create a
					// comparison function (the transform is not part of the wheelset def)
					AddToSortedWheelsetList(bogie.Wheelsets, wheelsetDefinition);
				}
			}
		}



		private static float GetWheelsetRadius(Transform wheelsetTransform)
		{
			Graphics.BoundingBox bbox = wheelsetTransform.CalculateBoundingBox();
			float diameter = Math.Min(bbox.Max.Y - bbox.Min.Y, bbox.Max.X - bbox.Min.X);
			return 0.5f * diameter;
		}



		private static void AddToSortedWheelsetList(
			List<WheelsetDefinition> wheelsetDefinitions, 
			WheelsetDefinition wheelset)
		{
			int wheelsetInsertIndex = wheelsetDefinitions.Count;
			for (int otherWheelsetIndex = 0 ; otherWheelsetIndex < wheelsetDefinitions.Count ; otherWheelsetIndex++)
			{
				if (wheelset.Transform.Translation.X > wheelsetDefinitions[otherWheelsetIndex].Transform.Translation.X)
				{
					wheelsetInsertIndex = otherWheelsetIndex;
					break;
				}
			}
			wheelsetDefinitions.Insert(wheelsetInsertIndex, wheelset);
		}



		private void RotateDefinitionToCorrectOrientation()
		{
			// Make sure the car is pointed in the right direction (front towards +X).
			// Cars from MSTS can be oriented +Z or -Z, so rotate the whole thing
			// so that it's pointing towards +X. 


			// We extract orientation from bogie positions
//			GeometryNode bogie1 = GroupNode.Find<GeometryNode>("BOGIE1");
//			Vector3 bogie1Mid = bogie1.BoundingSphere.Mid;
//			Matrix bogie1Transform = bogie1.GetWorldTransformation();
//			Vector3 bogie1MidGlobal = Vector3.Transform(bogie1Mid, bogie1Transform);

//			GeometryNode bogie2 = GroupNode.Find<GeometryNode>("BOGIE2");
//			Vector3 bogie2Mid = bogie2.BoundingSphere.Mid;
//			Matrix bogie2Transform = bogie2.GetWorldTransformation();
//			Vector3 bogie2MidGlobal = Vector3.Transform(bogie2Mid, bogie2Transform);

//			float angle;
//			if (bogie1MidGlobal.Z > bogie2MidGlobal.Z)
//			{
//				angle = -0.5f * (float) Math.PI;
//			}
//			else
//			{
//				angle = 0.5f * (float) Math.PI;
//			
//			}

			const float Angle = -0.5f * (float) Math.PI;
			GroupNode.RotateGeometry(0, Angle, 0);
			GroupNode.RotateTransforms(0, Angle, 0);

			foreach (ShapeAnimation animation in Animations)
			{
				animation.Rotate();
			}
		}

		private Transform m_bodyTransform;
	}
}

*/

}