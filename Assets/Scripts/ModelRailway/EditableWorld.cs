/*

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;



namespace ModelRailway
{
	partial class EditableWorld : World
	{
		private struct RailPieceSelectionInfo
		{
			public Material SelectedMaterialTable;
			public Material OriginalMaterialTable;
		}



		private struct ClipboardEntry
		{
			public RailPieceDefinition Definition;
			public Matrix4x4 WorldTransformation;
			public Vector3[] FlexibleControlPoints;
		}


		
		private bool m_selectionGlowFactorIncrease = true;
		private float m_selectionGlowFactor;
		private readonly Dictionary<RailPiece, RailPieceSelectionInfo> m_selectedRailPieces = new Dictionary<RailPiece, RailPieceSelectionInfo>();
		private readonly List<ClipboardEntry> m_clipboardEntries = new List<ClipboardEntry>();
		private readonly GameObject m_editGroupNode;
		private readonly GameObject m_markersGroupNode;
		private readonly List<Material> m_markerMaterialTables = new List<Material>();
		private readonly List<Marker> m_markers = new List<Marker>();
		private readonly Stack<EditorCommand> m_doneStack = new Stack<EditorCommand>();
		private readonly Stack<EditorCommand> m_redoStack = new Stack<EditorCommand>();

		public bool EditMode { get; set; }

		public float MarkerSnapRadius { get; private set; }

		public bool LockSelection { get; set; }

		public bool DisplayMarkers 
		{
			private get { return m_markersGroupNode.activeSelf; }  
			set { m_markersGroupNode.SetActive(value); }
		}



		public EditableWorld(GroupNode worldGroupNode, GroupNode editGroupNode, Node backgroundNode) : base(worldGroupNode, backgroundNode)
		{
			m_markersGroupNode = new GroupNode("Edit_Markers");

			m_editGroupNode = editGroupNode;
			m_editGroupNode.AddChild(m_markersGroupNode);

			CreateRGBMaterials();

			MarkerSnapRadius = 2;
		}



		public bool Undo()
		{
			if (m_doneStack.Count == 0)
			{
				return false;
			}
			EditorCommand command = m_doneStack.Pop();
			command.Undo();
			m_redoStack.Push(command);
			return true;
		}



		public bool Redo()
		{
			if (m_redoStack.Count == 0)
			{
				return false;
			}
			EditorCommand command = m_redoStack.Pop();
			command.Do();
			m_doneStack.Push(command);
			return true;
		}



		public Marker GetMarker(GeometryNode geometryNode)
		{
			foreach (Marker marker in m_markers)
			{
				if (marker.GameObject == geometryNode)
				{
					return marker;
				}
			}
			return null;
		}



		public override void FrameStepUpdate(float deltaRealTime, float deltaGameTime)
		{
			base.FrameStepUpdate(deltaRealTime, deltaGameTime);

			m_editGroupNode.Enabled = EditMode;
			if (EditMode)
			{
				UpdateMarkers();

				GlowSelectedRailPieces();
			}
		}

	
		
		public override void TimeStepUpdate(float deltaRealTime, float deltaGameTime)
		{
			if (EditMode)
			{
				if (m_selectionGlowFactorIncrease)
				{
					m_selectionGlowFactor += 2.5f * deltaRealTime;
					if (m_selectionGlowFactor > 1)
					{
						m_selectionGlowFactor = 1;
						m_selectionGlowFactorIncrease = false;
					}
				}
				else
				{
					m_selectionGlowFactor -= 2.5f * deltaRealTime;
					if (m_selectionGlowFactor < 0)
					{
						m_selectionGlowFactor = 0;
						m_selectionGlowFactorIncrease = true;
					}
				}
			}
			base.TimeStepUpdate(deltaRealTime, deltaGameTime);
		}



		public bool GetClosestConnector(
			Vector3 position,
			RailPiece[] railPiecesToExclude,
			out Vector3 closestPosition,
			out Vector3 closestDirection)
		{
			return m_railSystem.GetClosestConnector
			(
				position,
				railPiecesToExclude,
				out closestPosition,
				out closestDirection
			);
		}



		/// <summary>
		/// Returns the number of selected rail pieces.
		/// </summary>
		/// <returns>
		/// The number of selected rail pieces
		/// </returns>
		public int GetSelectedRailPieceCount()
		{
			return m_selectedRailPieces.Count;
		}



		/// <summary>
		/// Copy all rail pieces in the selection to our clipboard (no relation
		/// to the Windows clipboard). Keep the old clipboard contents if the
		/// selection is empty.
		/// </summary>
		public void CopySelectionToClipboard()
		{
			if (GetSelectedRailPieceCount() == 0)
			{
				// Don't clear old contents if selection is empty
				return;
			}

			m_clipboardEntries.Clear();

			foreach (RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				var entry = new ClipboardEntry
				{
					Definition = railPiece.Definition,
					WorldTransformation = railPiece.Transform.GetWorldTransformation()
				};
				if (entry.Definition.IsFlexible)
				{
					// Also copy the positions of the control points of a flexiable rail piece
					Debug.Assert(railPiece.RailSegments.Length == 1, "TODO implement");
					entry.FlexibleControlPoints = (Vector3[]) railPiece.RailSegments[0].Curve.ControlPoints.Clone();
				}
				m_clipboardEntries.Add(entry);
			}
		}



		/// <summary>
		/// Paste the contents of the clipboard centred around the given position.
		/// </summary>
		/// <param name="positionInWorldSpace">
		/// The pasted rail pieces are centred around this position (in world space)
		/// </param>
		public void PasteFromClipboard(Vector3 positionInWorldSpace)
		{
			if (m_clipboardEntries.Count == 0)
			{
				return;
			}

			Vector3 centre = GetCentrePosition(m_clipboardEntries);
			Vector3 offset = positionInWorldSpace - centre;
			var definitionNames = new List<string>();
			var transforms = new List<Matrix>();
			var controlPoints = new List<Vector3[]>();
			foreach (ClipboardEntry clipboardEntry in m_clipboardEntries)
			{
				definitionNames.Add(clipboardEntry.Definition.Name);
				Matrix transform = clipboardEntry.WorldTransformation;
				transform.Translation += offset;
				transforms.Add(transform);
				controlPoints.Add((Vector3[])clipboardEntry.FlexibleControlPoints.Clone());
			}
			CreateRailPieces(definitionNames, transforms, controlPoints);
		}


		private void ChangeRailPieceID(int oldID, int newID)
		{
			RailPiece railPiece = m_railPieces[oldID];
			m_railPieces.Remove(oldID);
			railPiece.ID = newID;
			railPiece.GameObject.ID = newID;
			m_railPieces.Add(newID, railPiece);
		}


		private void CreateRailPieces(List<string> definitionNames, List<Matrix> transformations, List<Vector3[]> controlPoints)
		{
			var command = new CreateRailPiecesEditorCommand(this, definitionNames, transformations, controlPoints);
			Do(command);
		}



		public override void CreateRailPiece(string definitionName, Matrix transformation, Vector3[] controlPoints)
		{
			var command = new CreateRailPiecesEditorCommand(
				this, 
				new List<string> { definitionName }, 
				new List<Matrix> { transformation }, 
				new List<Vector3[]> { controlPoints } );
			Do(command);
		}



		protected override void DestroyRailPiece(int id)
		{
			var command = new DestroyRailPiecesEditorCommand(this, new List<int> { id });
			Do(command);
		}


		public void RemoveSelectedRailPieces()
		{
			var ids = new List<int>();
			foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				ids.Add(railPiece.ID);
			}

			var command = new DestroyRailPiecesEditorCommand(this, ids);
			Do(command);
		}



		public override void DestroyAllRailPieces()
		{
			SelectNone();
			base.DestroyAllRailPieces();
		}



		public void CreateSampleRailPieces()
		{
			float z = 0;
			foreach(string definitionName in m_railPieceDefinitions.Keys)
			{
				Matrix transformation = Matrix.CreateTranslation(new Vector3(-60, 0, z));
				CreateRailPiece(definitionName, transformation, null);
				z += 20;
			}
		}



		public void TogglemarkersDisplay()
		{
			DisplayMarkers = !DisplayMarkers;
		}



		public void DrawRailPieceIDs(Graphics.Viewport viewport, SpriteFont font, SpriteBatch spriteBatch)
		{
			var shadowPos = new Vector2();

			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
			foreach (RailSegment railSegment in m_railSystem.RailSegments)
			{
				Vector3 textPos3D = railSegment.GetCurvePositionInWorldSpace(0.5f);
				Vector2 textPos = viewport.ProjectPoint(textPos3D);
				textPos.Y -= font.LineSpacing;
				shadowPos.X = textPos.X + 1;
				shadowPos.Y = textPos.Y + 1;

				var builder = new StringBuilder();
				foreach (RailSegment.ConnectedSegmentInfo connectedSegmentInfo in railSegment.ConnectedSegmentInfos)
				{
					if (connectedSegmentInfo.AsPrevious)
					{
						builder.Append(connectedSegmentInfo.RailSegment.RailPiece.ID);
						builder.Append(' ');
					}
				}
				builder.AppendFormat(": {0} : ", railSegment.RailPiece.ID);
				foreach (RailSegment.ConnectedSegmentInfo connectedSegmentInfo in railSegment.ConnectedSegmentInfos)
				{
					if (!connectedSegmentInfo.AsPrevious)
					{
						builder.Append(connectedSegmentInfo.RailSegment.RailPiece.ID);
						builder.Append(' ');
					}
				}
				string str = builder.ToString();
				spriteBatch.DrawString(font, str, textPos, Color.Black);
				spriteBatch.DrawString(font, str, shadowPos, Color.White);
			}
			spriteBatch.End();
		}



		public void ClearUndoRedoStack()
		{
			m_doneStack.Clear();
			m_redoStack.Clear();
			Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("Undo/Redo stack CLEAR");
		}



		public override bool Load(string fileName, EffectManager effectManager, TextureManager textureManager, MyEffect[] effects)
		{
			if (base.Load(fileName, effectManager, textureManager, effects))
			{
				ClearUndoRedoStack();
				return true;			
			}
			return false;
		}



		public void SelectAll()
		{
			// Delta is to select all unselected railpieces
			var idsToAdd = new List<int>();
			foreach(RailPiece railPiece in m_railPieces.Values)
			{
				if (!m_selectedRailPieces.ContainsKey(railPiece))
				{
					idsToAdd.Add(railPiece.ID);
				}
			}

			var command = new ChangeSelectionEditorCommand(this, idsToAdd, null);
			Do(command);
		}



		public void SelectConnectedToSelection()
		{
			// TODO select all railpieces connected to any railpiece in the selection
		}



		public void SelectNone()
		{
			// Delta is to deselect all selected railpieces
			var idsToRemove = new List<int>();
			foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				idsToRemove.Add(railPiece.ID);
			}

			var command = new ChangeSelectionEditorCommand(this, null, idsToRemove);
			Do(command);
		}



		public void SelectInvert()
		{
			if (LockSelection)
			{
				return;
			}

			var idsToRemove = new List<int>();
			foreach (RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				idsToRemove.Add(railPiece.ID);
			}

			var idsToAdd = new List<int>();
			foreach (RailPiece railPiece in m_railPieces.Values)
			{
				if (!IsSelectedRailPiece(railPiece))
				{
					idsToAdd.Add(railPiece.ID);
				}
			}

			var command = new ChangeSelectionEditorCommand(this, idsToAdd, idsToRemove);
			Do(command);
		}
		


		public void SelectSingle(RailPiece railPieceToSelect)
		{
			// Delta is to deselect all selected railpieces and select the specified railpiece
			var idsToRemove = new List<int>();
			foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				idsToRemove.Add(railPiece.ID);
			}
			var idsToAdd = new List<int> {railPieceToSelect.ID};

			var command = new ChangeSelectionEditorCommand(this, idsToAdd, idsToRemove);
			Do(command);
		}



		public void SelectToggleSingle(RailPiece railPiece)
		{
			// Delta is to deselect the rp is it's selected and vice versa
			var idToToggle = new List<int> { railPiece.ID };
			List<int> idToAdd;
			List<int> idToRemove;
			if (IsSelectedRailPiece(railPiece))
			{
				idToAdd = null;
				idToRemove = idToToggle;
			}
			else
			{
				idToAdd = idToToggle;
				idToRemove = null;
			}
			var command = new ChangeSelectionEditorCommand(this, idToAdd, idToRemove);
			Do(command);
		}

	
		
		public bool IsSelectedRailPiece(RailPiece railPiece)
		{
			return m_selectedRailPieces.ContainsKey(railPiece);
		}



		public void SelectionRotateBegin()
		{
			m_selectionTransformsBefore = GetSelectionTransforms();
		}



		public void SelectionRotate(float angleYInWorldSpace, Vector3 rotationOrigin)
		{
			foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				railPiece.Transform.Transform *= CreateYRotationTransform(-angleYInWorldSpace, rotationOrigin);
			}
		}



		public void SelectionRotateEnd()
		{
			if (m_selectionTransformsBefore.Count == 0)
			{
				return;
			}

			List<Matrix> selectionTransformsAfter = GetSelectionTransforms();
			if (m_selectionTransformsBefore[0] != selectionTransformsAfter[0])
			{
				List<int> selectionIDs = GetSelectionIDs();
				var command = new TransformSelectionEditorCommand(this, selectionIDs, m_selectionTransformsBefore, selectionTransformsAfter);
				Do(command);
			}
		}


		
		public void SelectionTranslateBegin()
		{
			m_selectionTransformsBefore = GetSelectionTransforms();
			m_selectionUnsnappedTransforms = GetSelectionTransforms();
		}



		public void SelectionTranslate(Vector3 deltaPosition)
		{
			if (deltaPosition == Vector3.Zero)
			{
				return;
			}

			int k = 0;
			foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				railPiece.Transform.Transform = m_selectionUnsnappedTransforms[k++];
				railPiece.Transform.Transform.Translation += deltaPosition;
			}
			m_selectionUnsnappedTransforms = GetSelectionTransforms();

			// TODO temporary: works, but is total overkill
			RemoveAndAddAllRailSegments();
			m_railSystem.SetDefaultPath();
			CreatePointsIndicators();

			var railPiecesToExclude = new RailPiece[m_selectedRailPieces.Keys.Count];
			m_selectedRailPieces.Keys.CopyTo(railPiecesToExclude, 0);

			float closestDistance = 1e30f;
			Vector3 closestPositionInRailSystem = Vector3.Zero;
			Vector3 closestDirectionInRailSystem = Vector3.UnitX;
			RailSegment closestSegmentInSelection = null;
			bool closestAtStartInSelection = true;
			foreach (RailPiece selectedRailPiece in m_selectedRailPieces.Keys)
			{
				foreach (RailSegment selectedSegment in selectedRailPiece.RailSegments)
				{
					// Get closest connector in rail system wrt start of selected segment
					Vector3 closestPosition1;
					Vector3 closestDirection1;
					Vector3 connectorPosition = selectedSegment.GetConnectorPositionInWorldSpace(true);
					m_railSystem.GetClosestConnector
					(
						connectorPosition, 
						railPiecesToExclude, 
						out closestPosition1,
						out closestDirection1
					);
					float distance1 = (closestPosition1 - connectorPosition).Length();

					// Get closest connector in rail system wrt end of selected segment
					Vector3 closestPosition2;
					Vector3 closestDirection2;
					connectorPosition = selectedSegment.GetConnectorPositionInWorldSpace(false);
					m_railSystem.GetClosestConnector
					(
						connectorPosition,
						railPiecesToExclude,
						out closestPosition2,
						out closestDirection2
					);

					// Determine closest of those two
					float distance2 = (closestPosition2 - connectorPosition).Length();
					if (distance1 < distance2)
					{
						if (distance1 < closestDistance)
						{
							closestDistance = distance1;

							closestPositionInRailSystem = closestPosition1;
							closestDirectionInRailSystem = closestDirection1;
							closestSegmentInSelection = selectedSegment;
							closestAtStartInSelection = true;
						}
					}
					else
					{
						if (distance2 < closestDistance)
						{
							closestDistance = distance2;

							closestPositionInRailSystem = closestPosition2;
							closestDirectionInRailSystem = closestDirection2;
							closestSegmentInSelection = selectedSegment;
							closestAtStartInSelection = false;
						}
					}
				}
			}

			if (closestSegmentInSelection != null && closestDistance < MarkerSnapRadius)
			{
				// Get direction of curve of rail system segment connector in world space
				Vector3 connectorDirectionInRailSystem = closestDirectionInRailSystem;

				// Get direction of curve of selected segment connector in worldspace
				Vector3 connectorDirectionInSelection = closestSegmentInSelection.GetConnectorTangentInWorldSpace(closestAtStartInSelection);

				// Rotate selection so that curves line up
				double angleInRailSystem = Math.Atan2(-connectorDirectionInRailSystem.Z, -connectorDirectionInRailSystem.X);
				double angleInSelection = Math.Atan2(connectorDirectionInSelection.Z, connectorDirectionInSelection.X);
				double deltaAngle = angleInRailSystem - angleInSelection;
				if (deltaAngle > Math.PI)
				{
					deltaAngle -= 2 * Math.PI;
				}
				else if (deltaAngle < -Math.PI)
				{
					deltaAngle += 2 * Math.PI;
				}
				var angle = (float)(Math.Abs(deltaAngle) < 0.5 * Math.PI ? -deltaAngle : Math.PI - deltaAngle);

				// Rotate selection to line up connector directions
				Vector3 rotationOrigin = closestSegmentInSelection.RailPiece.Transform.Transform.Translation;
				foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
				{
					railPiece.Transform.Transform *= CreateYRotationTransform(angle, rotationOrigin);
				}

				// Translate selection to snap to closest rail segment
				Vector3 connectorPosition = closestSegmentInSelection.GetConnectorPositionInWorldSpace(closestAtStartInSelection);
				Vector3 translation = closestPositionInRailSystem - connectorPosition;
				foreach(RailPiece railPiece in m_selectedRailPieces.Keys)
				{
					railPiece.Transform.Transform.Translation += translation;
				}
			}
 		}



		public void SelectionTranslateEnd()
		{
			List<int> selectionIDs = GetSelectionIDs();
			List<Matrix> selectionTransformsAfter = GetSelectionTransforms();
			if (m_selectionTransformsBefore[0] != selectionTransformsAfter[0])
			{
				var command = new TransformSelectionEditorCommand(this, selectionIDs, m_selectionTransformsBefore, selectionTransformsAfter);

				Do(command);
			}
		}



		private static Vector3 GetCentrePosition(IList<ClipboardEntry> clipboardEntries)
		{
			if (clipboardEntries.Count == 0)
			{
				return Vector3.Zero;
			}
			Vector3 centre = clipboardEntries[0].WorldTransformation.Translation;
			for (int k = 1; k < clipboardEntries.Count; k++)
			{
				centre += clipboardEntries[k].WorldTransformation.Translation;
			}
			centre /= clipboardEntries.Count;
			return centre;
		}


		
		private static Matrix CreateYRotationTransform(float angle, Vector3 rotationOrigin)
		{
			return Matrix.CreateTranslation(-rotationOrigin) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(rotationOrigin);
		}



		private void CreateRGBMaterials()
		{
			var redMaterial = new Material 
          	{
          		Ambient = Colour.PureRed, 
          		Diffuse = Colour.PureRed
          	};

			var redMaterialTable = new MaterialTable();
			redMaterialTable.Materials.Add(redMaterial);
			m_markerMaterialTables.Add(redMaterialTable);

			var blueMaterial = new Material 
           	{
           		Ambient = Colour.PureBlue, 
           		Diffuse = Colour.PureBlue
           	};

			var blueMaterialTable = new MaterialTable();
			blueMaterialTable.Materials.Add(blueMaterial);
			m_markerMaterialTables.Add(blueMaterialTable);

			var greenMaterial = new Material
        	{
        		Ambient = Colour.PureGreen, 
        		Diffuse = Colour.PureGreen
        	};

			var greenMaterialTable = new MaterialTable();
			greenMaterialTable.Materials.Add(greenMaterial);
			m_markerMaterialTables.Add(greenMaterialTable);
		}

	

		private void Do(EditorCommand command)
		{
			command.Do();
			m_doneStack.Push(command);
			m_redoStack.Clear();
		}

	

		private void AddToSelection(int id)
		{
			RailPiece railPiece = m_railPieces[id];
			Debug.Assert(!m_selectedRailPieces.ContainsKey(railPiece));

			RailPieceSelectionInfo railPieceSelectionInfo;
			railPieceSelectionInfo.OriginalMaterialTable = railPiece.GameObject.MaterialTable;
			railPieceSelectionInfo.SelectedMaterialTable = new MaterialTable(railPiece.GameObject.MaterialTable, true);
			railPiece.GameObject.MaterialTable = railPieceSelectionInfo.SelectedMaterialTable;
			m_selectedRailPieces.Add(railPiece, railPieceSelectionInfo);
		}

	
	
		private void RemoveFromSelection(int id)
		{
			RailPiece railPiece = m_railPieces[id];
			Debug.Assert(m_selectedRailPieces.ContainsKey(railPiece));
			RailPieceSelectionInfo selectionInfo = m_selectedRailPieces[railPiece];
			railPiece.GameObject.MaterialTable = selectionInfo.OriginalMaterialTable;
			m_selectedRailPieces.Remove(railPiece);
		}



		private void CreateRailPieceInternal(string definitionName, Vector3 position, Quaternion rotation, Vector3[] controlPoints)
		{
			base.CreateRailPiece(definitionName, position, rotation, controlPoints);
		}



		private void DestroyRailPieceInternal(int id)
		{
			base.DestroyRailPiece(id);
		}



		private void UpdateMarker(RailSegment segment, int pointIndex, int markerIndex, Vector3 markerPosition, MaterialTable materialTable)
		{
			if (markerIndex >= m_markers.Count)
			{
				var markerGeometry = GeometryNode.CreateCube(1.2f, materialTable);
				var markerTransform = new TransformNode("Marker");
				markerTransform.AddChild(markerGeometry);
				m_markersGroupNode.AddChild(markerTransform);

				var newMarker = new Marker(markerGeometry, markerTransform);
				m_markers.Add(newMarker);
			}
			
			Marker marker = m_markers[markerIndex];
			marker.TransformNode.Transform.Translation = markerPosition;
			marker.GameObject.MaterialTable = materialTable;
			marker.TransformNode.Enabled = true;
			marker.RailSegment = segment;
			marker.ControlPointIndex = pointIndex;
		}



		private void UpdateMarkers()
		{
			int markerIndex = 0;
			foreach(RailPiece railPiece in m_railPieces.Values)
			{
				foreach(RailSegment segment in railPiece.RailSegments)
				{
					// Draw marker at start of segment curve (t = 0) (not for unconnected rigid rail piece endpoints)
					Vector3 markerPositionPrevious = Vector3.Transform(segment.Curve.GetPosition(0), segment.WorldTransformation);
					markerPositionPrevious.Y += railPiece.RailTopHeight;
					if (segment.GetConnectionCount(true) == 0)
					{
						if (railPiece is FlexibleRailPiece)
						{
							UpdateMarker(segment, 0, markerIndex, markerPositionPrevious, m_markerMaterialTables[0]);
							markerIndex++;
						}
					}
					else
					{
						UpdateMarker(segment, 0, markerIndex, markerPositionPrevious, m_markerMaterialTables[2]);
						markerIndex++;
					}

					// Draw marker at end of segment curve (t = 1) (not for unconnected rigid rail piece endpoints)
					Vector3 markerPositionNext = Vector3.Transform(segment.Curve.GetPosition(1), segment.WorldTransformation);
					markerPositionNext.Y += railPiece.RailTopHeight;
					if (segment.GetConnectionCount(false) == 0)
					{
						if (railPiece is FlexibleRailPiece)
						{
							UpdateMarker(segment, 3, markerIndex, markerPositionNext, m_markerMaterialTables[0]);
							markerIndex++;
						}
					}
					else
					{
						UpdateMarker(segment, 3, markerIndex, markerPositionNext, m_markerMaterialTables[2]);
						markerIndex++;
					}

					// Draw 'shape'control point markers for flexible segments
					if (railPiece is FlexibleRailPiece)
					{
						for (int k = 1; k <= 2; k++)
						{
							Vector3 controlPointPosition = Vector3.Transform(segment.Curve.ControlPoints[k], segment.WorldTransformation);
							controlPointPosition.Y += railPiece.RailTopHeight;
							UpdateMarker(segment, k, markerIndex, controlPointPosition, m_markerMaterialTables[1]);
							markerIndex++;
						}

					}
				}
			}

			// Disable all other markers
			for (int k = markerIndex; k < m_markers.Count; k++)
			{
				m_markers[k].TransformNode.Enabled = false;
			}
		}
	
		
		
		private void GlowSelectedRailPieces()
		{
			foreach(RailPieceSelectionInfo selectionInfo in m_selectedRailPieces.Values)
			{
				for (int materialIndex = 0 ; materialIndex < selectionInfo.SelectedMaterialTable.Materials.Count; materialIndex++)
				{
					Material selectedMaterial = selectionInfo.SelectedMaterialTable.Materials[materialIndex];
					Material originalMaterial = selectionInfo.OriginalMaterialTable.Materials[materialIndex];

					float f = m_selectionGlowFactor + 1;
					selectedMaterial.Ambient = new Colour(originalMaterial.Ambient.Red * f, originalMaterial.Ambient.Green * f, originalMaterial.Ambient.Blue * f, originalMaterial.Ambient.Alpha);
				}
			}
		}



		private void SetSelectionTransforms(IList<int> ids, IList<Transform> transforms)
		{
			Debug.Assert(ids.Count == transforms.Count);

			foreach (RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				for (int k = 0; k < ids.Count; k++)
				{
					if (railPiece.ID == ids[k])
					{
						railPiece.Transform.Transform = transforms[k];
						break;
					}
				}
			}
		}



		private List<Transform> GetSelectionTransforms()
		{
			var transforms = new List<Transform>();
			foreach (RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				transforms.Add(railPiece.Transform);
			}
			return transforms;
		}

	
		
		private List<int> GetSelectionIDs()
		{
			var ids = new List<int>();
			foreach (RailPiece railPiece in m_selectedRailPieces.Keys)
			{
				ids.Add(railPiece.ID);
			}
			return ids;
		}

	
		
		private List<Matrix> m_selectionTransformsBefore;
		private List<Matrix> m_selectionUnsnappedTransforms;



		public bool SelectionEmpty()
		{
			return m_selectedRailPieces.Count == 0;
		}
	}
}

*/