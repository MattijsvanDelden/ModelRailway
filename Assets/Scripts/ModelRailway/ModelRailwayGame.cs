using UnityEngine;



namespace ModelRailway
{

public class ModelRailwayGame : Game.Game
{
	public TrainCarDefinition Definition;



	public override void Awake()	// Called by Unity
	{
		base.Awake();

		SetControlFireCallback("ToggleMirrors", ToggleTrainCarMirrors);
		SetControlFireCallback("TogglePantos", ToggleTrainCarPantos);
		SetControlFireCallback("ToggleWipers", ToggleTrainCarWipers);
		SetControlHeldCallback("SpeedIncrease", IncreaseTrainSpeed);
		SetControlHeldCallback("SpeedDecrease", DecreaseTrainSpeed);

		m_selectedTrainCar = new TrainCar(Definition);
	}



	//- PROTECTED ------------------------------------------------------------------------------



	protected override void TimestepUpdate(float deltaTime)
	{
		m_selectedTrainCar.TimestepUpdate(deltaTime);
	}



	protected override void FramestepUpdate(float deltaTime)
	{
		m_selectedTrainCar.FramestepUpdate(deltaTime);
	}



	//- PRIVATE -----------------------------------------------------------------------------



	private void ToggleTrainCarMirrors()
	{
		if (m_selectedTrainCar != null)
		{
			m_selectedTrainCar.ToggleMirrors();
		}
	}



	private void ToggleTrainCarPantos()
	{
		if (m_selectedTrainCar != null)
		{
			m_selectedTrainCar.TogglePantos();
		}
	}



	private void ToggleTrainCarWipers()
	{
		if (m_selectedTrainCar != null)
		{
			m_selectedTrainCar.ToggleWipers();
		}
	}



	private void IncreaseTrainSpeed()
	{
		m_targetSpeed += 
	}



	private void DecreaseTrainSpeed()
	{
	}



