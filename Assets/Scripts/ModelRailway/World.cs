// Model railway in C# by Mattijs
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/*

namespace ModelRailway
{
	class World
	{
		private const string FileHeader = "RailwayWorld";
		private const string RailPieceString = "RailPiece";
		private const string BackgroundString = "Background";
		private const string BackgroundFileNameString = "    FileName = ";
		private const string RailPieceDefinitionString = "    Definition = ";
		private const string RailPieceTransformString = "    Transform = ";
		private const string RailPieceControlPointsString = "    ControlPoints = ";
		private const string PointsIndicatorFileName = "Data/Geometry/PointsArrow.fsd";

		private string m_backgroundFileName; 
		private readonly List<PointsIndicator> m_pointsIndicators = new List<PointsIndicator>();
		private readonly GameObject m_pointsIndicatorModel;
		private readonly float m_pointsIndicatorLength;
		private RailPiece m_lastCreatedRailPiece;

		protected readonly RailSystem m_railSystem = new RailSystem();
		protected readonly Dictionary<int, RailPiece> m_railPieces = new Dictionary<int, RailPiece>();
		protected readonly Dictionary<string, RailPieceDefinition> m_railPieceDefinitions = new Dictionary<string, RailPieceDefinition>();
		protected readonly GameObject m_groupNode;

		protected GameObject m_backgroundNode;


	
//		public readonly List<TrainCar> TrainCars = new List<TrainCar>();

//		public readonly List<Engine> Engines = new List<Engine>();


		public bool ShowDebugInfo
		{
			set 
			{ 

//				foreach (TrainCar trainCar in TrainCars)
//				{
//					trainCar.ShowDebugInfo = value;
//				}

			}
		}

		public World(GameObject topnode, GameObject backgroundNode)
		{
			m_groupNode = topnode;
			m_backgroundNode = backgroundNode;
			m_backgroundFileName = null;
			if (m_backgroundNode != null)
			{
				m_backgroundNode.transform.parent = m_groupNode.transform;
			}

			Node node = Node.Load(PointsIndicatorFileName);
			var groupNode = node as GroupNode;
			Debug.Assert(groupNode != null);
			m_pointsIndicatorModel = groupNode.Children[0];
			Graphics.BoundingBox bbox = m_pointsIndicatorModel.CalculateBoundingBox();
			m_pointsIndicatorLength = bbox.Max.X - bbox.Min.X;
		}



		public RailPiece GetRailPiece(GameObject geometryNode)
		{
			return GetRailPiece(geometryNode.ID);
		}



		public RailPiece GetRailPiece(int id)
		{
			if (!m_railPieces.ContainsKey(id))
			{
				return null;
			}
			return m_railPieces[id];
		}



		public PointsIndicator GetPointsIndicator(GameObject geometryNode)
		{
			foreach (PointsIndicator indicator in m_pointsIndicators)
			{
				if (indicator.Model == geometryNode)
				{
					return indicator;
				}
			}
			return null;
		}

	
		
		public TrainCar GetTrainCar(GeometryNode geometryNode)
		{
			foreach (TrainCar trainCar in TrainCars)
			{
				if (trainCar.ContainsNode(geometryNode))
				{
					return trainCar;
				}
			}
			return null;
		}



		public void AddRailPieceDefinition(RailPieceDefinition definition)
		{
			m_railPieceDefinitions.Add(definition.Name, definition);
		}



		public virtual void CreateRailPiece(string definitionName, Vector3 position, Vector3 rotation, Vector3[] controlPoints)
		{
			if (!m_railPieceDefinitions.ContainsKey(definitionName))
			{
				return;
			}

			RailPieceDefinition definition = m_railPieceDefinitions[definitionName];
			RailPiece railPiece;
			if (definition.IsFlexible)
			{
				railPiece = new FlexibleRailPiece(definition, controlPoints);
			}
			else
			{
				railPiece = new RigidRailPiece(definition);
			}
			railPiece.SetTransformation(transformation);
			m_railPieces.Add(railPiece.ID, railPiece);
			m_groupNode.AddChild(railPiece.Transform);
			railPiece.AddRailSegmentsToRailSystem(m_railSystem);

			m_lastCreatedRailPiece = railPiece;
		}



		public RailPiece GetLastCreatedRailPiece()
		{
			return m_lastCreatedRailPiece;
		}



		public void AddObject(Node objectNode)
		{
			m_groupNode.AddChild(objectNode);
		}



		public void RemoveObject(Node objectNode)
		{
			m_groupNode.RemoveChild(objectNode);
		}

	
		
		public void AddTrain(TrainCar firstTrainCar, RailSegment segment, float segmentT, bool addDebugMarkers)
		{
			TrainCar currentTrainCar = firstTrainCar;
			TrainCar previousTrainCar = null;
			while (currentTrainCar != null)
			{
				TrainCars.Add(currentTrainCar);
				if (currentTrainCar is Engine)
				{
					Engines.Add(currentTrainCar as Engine);
				}
				if (currentTrainCar == firstTrainCar)
				{
					currentTrainCar.PutOnTrack(segment, segmentT, true);
				}
				else
				{
					currentTrainCar.PutOnTrackAfter(previousTrainCar);
				}
				m_groupNode.AddChild(currentTrainCar.GroupNode);
				if (addDebugMarkers)
				{
					Debug.Assert(false);

//					GroupNode debugMarkerGroup = new GroupNode();
//					m_groupNode.AddChild(debugMarkerGroup);
//					debugMarkerGroup.AddChild(trainCar.DebugBodyMarker);
//					for (int bogieIndex = 0; bogieIndex < trainCar.DebugBogieMarkers.Length; bogieIndex++)
//					{
//						debugMarkerGroup.AddChild(trainCar.DebugBogieMarkers[bogieIndex]);
//						for (int wheelsetIndex = 0; wheelsetIndex < 2; wheelsetIndex++)
//						{
//							debugMarkerGroup.AddChild(trainCar.DebugWheelsetMarkers[bogieIndex, wheelsetIndex]);
//						}
//					}
				}
				previousTrainCar = currentTrainCar;
				currentTrainCar = currentTrainCar.Next;
			}
		}



		public void RemoveTrain(TrainCar oneOfTheTraincars)
		{
			TrainCar currentCar = Train.GetFront(oneOfTheTraincars);
			while (currentCar != null)
			{
				TrainCar nextCar = currentCar.Next;
				RemoveTrainCar(currentCar);
				currentCar = nextCar;
			}


//			trainCar.DebugBodyMarker.Remove();
//			for (int bogieIndex = 0 ; bogieIndex < trainCar.DebugBogieMarkers.Length ; bogieIndex++)
//			{
//				trainCar.DebugBogieMarkers[bogieIndex].Remove();
//				for (int wheelsetIndex = 0 ; wheelsetIndex < 2 ; wheelsetIndex++)
//				{
//					trainCar.DebugWheelsetMarkers[bogieIndex, wheelsetIndex].Remove();
//				}
//			}

		}



		public void RemoveTrainCar(TrainCar traincar)
		{
			if (traincar == null)
			{
				return;
			}
//			traincar.Decouple(true, false);
//			traincar.Decouple(false, false);
			m_groupNode.RemoveChild(traincar.GroupNode);
			TrainCars.Remove(traincar);
			if (traincar is Engine)
			{
				Engines.Remove(traincar as Engine);
			}
		}



		public virtual void TimeStepUpdate(float deltaRealTime, float deltaGameTime)
		{
			for (int i1 = 0; i1 < TrainCars.Count; i1++)
			{
				TrainCar trainCar = TrainCars[i1];
				trainCar.AddCouplerForces(true);
				trainCar.AddCouplerForces(false);

				TryToSeparate(trainCar, true);
				TryToSeparate(trainCar, false);

				for (int i2 = i1 + 1; i2 < TrainCars.Count; i2++)
				{
					TrainCar otherTrainCar = TrainCars[i2];
					if (!TryToCouple(trainCar, otherTrainCar, true))
					{
						TryToCouple(trainCar, otherTrainCar, false);
					}
				}
			}
			foreach (TrainCar trainCar in TrainCars)
			{
				trainCar.TimeStepUpdate(deltaGameTime);
			}
		}



		private static void TryToSeparate(TrainCar trainCar, bool previous)
		{
			const float SeparateDistance = 0.5f;

			float distance;
			if (previous)
			{
				if (trainCar.PreviousCoupleType == CoupleType.PushOnly)
				{
					TrainCar otherTrainCar = trainCar.Previous;
					Debug.Assert(otherTrainCar.NextCoupleType == CoupleType.PushOnly);
					distance = (trainCar.GetPreviousCouplerPositionInWorldSpace() - otherTrainCar.GetNextCouplerPositionInWorldSpace()).Length();
					if (distance > SeparateDistance)
					{
						trainCar.Decouple(true, false);
					}
				}
			}
			else
			{
				if (trainCar.NextCoupleType == CoupleType.PushOnly)
				{
					TrainCar otherTrainCar = trainCar.Next;
					Debug.Assert(otherTrainCar.PreviousCoupleType == CoupleType.PushOnly);
					distance = (trainCar.GetNextCouplerPositionInWorldSpace() - otherTrainCar.GetPreviousCouplerPositionInWorldSpace()).Length();
					if (distance > SeparateDistance)
					{
						trainCar.Decouple(false, false);
					}
				}
			}
		}



		private static bool TryToCouple(TrainCar trainCar, TrainCar otherTrainCar, bool updatePreviousCoupler)
		{
			const float CoupleDistance = 0.4f;

			float distance;
			Vector3 direction;
			if (updatePreviousCoupler)
			{
				if (trainCar.Previous != null)
				{
					return false;
				}
				if (otherTrainCar.Previous == null)
				{
					direction = trainCar.GetPreviousCouplerPositionInWorldSpace() - otherTrainCar.GetPreviousCouplerPositionInWorldSpace();
					distance = direction.Length();
					if (distance < CoupleDistance)
					{
						if (Vector3.Dot(direction, trainCar.DirectionInWorldSpace) > 0)
						{
							trainCar.Couple(true, otherTrainCar, true);
						}
						return true;
					}
				}
				if (otherTrainCar.Next == null)
				{
					direction = trainCar.GetPreviousCouplerPositionInWorldSpace() - otherTrainCar.GetNextCouplerPositionInWorldSpace();
					distance = direction.Length();
					if (distance < CoupleDistance)
					{
						if (Vector3.Dot(direction, trainCar.DirectionInWorldSpace) > 0)
						{
							trainCar.Couple(true, otherTrainCar, false);
						}
						return true;
					}
				}
			}
			else
			{
				if (trainCar.Next != null)
				{
					return false;
				}
				if (otherTrainCar.Previous == null)
				{
					direction = trainCar.GetNextCouplerPositionInWorldSpace() - otherTrainCar.GetPreviousCouplerPositionInWorldSpace();
					distance = direction.Length();
					if (distance < CoupleDistance)
					{
						if (Vector3.Dot(direction, trainCar.DirectionInWorldSpace) < 0)
						{
							trainCar.Couple(false, otherTrainCar, true);
						}
						return true;
					}
				}
				if (otherTrainCar.Next == null)
				{
					direction = trainCar.GetNextCouplerPositionInWorldSpace() - otherTrainCar.GetNextCouplerPositionInWorldSpace();
					distance = direction.Length();
					if (distance < CoupleDistance)
					{
						if (Vector3.Dot(direction, trainCar.DirectionInWorldSpace) < 0)
						{
							trainCar.Couple(false, otherTrainCar, false);
						}
						return true;
					}
				}
			}
			return false;
		}



		static bool done = false;
		public virtual void FrameStepUpdate(float deltaRealTime, float deltaGameTime)
		{
			// TODO temporary: works, but is total overkill
			if (!done)
			{
				RemoveAndAddAllRailSegments();
				m_railSystem.SetDefaultPath();
				done = true;
			}
			CreatePointsIndicators();
			
			foreach (TrainCar trainCar in TrainCars)
			{
				trainCar.FrameStepUpdate();
			}
		}



		protected void RemoveAndAddAllRailSegments()
		{
			foreach (RailPiece railPiece in m_railPieces.Values)
			{
				railPiece.RemoveSegmentsFromRailSystem(m_railSystem);
			}
			foreach (RailPiece railPiece in m_railPieces.Values)
			{
				railPiece.AddRailSegmentsToRailSystem(m_railSystem);
			}
		}



		protected virtual void DestroyRailPiece(int id)
		{
			RailPiece railPiece = GetRailPiece(id);
			DestroyRailPiece(railPiece);
				
		}


		private void DestroyRailPiece(RailPiece railPiece)
		{
			railPiece.RemoveSegmentsFromRailSystem(m_railSystem);
			railPiece.Transform.Remove();
			m_railPieces.Remove(railPiece.ID);
		}



		protected void CreatePointsIndicators()
		{
			foreach (PointsIndicator pointsIndicator in m_pointsIndicators)
			{
				pointsIndicator.Transform.Remove();
			}
			m_pointsIndicators.Clear();

			foreach (RailSegment railSegment in m_railSystem.RailSegments)
			{
				if (railSegment.GetConnectionCount(false) > 1 || railSegment.GetConnectionCount(true) > 1)
				{
					bool previousSegments = railSegment.GetConnectionCount(true) > 1;

					var pointsIndicator = new PointsIndicator
                  	{
                  		Model		= m_pointsIndicatorModel.Copy(false, false) as GeometryNode,
                  		Transform	= new TransformNode("Points indicator"),
						Segment		= railSegment,
						AtPrevious	= previousSegments
                  	};
					pointsIndicator.Transform.AddChild(pointsIndicator.Model);
					m_groupNode.AddChild(pointsIndicator.Transform);

					RailSegment.ConnectedSegmentInfo info = railSegment.GetActiveConnectionInfo(previousSegments);
					Debug.Assert(info != null);
					float distance = 25;
					if (info.RailSegment.Curve.Length < 25)
					{
						distance = 0.95f * info.RailSegment.Curve.Length;
					}
					float moveDistance = distance * (previousSegments ? -1 : 1);
					float t = previousSegments ? 0 : 1;
					int directionAlongT = previousSegments ? -1 : 1;
					RailSegment indicatorSegment = railSegment.MoveSignedDistance(moveDistance, ref t, ref directionAlongT);
					Debug.Assert(indicatorSegment == info.RailSegment);
					Vector3 indicatorPosition = indicatorSegment.GetCurvePositionInWorldSpace(t);
					Vector3 indicatorDirection = indicatorSegment.GetCurveTangentInWorldSpace(t) * directionAlongT;
					indicatorPosition.Y += indicatorSegment.RailPiece.Definition.RailTopHeight - 0.2f;
					pointsIndicator.Transform.Transform = MatrixHelper.CreateLookAt(indicatorPosition, indicatorDirection, Vector3.Up);

					m_pointsIndicators.Add(pointsIndicator);
				}
			}
		}



		public void SwitchPoints(PointsIndicator pointIndicator)
		{
			RailSegment segment = pointIndicator.Segment;
			int segmentCount = segment.GetConnectionCount(pointIndicator.AtPrevious);	
			int currentSegmentIndex = segment.GetActiveConnectionIndex(pointIndicator.AtPrevious);
			currentSegmentIndex++;
			if (currentSegmentIndex >= segmentCount)
			{
				currentSegmentIndex = 0;
			}
			segment.SetActiveConnection(pointIndicator.AtPrevious, currentSegmentIndex);
		}



		public virtual void DestroyAllRailPieces()
		{
			int railPieceCount = m_railPieces.Count;
			var railPieces = new RailPiece[railPieceCount];
			m_railPieces.Values.CopyTo(railPieces, 0);
			for (int k = 0 ; k < railPieceCount ; k++)
			{
				DestroyRailPiece(railPieces[k]);
			}
			Debug.Assert(m_railPieces.Count == 0);
		}

	
		
		public bool Save(string fileName)
		{
			StreamWriter streamWriter;
			try
			{
				streamWriter = new StreamWriter(fileName);
			}
			catch
			{
				return false;
			}
			streamWriter.WriteLine(FileHeader);
			streamWriter.Write("{0}\n{{\n", BackgroundString);
			streamWriter.Write("{0}{1}\n}}\n", BackgroundFileNameString, m_backgroundFileName ?? "none");
			foreach(RailPiece railPiece in m_railPieces.Values)
			{
				streamWriter.Write("{0}\n{{\n", RailPieceString);
				if (railPiece is FlexibleRailPiece)
				{
					streamWriter.Write("{0}", RailPieceControlPointsString);
					Debug.Assert(railPiece.RailSegments.Length == 1);
					BezierCurve3 curve = railPiece.RailSegments[0].Curve;
					for (int k = 0; k < curve.ControlPoints.Length; k++)
					{
						SaveVector3(streamWriter, curve.ControlPoints[k]);
						streamWriter.Write(" ");
					}
					streamWriter.WriteLine();
				}
				streamWriter.WriteLine("{0}{1}", RailPieceDefinitionString, railPiece.Definition.Name);
				Matrix m = railPiece.Transform.GetWorldTransformation();
				streamWriter.WriteLine
				(
					"{0}{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}",
					RailPieceTransformString, 
					m.M11, m.M12, m.M13, m.M14, 
					m.M21, m.M22, m.M23, m.M24, 
					m.M31, m.M32, m.M33, m.M34, 
					m.M41, m.M42, m.M43, m.M44
				);
				streamWriter.WriteLine("}");
			}
			streamWriter.Close();
			return true;
		}



		private static void SaveVector3(StreamWriter writer, Vector3 vector)
		{
			writer.Write("{0} {1} {2}", vector.X, vector.Y, vector.Z);
		}



		public virtual bool Load(string fileName, EffectManager effectManager, TextureManager textureManager, MyEffect[] effects)
		{
			StreamReader streamReader;
			try
			{
				streamReader = new StreamReader(fileName);
			}
			catch
			{
				return false;
			}

			string header = streamReader.ReadLine();
			if (header != FileHeader)
			{
				streamReader.Close();
				return false;
			}

			for(;;)
			{
				string line = streamReader.ReadLine();
				if (streamReader.EndOfStream)
				{
					break;
				}
				if (line == RailPieceString)
				{
					LoadRailPiece(streamReader);
				}
				else if (line == BackgroundString)
				{
					LoadBackground(streamReader, effectManager, textureManager, effects);
				}
				else
				{
					Debug.Assert(false);
				}
			}

			streamReader.Close();
			return true;
		}



		private void LoadBackground(StreamReader streamReader, EffectManager effectManager, TextureManager textureManager, MyEffect[] effects)
		{
			streamReader.ReadLine();    // Skip '{'
			for(;;)
			{
				string line = streamReader.ReadLine();
				if (line == "}")
				{
					break;
				}
				if (line.StartsWith(BackgroundFileNameString))
				{
					string backgroundFileName = line.Substring(BackgroundFileNameString.Length);
					if (backgroundFileName != "none")
					{
						Node backgroundNode = Node.Load(backgroundFileName);
						if (backgroundNode != null)
						{
							m_backgroundNode.Remove();
							m_backgroundNode = backgroundNode;
							m_backgroundFileName = backgroundFileName;
							m_backgroundNode.Accept(new EffectSetVisitor(effectManager, textureManager, effects));
							m_groupNode.AddChild(m_backgroundNode);
						}
					}
				}
				else
				{
					Debug.Assert(false);
				}
			}
		}



		private void LoadRailPiece(StreamReader streamReader)
		{
			streamReader.ReadLine();    // Skip '{'

			string definitionName = null;
			Matrix transform = Matrix.Identity;
			Vector3[] controlPoints = null;
			for(;;)
			{
				string line = streamReader.ReadLine();
				if (line == "}")
				{
					break;
				}
				if (line.StartsWith(RailPieceControlPointsString))
				{
					// TODO don't blatantly assume 4 control points
					var stringReader = new StringReader(line.Substring(RailPieceControlPointsString.Length));
					controlPoints = new Vector3[4];
					for (int k = 0; k < 4; k++)
					{
						float x = float.Parse(stringReader.ReadWord());
						float y = float.Parse(stringReader.ReadWord());
						float z = float.Parse(stringReader.ReadWord());
						controlPoints[k] = new Vector3(x, y, z);
					}
				}
				else if (line.StartsWith(RailPieceDefinitionString))
				{
					definitionName = line.Substring(RailPieceDefinitionString.Length);
				}
				else if (line.StartsWith(RailPieceTransformString))
				{
					transform = CreateMatrix(line.Substring(RailPieceTransformString.Length));
				}
				else
				{
					Debug.Assert(false);
				}
			}

			if (definitionName != null)
			{
				CreateRailPiece(definitionName, transform, controlPoints);
			}
		}



		private static Matrix CreateMatrix(string line)
		{
			var stringReader = new StringReader(line);
			var m = new float[4, 4];
			for (int row = 0; row < 4; row++)
			{
				for (int column = 0 ; column < 4; column++)
				{
					m[row, column] = float.Parse(stringReader.ReadWord());
				}
			}

			return new Matrix(
				m[0, 0], m[0, 1], m[0, 2], m[0, 3], 
				m[1, 0], m[1, 1], m[1, 2], m[1, 3], 
				m[2, 0], m[2, 1], m[2, 2], m[2, 3], 
				m[3, 0], m[3, 1], m[3, 2], m[3, 3]);
		}



		public RailSegment GetFirstRailSegment()
		{
			if (m_railSystem.RailSegments.Count > 0)
			{
				return m_railSystem.RailSegments[0];
			}
			return null;
		}
	}
}
*/