	private TrainCar	m_selectedTrainCar;
	private float		m_targetSpeed;
}





/*

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace ModelRailway
{
	public class ModelRailwayGame : MonoBehaviour
	{
		private readonly Dictionary<string, TrainCarDefinition> m_trainCarDefinitions = new Dictionary<string, TrainCarDefinition>();

		private GroupNode m_topGroup;
		private TrainCar m_selectedTrainCar;
		private EditableWorld m_editableWorld;
		private float m_wantedRelativeThrottle;
		private Vector3 m_pickedGroundHitPosition;
		private RailPiece m_pickedRailPiece;
		private bool m_pickedMarker;
		private RailSegment m_pickedMarkerRailSegment;
		private int m_pickedMarkerControlPointIndex;
		private bool m_showDebugInfo;
		private int m_trainCarDefinitionIndex;
		private TrainCar m_firstTrainCar;
		private bool m_animateCamera;


		private string m_directoryName;
		private string[] m_fileNames;
		private int m_currentObjectIndex;
		private Node m_currentObject;

		private PickResults Pick()
		{
			Ray pickRay = m_mainViewport.CreateRayFromEyePoint(m_mouse.X, m_mouse.Y);
			return TopNode.Pick(pickRay);
		}

		private void CollectTrainCarDefinitions(string searchPath)
		{
			if (!Directory.Exists(searchPath))
			{
				return;
			}
			foreach (string folderName in Directory.GetDirectories(searchPath))
			{
				CollectTrainCarDefinitions(folderName);
			}
			bool foundEngFiles = false;
			foreach (string fileName in Directory.GetFiles(searchPath, "*.eng"))
			{
				var definition = new TrainCarDefinition(Path.GetFileNameWithoutExtension(fileName), fileName, true);
				if (!m_trainCarDefinitions.ContainsKey(definition.Name))
				{
					m_trainCarDefinitions.Add(definition.Name, definition);
				}
				foundEngFiles = true;
			}
			foreach (string fileName in Directory.GetFiles(searchPath, "*.wag"))
			{
				var definition = new TrainCarDefinition(Path.GetFileNameWithoutExtension(fileName), fileName, false);
				if (!m_trainCarDefinitions.ContainsKey(definition.Name))
				{
					m_trainCarDefinitions.Add(definition.Name, definition);
				}
				foundEngFiles = true;
			}
			if (!foundEngFiles)
			{
				foreach (string fileName in Directory.GetFiles(searchPath, "*.s"))
				{
					bool isEngine = fileName.Contains("engine");
					var definition = new TrainCarDefinition(Path.GetFileNameWithoutExtension(fileName), fileName, isEngine);
					if (!m_trainCarDefinitions.ContainsKey(definition.Name))
					{
						m_trainCarDefinitions.Add(definition.Name, definition);
					}
				}
			}
		}



		protected override void OnKeyboardKeyChanged(Keys key, bool pressed)
		{
			base.OnKeyChanged(key, pressed);

			if (!pressed)
				return;

			bool controlDown = m_keyboard.IsKeyDown (Keys.LeftControl) || m_keyboard.IsKeyDown (Keys.RightControl);

			switch(key)
			{
				case Keys.A:
					if (controlDown)
					{
						m_editableWorld.SelectAll();
					}
					else
					{
						m_editableWorld.SelectConnectedToSelection();
					}
					break;

				case Keys.B:
					if (controlDown)
					{
						m_animateCamera = !m_animateCamera;
						m_cameraController.Animate(m_animateCamera);
					}
					break;

				case Keys.C:
					if (controlDown)
					{
						m_editableWorld.CopySelectionToClipboard();
					}
					break;

				case Keys.D:
					if (controlDown)
					{
						m_showDebugInfo = !m_showDebugInfo;
						m_editableWorld.ShowDebugInfo = m_showDebugInfo;
					}
					break;

				case Keys.I:
					if (!controlDown)
					{
						m_editableWorld.SelectInvert();
					}
					break;

				case Keys.L:
					if (controlDown)
					{
						m_editableWorld.DestroyAllRailPieces();
						RailPiece.FirstUnusedID = 1;		// TODO not nice here
						m_editableWorld.RemoveTrain(m_firstTrainCar);
						m_textureManager.SearchPath = "Data/Textures";
						m_editableWorld.Load("Testworld.txt", m_effectManager, m_textureManager, m_shadowMapSceneEffects);
						m_editableWorld.AddTrain(m_firstTrainCar, m_editableWorld.GetFirstRailSegment(), 0.7f, false);
						m_editableWorld.LockSelection = false;
					}
					break;

				case Keys.M: 
					if (controlDown)
					{
						m_editableWorld.TogglemarkersDisplay();
					}
					break;

				case Keys.O:
					if (controlDown && m_selectedTrainCar != null)
					{
						m_selectedTrainCar.TogglePantographs();
					}
					break;

				case Keys.S:
					if (controlDown)
					{
						if (m_editableWorld.Save("Testworld.txt"))
						{
							MessageManager.AddMessage("Saved world to 'Testworld.txt'");
						}
						else
						{
							MessageManager.AddMessage("Error, saving world failed");							
						}
					}
					break;

				case Keys.U:
					if (controlDown && m_selectedTrainCar != null)
					{
						m_selectedTrainCar.ToggleMirrors();
					}
					break;

				case Keys.V:
					if (controlDown)
					{
						m_editableWorld.PasteFromClipboard(GetRayPlaneIntersection(m_mouse.X, m_mouse.Y, 0));
					}
					break;

				case Keys.W:
					if (controlDown && m_selectedTrainCar != null)
					{
						m_selectedTrainCar.ToggleWipers();
					}
					break;

				case Keys.Space:
					m_editableWorld.LockSelection = !m_editableWorld.LockSelection;
					MessageManager.AddMessage("{0} selection", m_editableWorld.LockSelection ? "Locked" : "Unlocked");
					break;

				case Keys.Delete:
				case Keys.Back:
					if (m_editableWorld.GetSelectedRailPieceCount() > 0)
					{
						m_editableWorld.LockSelection = false;
						m_editableWorld.RemoveSelectedRailPieces();
					}
					break;

				case Keys.Escape:
					if (m_editableWorld.EditMode && !m_editableWorld.SelectionEmpty())
					{
						m_editableWorld.SelectNone();
					}
					foreach (Engine engine in m_editableWorld.Engines)
					{

						engine.GoalSpeed = 0;
					}
					m_wantedRelativeThrottle = 0;
//					MessageManager.AddMessage("Normalized speed = {0}", m_wantedRelativeThrottle);
					break;

				case Keys.Y:
					if (controlDown)
					{
						if (!m_editableWorld.Redo())
						{
							MessageManager.AddMessage("No more steps to Redo");
						}
					}
					break;

				case Keys.Z:
					if (controlDown)
					{
						if (!m_editableWorld.Undo())
						{
							MessageManager.AddMessage("No more steps to Undo");
						}
					}
					break;

				case Keys.Up:
				case Keys.Down:
					if (m_currentObject != null)
					{
						m_editableWorld.RemoveObject(m_currentObject);
					}
					while (!m_fileNames[m_currentObjectIndex].EndsWith(".s"))
					{
						if (key == Keys.Up)
						{
							m_currentObjectIndex++;
							if (m_currentObjectIndex >= m_fileNames.Length)
							{
								m_currentObjectIndex = 0;
							}
						}
						else
						{
							m_currentObjectIndex--;
							if (m_currentObjectIndex < 0)
							{
								m_currentObjectIndex = m_fileNames.Length - 1;
							}
						}
					}
					m_currentObject = Node.Load(m_fileNames[m_currentObjectIndex]);
					if (m_currentObject != null)
					{
						m_textureManager.ForceSearchPath = true;
						m_textureManager.SearchPath = "D:/Games/Train Simulator/ROUTES/USA1/Textures";
						m_currentObject.Accept(new EffectSetVisitor(m_effectManager, m_textureManager, m_shadowMapSceneEffects));
						m_editableWorld.AddObject(m_currentObject);
						MessageManager.AddMessage("Loaded " + Path.GetFileName(m_fileNames[m_currentObjectIndex]));
					}
					else
					{
						MessageManager.AddMessage("Cannot load " + m_fileNames[m_currentObjectIndex]);
					}
					if (key == Keys.Up)
					{
						m_currentObjectIndex++;
						if (m_currentObjectIndex >= m_fileNames.Length)
						{
							m_currentObjectIndex = 0;
						}
					}
					else
					{
						m_currentObjectIndex--;
						if (m_currentObjectIndex < 0)
						{
							m_currentObjectIndex = m_fileNames.Length - 1;
						}
					}
					break;


				case Keys.Left:
					if (m_trainCarDefinitions.Count > 1)
					{
						m_trainCarDefinitionIndex--;
						if (m_trainCarDefinitionIndex < 0)
						{
							m_trainCarDefinitionIndex = m_trainCarDefinitions.Count - 1;
						}
						ReplaceTrain(m_trainCarDefinitionIndex);
					}
					break;

				case Keys.Right:
					if (m_trainCarDefinitions.Count > 1)
					{
						m_trainCarDefinitionIndex++;
						if (m_trainCarDefinitionIndex > m_trainCarDefinitions.Count - 1)
						{
							m_trainCarDefinitionIndex = 0 ;
						}
						ReplaceTrain(m_trainCarDefinitionIndex);
					}
					break;

				case Keys.Add:
					m_wantedRelativeThrottle += 0.1f;
					if (m_wantedRelativeThrottle > 1)
					{
						m_wantedRelativeThrottle = 1;
					}
//					MessageManager.AddMessage("Normalized speed = {0}", m_wantedRelativeThrottle);
					break;

				case Keys.Subtract:
					m_wantedRelativeThrottle -= 0.1f;
					if (m_wantedRelativeThrottle < -1)
					{
						m_wantedRelativeThrottle = -1;
					}
//					MessageManager.AddMessage("Normalized speed = {0}", m_wantedRelativeThrottle);
					break;

					
				default :
					break;
			}
		}



		private void ReplaceTrain(int definitionIndex)
		{
			if (m_firstTrainCar != null)
			{
				m_editableWorld.RemoveTrain(m_firstTrainCar);
			}
			string[] definitionNames = { GetDefinitionName(definitionIndex) };
			bool[] reversed = { false };
			m_firstTrainCar = CreateTrain(definitionNames, reversed);
			m_editableWorld.AddTrain(m_firstTrainCar, m_editableWorld.GetFirstRailSegment(), 0.7f, false);
			MessageManager.AddMessage("Created {0}", definitionNames[0]);
			m_selectedTrainCar = m_firstTrainCar;
		}



		private string GetDefinitionName(int definitionIndex)
		{
			int index = 0;
			foreach(KeyValuePair<string, TrainCarDefinition> kvp in m_trainCarDefinitions)
			{
				if (index == definitionIndex)
				{
					return kvp.Value.Name;
				}
				index++;
			}
			return null;
		}



		protected override void OnMouseButtonChanged(MyMouse.Buttons button, bool pressed)
		{
 			base.OnMouseButtonChanged(button, pressed);

			switch(button)
			{
				case MyMouse.Buttons.Left:
					OnLeftMouseButtonChanged(pressed);
					break;

				case MyMouse.Buttons.Right:
					OnRightMouseButtonChanged(pressed);
					break;

				case MyMouse.Buttons.Middle:
					OnMiddleMouseButtonChanged(pressed);
					break;

				default:
					break;
			}
		}



		private void OnMiddleMouseButtonChanged(bool pressed)
		{
			if (pressed)
			{
				PickResults pickResults = Pick();
				if (pickResults != null)
				{
					RailPiece pickedRailPiece = m_editableWorld.GetRailPiece(pickResults.GeometryNode);
					Marker pickedMarker = m_editableWorld.GetMarker(pickResults.GeometryNode);

					// Don't 'attach' camera to marker
					if (pickedMarker == null)
					{
						m_cameraController.UpdateWithPickResults(pickResults, false);
					}
				}
			}
		}



		private void OnRightMouseButtonChanged(bool pressed)
		{
			if (pressed)
			{
				PickResults pickResults = Pick();
				m_pickedGroundHitPosition = GetRayPlaneIntersection(m_mouse.X, m_mouse.Y, 0);
				if (pickResults != null)
				{
					// Clicked on some geometry
					PickedGeometryNode(pickResults, false, false);
				}
				else
				{
					// Clicked on 'nothing' (e.g. the sky)
					if (!m_editableWorld.LockSelection)
					{
						m_editableWorld.SelectNone();
					}
					m_pickedRailPiece = null;
				}
			}
		}



		private void OnLeftMouseButtonChanged(bool pressed)
		{
			if (pressed)
			{
				PickResults pickResults = Pick();
				m_pickedGroundHitPosition = GetRayPlaneIntersection(m_mouse.X, m_mouse.Y, 0);
				if (pickResults != null)
				{
					// Clicked on some geometry
					PickedGeometryNode(pickResults, true, true);
				}
				else
				{
					bool controlPressed = m_keyboard.IsKeyDown(Keys.LeftControl) || m_keyboard.IsKeyDown(Keys.RightControl);
					
					// Clicked on 'nothing' (e.g. the sky)
					if (!m_editableWorld.LockSelection && !controlPressed)
					{
						m_editableWorld.SelectNone();
					}
					m_pickedRailPiece = null;
				}
			}
			else
			{
				if (m_translatingSelection)
				{
					m_editableWorld.SelectionTranslateEnd();
					m_translatingSelection = false;
				}
				if (m_rotatingSelection)
				{
					m_editableWorld.SelectionRotateEnd();
					m_rotatingSelection = false;
				}
				m_pickedRailPiece = null ;
			}
		}



		protected override void Initialize()
		{
			base.Initialize();

			Window.Title = "Model Railway in C#";

			m_directoryName			= "D:/Games/Train Simulator/ROUTES/USA1/Shapes";
			m_fileNames				= Directory.GetFiles(m_directoryName);
			m_currentObject			= null;
			m_currentObjectIndex	= 0;

			m_topGroup = new GroupNode();

			m_lightNode = new LightNode("Sun") 
			{
				AmbientColour = new Colour(0.6f, 0.6f, 0.6f, 1),
				DiffuseColour = Colour.White
			};
			m_lightNode.AddChild(m_topGroup);

//			Node terrain = Node.Load("Geometry/The Berrow Format.fsd");
//			Node terrain = Node.Load("Geometry/Pallet Lane.fsd");
//			terrain.Accept(new EffectSetVisitor(m_effectManager, m_textureManager));
			GeometryNode terrain = GeometryNode.CreatePlane(500, 500, new Colour(2 * 25 / 255.0f, 2 * 42 / 255.0f, 2 * 10 / 255.0f, 1), DoShadowMapping ? m_shadowMapSceneEffects[1] : m_effectManager.GetEffect("ClassicLighting_1Texture"));
			Geometry geo = terrain.GeometrySet.Geometries[0];
			geo.TextureCoordinateElementCounts.Add(2);
			var uvs = new float[] { 0, 0,  20, 0,  20, 20,  0, 20 };
			geo.TextureCoordinates.Add(uvs);
			Material terrainMaterial = terrain.MaterialTable.Materials[0];
			m_textureManager.SearchPath = "Data/Textures";
			var stage = new MaterialStage
          	{
          		Texture = m_textureManager.GetTexture("Speckles"),
				TextureName = "Speckles"
          	};
			terrainMaterial.Stages.Add(stage);
//			terrain.MaterialTable.Materials[0].Specular = Colour.White;
//			terrain.TranslateGeometry(0, -5, 0);
//			m_topGroup.AddChild(terrain);

			m_graphGroup = new GroupNode("Scene graph scratch group");
			m_topGroup.AddChild(m_graphGroup);

			m_axesGeometryForGraph = Node.Load("Data/Geometry/Axes.fsd");
			if (m_axesGeometryForGraph != null)
			{
				m_axesTransformForGraph = new TransformNode("Scene graph axes transform");
				m_axesTransformForGraph.AddChild(m_axesGeometryForGraph);
				m_graphGroup.AddChild(m_axesTransformForGraph);
				m_axesTransformForGraph.Enabled = false;
			}


			var editGroupNode = new GroupNode("Edit");
			m_topGroup.AddChild(editGroupNode);

			var worldGroupNode = new GroupNode("World");
			m_topGroup.AddChild(worldGroupNode);

			m_editableWorld = new EditableWorld(worldGroupNode, editGroupNode, terrain)
			{
				EditMode = true
			};

			CreateSampleRailPieceDefinitions();
//			m_editableWorld.CreateSampleRailPieces();

//			string[] definitionNames = { "eng_class50", "vam1", "vam2", "vam3", "vam4", "vam5", "vam2", "vam3", "vam1", "vam5", "vam4", "us2freightcar1", "us2freightcar1", "us2freightcar3", "us2freightcar4", "us2freightcar4", "us2freightcar4", "us2freightcar5" };
//			string[] definitionNames = { "eng_class50", "eng_class50", "us2freightcar4", "us2freightcar4", "us2freightcar4", "us2freightcar4", "us2freightcar4", "us2freightcar4", "us2freightcar4" };
//			string[] definitionNames = { "oe380engine", "oe380tender", "oe380servcar1", "oe380servcar2" };
//			string[] definitionNames = { "oe380engine", "oe380tender", "secrw1328", "secrw1328", "secrw1328", "secrw1344", "secrw1344", "secrw1344", "secrw1346", "secrw1346", "secrw1328", "secrw1328", "secrw1328", "secrw1344", "secrw1344", "secrw1344", "secrw1346", "secrw1346", "secrw1328", "secrw1328", "secrw1328", "secrw1344", "secrw1344", "secrw1344", "secrw1346", "secrw1346", "secrw1328", "secrw1328", "secrw1328", "secrw1344", "secrw1344", "secrw1344", "secrw1346", "secrw1346", "secrw1328", "secrw1328", "secrw1328", "secrw1344", "secrw1344", "secrw1344", "secrw1346", "secrw1346" };
//			string[] definitionNames = { "oe380engine", "oe380tender", "secrw1328", "secrw1328", "secrw1328", "secrw1344", "secrw1344", "secrw1344", "secrw1346", "secrw1346" };
//			string[] definitionNames = { "BigBoyPart1", "BigBoyPart2", "BigBoyTender", "us2fullloggercar", "us2fullloggercar", "us2fullloggercar", "us2fullloggercar", "us2fullloggercar" };
			string[] definitionNames = { "BigBoyPart1", "BigBoyPart2", "BigBoyTender", "rg92", "rg92" };
//			string[] definitionNames = { "rg92" } ;


			m_editableWorld.Load("Pallet Lane.txt", m_effectManager, m_textureManager, m_shadowMapSceneEffects);
			m_editableWorld.FrameStepUpdate(0, 0);


			m_firstTrainCar = CreateTrain(definitionNames, null);
			m_selectedTrainCar = m_firstTrainCar;
			m_editableWorld.AddTrain(m_firstTrainCar, m_editableWorld.GetFirstRailSegment(), 0.99f, false);

			m_trainCarDefinitionIndex = 0;
			int index = 0;
			foreach (string name in m_trainCarDefinitions.Keys)
			{
				if (name == definitionNames[0])
				{
					m_trainCarDefinitionIndex = index;
					break;
				}
				index++;
			}


			TopNode = m_lightNode;

			m_editableWorld.ClearUndoRedoStack();
			m_editableWorld.DisplayMarkers = false;
	
		}



		private TrainCar CreateTrain(string[] definitionNames, bool[] reversed)
		{
			if (reversed != null && definitionNames.Length != reversed.Length)
			{
				throw new ArgumentException("arrays definitionNames and reversed should have same length");
			}

			// TODO use reversed

			TrainCar firstTrainCar = null;
			TrainCar previousTrainCar = null;
			for (int k = 0 ; k < definitionNames.Length ; k++)
			{
				TrainCar trainCar = CreateTrainCar(definitionNames[k]);
				if (trainCar == null)
				{
					continue;
				}
				if (firstTrainCar == null)
				{
					firstTrainCar = trainCar;
				}
				else
				{
					trainCar.Couple(true, previousTrainCar, false);
				}
				previousTrainCar = trainCar;
				
			}
			return firstTrainCar;
		}



		private TrainCar CreateTrainCar(string definitionName)
		{
			if (!m_trainCarDefinitions.ContainsKey(definitionName))
			{
				return null;
			}
			var definition = m_trainCarDefinitions[definitionName];
			if (!definition.IsLoaded)
			{
				if (!definition.Load(m_textureManager))
				{
					return null;
				}
				if (!definition.IsLoaded)
				{
					return null;
				}
				definition.GroupNode.Accept(new EffectSetVisitor(m_effectManager, m_textureManager, m_shadowMapSceneEffects));
			}
			if (definition.Engine)
			{
				return new Engine(definition);
			}
			return new TrainCar(definition);
		}



		private void CreateSampleRailPieceDefinitions()
		{
			m_textureManager.SearchPath = "Data/Textures";
            var materialStage1 = new MaterialStage
            {
            	TextureName = "Foundation01",
            	Texture = m_textureManager.GetTexture("Foundation01")
            };
            var materialStage2 = new MaterialStage
            {
            	TextureName = "Foundation01Gloss",
            	Texture = m_textureManager.GetTexture("Foundation01Gloss")
            };

			var material = new Material 
			{
				Ambient = Colour.Grey75Percent, 
				Diffuse = Colour.Grey75Percent,
				Specular = Colour.White
			};
			material.Stages.Add(materialStage1);
			material.Stages.Add(materialStage2);
			material.Effect = DoShadowMapping ? m_shadowMapSceneEffects[1] : m_effectManager.GetEffect("ClassicLighting_1Texture");

			var materialTable = new MaterialTable();
			materialTable.Materials.Add(material);

			Vector3[] controlPoints1 = 
			{
				new Vector3(0, 0, 0),
				new Vector3(22, 0, 0),
				new Vector3(80, 0, 0),
				new Vector3(100, 0, 0),
			};
			var curve1 = new BezierCurve3(controlPoints1);
			RailSegment[] segments1 =
			{
				new RailSegment(curve1)
			};


			Vector3[] controlPoints2 = 
			{
				new Vector3(0, 0, 0),
				new Vector3(-8, 0, -3),
				new Vector3(-36, 0, 0),
				new Vector3(-40, 0, 0),
			};
			var curve2 = new BezierCurve3(controlPoints2);
			RailSegment[] segments2 =
			{
				new RailSegment(curve2)
			};

	
			Vector3[] controlPoints3 = 
			{
				new Vector3(0, 0, 0),
				new Vector3(1, 0, 0),
				new Vector3(39, 0, 0),
				new Vector3(40, 0, 0),
			};
			var curve3 = new BezierCurve3(controlPoints3);
			RailSegment[] segments3 =
			{
				new RailSegment(curve3)
			};


			Vector3[] pointsControlPointsStraight =
			{
				new Vector3(0, 0, 0), 
				new Vector3(1, 0, 0), 
				new Vector3(24, 0, 0), 
				new Vector3(25, 0, 0), 
			};
			var pointsCurveStraight = new BezierCurve3(pointsControlPointsStraight);

			Vector3[] pointsControlPointsCurvedLeft =
			{
				new Vector3(0, 0, 0), 
				new Vector3(5, 0, 0), 
				new Vector3(15, 0, 0), 
				new Vector3(25, 0, 5), 
			};
			var pointsCurveCurvedLeft = new BezierCurve3(pointsControlPointsCurvedLeft);

			Vector3[] pointsControlPointsCurvedRight =
			{
				new Vector3(0, 0, 0), 
				new Vector3(5, 0, 0), 
				new Vector3(15, 0, 0), 
				new Vector3(25, 0, -5), 
			};
			var pointsCurveCurvedRight = new BezierCurve3(pointsControlPointsCurvedRight);

			RailSegment[] pointsLeftSegments =
			{
				new RailSegment(pointsCurveStraight),
				new RailSegment(pointsCurveCurvedLeft),
			};

			RailSegment[] pointsRightSegments =
			{
				new RailSegment(pointsCurveStraight),
				new RailSegment(pointsCurveCurvedRight),
			};
	
			RailSegment[] pointsSymmetricalSegments =
			{
				new RailSegment(pointsCurveCurvedLeft),
				new RailSegment(pointsCurveCurvedRight),
			};

			RailSegment[] pointsThreewaySegments =
			{
				new RailSegment(pointsCurveStraight),
				new RailSegment(pointsCurveCurvedLeft),
				new RailSegment(pointsCurveCurvedRight),
			};

			Vector2[] crossSectionPositions =
			{
				new Vector2(-2.5f, 0), 
				new Vector2(-1.5f, 0.4f), 
				new Vector2(-0.85f, 0.4f),
				new Vector2(-0.85f, 0.4f),
				new Vector2(-0.85f, 0.6f),
				new Vector2(-0.85f, 0.6f),
				new Vector2(-0.75f, 0.6f),
				new Vector2(-0.75f, 0.6f),
				new Vector2(-0.75f, 0.4f),
				new Vector2(-0.75f, 0.4f),
				new Vector2(0.75f, 0.4f),
				new Vector2(0.75f, 0.4f),
				new Vector2(0.75f, 0.6f),
				new Vector2(0.75f, 0.6f),
				new Vector2(0.85f, 0.6f),
				new Vector2(0.85f, 0.6f),
				new Vector2(0.85f, 0.4f),
				new Vector2(0.85f, 0.4f),
				new Vector2(1.5f, 0.4f), 
				new Vector2(2.5f, 0), 
			};

			float s = 512;
			float[] crossSectionVs =
			{
				1, 458 / s, 394 / s,
				0.0f, 51 / s, 51 / s, 75 / s, 75 / s, 126 / s, 
				395 / s, 236 / s, 
				0.0f, 51 / s, 51 / s, 75 / s, 75 / s, 126 / s, 
				237 / s, 175 / s, 130 / s,
			};

			var creationParameters = new RailGeometryCreationParameters(materialTable, crossSectionPositions, crossSectionVs, 0.15f, 0.6f);

			var railPieceDefinition1 = new RailPieceDefinition("Test1", true, segments1, creationParameters);
			m_editableWorld.AddRailPieceDefinition(railPieceDefinition1);

			var railPieceDefinition2 = new RailPieceDefinition("Test2", false, segments2, creationParameters);
			m_editableWorld.AddRailPieceDefinition(railPieceDefinition2);

			var railPieceDefinition3 = new RailPieceDefinition("Test3", false, segments3, creationParameters);
			m_editableWorld.AddRailPieceDefinition(railPieceDefinition3);

			var pointsLeftDefinition = new RailPieceDefinition("PointsLeft", false, pointsLeftSegments, creationParameters);
			m_editableWorld.AddRailPieceDefinition(pointsLeftDefinition);

			var pointsRightDefinition = new RailPieceDefinition("PointsRight", false, pointsRightSegments, creationParameters);
			m_editableWorld.AddRailPieceDefinition(pointsRightDefinition);

			var pointsSymmetricalDefinition = new RailPieceDefinition("PointsSymmetrical", false, pointsSymmetricalSegments, creationParameters);
			m_editableWorld.AddRailPieceDefinition(pointsSymmetricalDefinition);

			var pointsThreewayDefinition = new RailPieceDefinition("PointsThreeway", false, pointsThreewaySegments, creationParameters);
			m_editableWorld.AddRailPieceDefinition(pointsThreewayDefinition);
		}



		protected override void TimeStepUpdate(float deltaRealTime, float deltaGameTime)
		{
			m_editableWorld.TimeStepUpdate(deltaRealTime, deltaGameTime);

			base.TimeStepUpdate(deltaRealTime, deltaGameTime);
		}



		protected override void FrameStepUpdate(float deltaRealTime, float deltaGameTime)
		{
//			Thread.Sleep(10);

			if (m_selectedTrainCar != null)
			{
				Train.SetRelativeThrottle(m_selectedTrainCar, m_wantedRelativeThrottle);
			}

			if (IsActive)
			{
				m_cameraController.RotationEnabled = true;
				m_cameraController.ZoomEnabled = true;
				if (m_pickedRailPiece != null)
				{
					if (m_mouse.IsPressed(MyMouse.Buttons.Left))
					{
						if (m_keyboard.IsKeyDown(Keys.LeftShift) || m_keyboard.IsKeyDown(Keys.RightShift))
						{
							m_cameraController.ZoomEnabled = false;
							if (!m_rotatingSelection)
							{
								m_editableWorld.SelectionRotateBegin();
								m_rotatingSelectionOrigin = GetRayPlaneIntersection(m_mouse.X, m_mouse.Y, 0);
								m_rotatingSelection = true;
							}
							m_editableWorld.SelectionRotate(m_mouse.DeltaX * 0.02f, m_rotatingSelectionOrigin);
						}
						else
						{
							// Dragging with the left button pressed
							Vector3 newGroundHitPosition = GetRayPlaneIntersection(m_mouse.X, m_mouse.Y, 0);
							Vector3 deltaHitPosition = newGroundHitPosition - m_pickedGroundHitPosition;
							m_pickedGroundHitPosition = newGroundHitPosition;

							if (m_pickedMarker)
							{
								// Drag the marker (might also drag associated markers)
								m_cameraController.RotationEnabled = false;
								DragMarker(deltaHitPosition, newGroundHitPosition);
							}
							else
							{
								if (m_editableWorld.IsSelectedRailPiece(m_pickedRailPiece))
								{
									// Drag the selected rail pieces
									m_cameraController.RotationEnabled = false;
									if (!m_translatingSelection)
									{
										m_editableWorld.SelectionTranslateBegin();
										m_translatingSelection = true;
									}
									m_editableWorld.SelectionTranslate(deltaHitPosition);
								}
							}
						}
					}
				}
			}

			m_editableWorld.FrameStepUpdate(deltaRealTime, deltaGameTime);

			m_cameraController.Update(deltaRealTime, m_keyboard, m_mouse);

			base.FrameStepUpdate(deltaRealTime, deltaGameTime);
		}



		private void DragMarker(Vector3 deltaPosition, Vector3 position)
		{
			RailSegment railSegment = m_pickedMarkerRailSegment;
			if (railSegment.RailPiece is FlexibleRailPiece)
			{
				// Transform deltaPosition to local rail piece space
				Matrix transform = railSegment.RailPiece.Transform.GetWorldTransformation();
				Matrix transformInverse = Matrix.Invert(transform);
				Vector3 deltaPositionInRailPieceSpace = Vector3.TransformNormal(deltaPosition, transformInverse);
				Vector3 controlPointPosition = railSegment.Curve.ControlPoints[m_pickedMarkerControlPointIndex] + deltaPositionInRailPieceSpace;

				// Dragging a marker (i.e. a control point) of a flexible
				// rail piece curve. Move that control point. If it's a
				// 'shape' point adjacent to an endpoint and that endpoint
				// is connected, then desired behaviour depends on the other
				// segments. If at least one is a rigid segment, we want to constrain the 
				// movement of the shape point so that the connector maintains C1 
				// continuity. If they're all flexible segments, we want to move the
				// corresponding shape point of those segments in the opposite
				// direction, again to maintain C1 continuity
				if (m_pickedMarkerControlPointIndex == 1)
				{
					if (railSegment.ConnectedSegmentInfos.Count > 0)
					{
						if (railSegment.AllConnectedSegmentsAreFlexible(true))
						{
							Vector3 directionInWorldSpace = railSegment.Curve.ControlPoints[0] - controlPointPosition;
							directionInWorldSpace = Vector3.TransformNormal(directionInWorldSpace, transform);
							directionInWorldSpace.Normalize();

							railSegment.UpdateConnectedShapeControlPoints(directionInWorldSpace, true);

							RailSegment.ConnectedSegmentInfo previousSegmentInfo = RailSegment.NoConnectedSegmentInfo;
							foreach (RailSegment.ConnectedSegmentInfo connectedSegmentInfo in railSegment.ConnectedSegmentInfos)
							{
								if (connectedSegmentInfo.AsPrevious)
								{
									previousSegmentInfo = connectedSegmentInfo;
									break;
								}
							}
							if (previousSegmentInfo.RailSegment != null)
							{
								previousSegmentInfo.RailSegment.UpdateConnectedShapeControlPoints(-directionInWorldSpace, !previousSegmentInfo.SameTDirection);
							}
						}
						else
						{
							Vector3 curveTangent = railSegment.Curve.GetTangent(0);
							Vector3 markerPositionInRailPieceSpace = Vector3.Transform(position, transformInverse);
							Vector3 v = markerPositionInRailPieceSpace - railSegment.Curve.ControlPoints[0];

							float f = Vector3.Dot(curveTangent, v);
							controlPointPosition = railSegment.Curve.ControlPoints[0] + curveTangent * f;
						}
					}
				}
				else if (m_pickedMarkerControlPointIndex == 2)
				{
					if (railSegment.ConnectedSegmentInfos.Count > 0)
					{
						if (railSegment.AllConnectedSegmentsAreFlexible(false))
						{
							Vector3 directionInWorldSpace = railSegment.Curve.ControlPoints[3] - controlPointPosition;
							directionInWorldSpace = Vector3.TransformNormal(directionInWorldSpace, transform);
							directionInWorldSpace.Normalize();

							railSegment.UpdateConnectedShapeControlPoints(directionInWorldSpace, false);

							RailSegment.ConnectedSegmentInfo nextSegmentInfo = RailSegment.NoConnectedSegmentInfo;
							foreach (RailSegment.ConnectedSegmentInfo connectedSegmentInfo in railSegment.ConnectedSegmentInfos)
							{
								if (!connectedSegmentInfo.AsPrevious)
								{
									nextSegmentInfo = connectedSegmentInfo;
									break;
								}
							}
							if (nextSegmentInfo.RailSegment != null)
							{
								nextSegmentInfo.RailSegment.UpdateConnectedShapeControlPoints(-directionInWorldSpace, nextSegmentInfo.SameTDirection);
							}
						}
						else
						{
							Vector3 curveTangent = railSegment.Curve.GetTangent(0);
							Vector3 markerPositionInRailPieceSpace = Vector3.Transform(position, transformInverse);
							Vector3 v = markerPositionInRailPieceSpace - railSegment.Curve.ControlPoints[0];

							float f = Vector3.Dot(curveTangent, v);
							controlPointPosition = railSegment.Curve.ControlPoints[0] + curveTangent * f;
						}
					}
				}

				// If the control point is a curve start or end point, also 
				// move the corresponding intermediate ('shape') point
				if (m_pickedMarkerControlPointIndex == 0)
				{
					Vector3 deltaControlPositions = railSegment.Curve.ControlPoints[1] - railSegment.Curve.ControlPoints[0];
					Vector3 connectorPositionInWorldSpace = Vector3.Transform(controlPointPosition, transform);
					RailPiece[] exclude = { railSegment.RailPiece } ;
					Vector3 closestConnectorPosition;
					Vector3 closestConnectorDirection;
					bool snapped = false;
					if (m_editableWorld.GetClosestConnector(connectorPositionInWorldSpace, exclude, out closestConnectorPosition, out closestConnectorDirection))
					{
						float distance = (connectorPositionInWorldSpace - closestConnectorPosition).Length();
						if (distance < m_editableWorld.MarkerSnapRadius)
						{
							controlPointPosition = Vector3.Transform(closestConnectorPosition, transformInverse);
							snapped = true;
						}
					}
					if (snapped)
					{
						closestConnectorDirection = Vector3.TransformNormal(closestConnectorDirection, transformInverse);
						double angleOfClosestConnector = Math.Atan2(-closestConnectorDirection.Z, -closestConnectorDirection.X);

						Vector3 direction = railSegment.Curve.GetTangent(0);
						double angleOfConnector = Math.Atan2(direction.Z, direction.X);

						float deltaAngle = (float) (angleOfClosestConnector - angleOfConnector);
						const float Pi = (float) Math.PI;
						if (deltaAngle > Pi)
						{
							deltaAngle -= 2 * Pi;
						}
						else if (deltaAngle < -Pi)
						{
							deltaAngle += 2 * Pi;
						}
						float length = deltaControlPositions.Length();
						if (Math.Abs(deltaAngle) < 0.5f * Pi)
						{
							railSegment.Curve.SetControlPoint(1, controlPointPosition - closestConnectorDirection * length);
						}
						else
						{
							railSegment.Curve.SetControlPoint(1, controlPointPosition + closestConnectorDirection * length);
						}
					}
					else
					{
						railSegment.Curve.SetControlPoint(1, controlPointPosition + deltaControlPositions);
					}
				}
				else if (m_pickedMarkerControlPointIndex == 3)
				{
					Vector3 deltaControlPositions = railSegment.Curve.ControlPoints[2] - railSegment.Curve.ControlPoints[3];
					Vector3 connectorPositionInWorldSpace = Vector3.Transform(controlPointPosition, transform);
					RailPiece[] exclude = { railSegment.RailPiece };
					Vector3 closestConnectorPosition;
					Vector3 closestConnectorDirection;
					bool snapped = false;
					if (m_editableWorld.GetClosestConnector(connectorPositionInWorldSpace, exclude, out closestConnectorPosition, out closestConnectorDirection))
					{
						float distance = (connectorPositionInWorldSpace - closestConnectorPosition).Length();
						if (distance < m_editableWorld.MarkerSnapRadius)
						{
							controlPointPosition = Vector3.Transform(closestConnectorPosition, transformInverse);
							snapped = true;
						}
					}
					if (snapped)
					{
						closestConnectorDirection = Vector3.TransformNormal(closestConnectorDirection, transformInverse);
						double angleOfClosestConnector = Math.Atan2(-closestConnectorDirection.Z, -closestConnectorDirection.X);

						Vector3 direction = railSegment.Curve.GetTangent(1);
						double angleOfConnector = Math.Atan2(direction.Z, direction.X);

						float deltaAngle = (float) (angleOfClosestConnector - angleOfConnector);
						const float Pi = (float) Math.PI;
						if (deltaAngle > Pi)
						{
							deltaAngle -= 2 * Pi;
						}
						else if (deltaAngle < -Pi)
						{
							deltaAngle += 2 * Pi;
						}
						float length = deltaControlPositions.Length();
						if (Math.Abs(deltaAngle) < 0.5f * Pi)
						{
							railSegment.Curve.SetControlPoint(2, controlPointPosition + closestConnectorDirection * length);
						}
						else
						{
							railSegment.Curve.SetControlPoint(2, controlPointPosition - closestConnectorDirection * length);
						}
					}
					else
					{
						railSegment.Curve.SetControlPoint(2, controlPointPosition + deltaControlPositions);
					}
				}

				railSegment.Curve.SetControlPoint(m_pickedMarkerControlPointIndex, controlPointPosition);

				// Update (by recreating) the flexible rail piece geometry
				RailPiece railPiece = railSegment.RailPiece;
				GeometrySet geometrySet = railPiece.GameObject.GeometrySet;
				geometrySet.Geometries.Clear();
				geometrySet.Geometries.Add(railPiece.Definition.CreateGeometry(railPiece.RailSegments));
			}
			else
			{
				RailPiece railPiece = railSegment.RailPiece;
				if (m_editableWorld.IsSelectedRailPiece(railPiece))
				{
					// Drag the whole selected (rigid) rail piece when dragging one of its markers
					m_editableWorld.SelectionTranslate(deltaPosition);
				}
			}
		}



		/// <summary>
		/// Returns the point where the ray starting at the eye point and passing through
		/// the viewport at the specified pixel location intersects the horizontal plane
		/// with equation Y = planeY
		/// </summary>
		/// <param name="viewportPositionX">
		/// X coordinate of pixel location in viewport
		/// </param>
		/// <param name="viewportPositionY">
		/// Y coordinate of pixel location in viewport, with Y = 0 at top of viewport
		/// </param>
		/// <param name="planeY"></param>
		/// <returns>
		/// The 3D intersection point of the ray and the plane, or the zero vector if
		/// the ray and plane do not intersect. This point is in camera space.
		/// </returns>
		private Vector3 GetRayPlaneIntersection(int viewportPositionX, int viewportPositionY, float planeY)
		{
			Vector3 intersection;
			Ray mouseRay = m_mainViewport.CreateRayFromEyePoint(viewportPositionX, viewportPositionY);
			float t = ((mouseRay.Position.Y - planeY) / -mouseRay.Direction.Y);
			if (t < 0)
			{
				// intersection point is behind the eye point
				intersection = Vector3.Zero;
			}
			else
			{
				intersection = mouseRay.Position + mouseRay.Direction * t;
			}
			return intersection;
		}



		private void PickedGeometryNode(PickResults pickResults, bool allowRailPiecePick, bool leftPick)
		{
			if (!m_editableWorld.EditMode)
			{
				return;
			}
			
			m_pickedRailPiece = m_editableWorld.GetRailPiece(pickResults.GeometryNode);
			Marker pickedMarker = m_editableWorld.GetMarker(pickResults.GeometryNode);
			TrainCar pickedTrainCar = m_editableWorld.GetTrainCar(pickResults.GeometryNode);
			PointsIndicator pointsIndicator = m_editableWorld.GetPointsIndicator(pickResults.GeometryNode);
			m_pickedMarker = false;

			if (m_pickedRailPiece != null && allowRailPiecePick)
			{
				PickedRailPiece();
			}
			else if (pickedMarker != null)
			{
				// Picked a marker. If it's a marker of a rigid rail
				// piece, select the rail piece itself so that the
				// whole rail piece will be dragged
				m_pickedRailPiece = pickedMarker.RailSegment.RailPiece;
				if (m_pickedRailPiece is RigidRailPiece)
				{
					PickedRailPiece();
				}
				else
				{
					m_pickedMarker = true;
					m_pickedMarkerRailSegment = pickedMarker.RailSegment;
					m_pickedMarkerControlPointIndex = pickedMarker.ControlPointIndex;
				}
			}
			else if (pointsIndicator != null)
			{
				m_editableWorld.SwitchPoints(pointsIndicator);
			}
			else if (pickedTrainCar != null)
			{
				if (leftPick)
				{
					if (m_keyboard.IsKeyDown (Keys.LeftControl))
					{
						PartiallyDecouple(pickedTrainCar, pickResults.GlobalHitPosition);					
					}
					else
					{
						m_selectedTrainCar = pickedTrainCar;
					}
				}
			}
			else
			{
				bool controlPressed = m_keyboard.IsKeyDown(Keys.LeftControl) || m_keyboard.IsKeyDown(Keys.RightControl);

				// Clicked on 'other' geometry
				if (!m_editableWorld.LockSelection && !controlPressed && leftPick)
				{
					m_editableWorld.SelectNone();
				}
			}
		}



		private static void PartiallyDecouple(TrainCar trainCar, Vector3 globalHitPosition)
		{
			Matrix inverseBodyTransform = Matrix.Invert(trainCar.BodyTransform);
			Vector3 localHitPosition = Vector3.Transform(globalHitPosition, inverseBodyTransform);

			float distToFront	= Math.Abs(trainCar.Definition.BoundingBox.Max.X - localHitPosition.X);
			float distToRear	= Math.Abs(trainCar.Definition.BoundingBox.Min.X - localHitPosition.X);
			if (distToFront < distToRear)
			{
				trainCar.Decouple(trainCar.Reversed ? false : true, true);
			}
			else
			{
				trainCar.Decouple(trainCar.Reversed ? true : false, true);
			}
		}



		private void PickedRailPiece()
		{
			if (m_editableWorld.LockSelection)
			{
				// Don't allow selection to be changed if locked
				return;
			}

			bool controlPressed = m_keyboard.IsKeyDown(Keys.LeftControl) || m_keyboard.IsKeyDown(Keys.RightControl);
			
			if (controlPressed)
			{
				// Ctrl + left button click: toggle selection status of clicked rail piece,
				// leave selection status of all other rail pieces unchanged
				m_editableWorld.SelectToggleSingle(m_pickedRailPiece);
			}
			else
			{
				// Left button click: if on unselected rail piece: select this piece and deselect
				// everyhting else. If on selected rail piece: no change
				if (!m_editableWorld.IsSelectedRailPiece(m_pickedRailPiece))
				{
					m_editableWorld.SelectSingle(m_pickedRailPiece);
				}
			}
		}

	
		
		public ModelRailwayGame()
		{
			AddHelpText("");
			AddHelpText("Left mouse : Select train");
			AddHelpText("Ctrl+left mouse : Decouple train car");
			AddHelpText("Left mouse : Select rail piece (deselects others)");
			AddHelpText("Ctrl+left mouse : Add rail piece to selection");
			AddHelpText("Shift+left mouse : Rotate selection");
			AddHelpText("");
			AddHelpText("Ctrl+A : Select all rail pieces");
			AddHelpText("Ctrl+C : Copy selected rail pieces to clipboard");
			AddHelpText("Ctrl+D : Show/hide debug information");
			AddHelpText("Ctrl+L : Load world file 'Testworld.txt'");
			AddHelpText("Ctrl+M : Show/hide markers");
			AddHelpText("Ctrl+O : Toggle train pantographs");
			AddHelpText("Ctrl+S : Save current world to file 'Testworld.txt'");
			AddHelpText("Ctrl+U : Toggle train mirrors");
			AddHelpText("Ctrl+V : Paste contents op clipboard");
			AddHelpText("Ctrl+W : Toggle train wipers");
			AddHelpText("I : invert selection");
			AddHelpText("Space : Lock/unlock selection");
			AddHelpText("Delete : Delete selected rail pieces");
			AddHelpText("Backspace : Delete selected rail pieces");
			AddHelpText("Escape : Clear selection");
			AddHelpText("Left: Previous train car definition");
			AddHelpText("Right : Next train car definition");
			AddHelpText("Num + : Increase train car speed");
			AddHelpText("Num - : Decrease train car speed");

			CollectTrainCarDefinitions("Data/Rolling Stock");
		}



		protected override void Draw (GameTime gameTime)
		{
			m_lightNode.Direction = Vector3.TransformNormal(Vector3.UnitZ, m_shadowMapCreators[0].Camera.Transformation);

			base.Draw (gameTime);

			if (m_showDebugInfo)
			{
				m_editableWorld.DrawRailPieceIDs(m_mainViewport, m_defaultFont, m_spriteBatch);
			}
		}



		private bool m_rotatingSelection;
		private Vector3 m_rotatingSelectionOrigin;
		private bool m_translatingSelection;
	}
}
*/
}