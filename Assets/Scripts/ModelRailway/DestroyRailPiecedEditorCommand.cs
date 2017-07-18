using System.Collections.Generic;
using UnityEngine;

/*

namespace ModelRailway
{

partial class EditableWorld
{
	private class DestroyRailPiecesEditorCommand : EditorCommand
	{
		public DestroyRailPiecesEditorCommand(EditableWorld world, List<int> ids) : base(world)
		{
			m_ids				= ids;
			m_definitionNames	= new List<string>();
			m_positions			= new List<Vector3>();
			m_rotations			= new List<Quaternion>();
			m_controlPoints		= new List<Vector3[]>();
			m_selected			= new List<bool>();
		}



		public override void Do()
		{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("DestroyRailPieces DO");
			foreach (int id in m_ids)
			{
				RailPiece railPiece = m_editableWorld.GetRailPiece(id);

				Vector3[] controlPoints = null;
				if (railPiece is FlexibleRailPiece)
				{
					var flexPiece = railPiece as FlexibleRailPiece;
					Debug.Assert(flexPiece.RailSegments.Length == 1);
					controlPoints = flexPiece.RailSegments[0].Curve.ControlPoints;
				}

				m_definitionNames.Add(railPiece.Definition.Name);
				m_positions.Add(railPiece.Transform.localPosition);
				m_rotations.Add(railPiece.Transform.localRotation);
				m_controlPoints.Add(controlPoints);
				m_selected.Add(m_editableWorld.m_selectedRailPieces.ContainsKey(railPiece));

				m_editableWorld.RemoveFromSelection(id);
				m_editableWorld.DestroyRailPieceInternal(id);
			}
		}



		public override void Undo()
		{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("DestroyRailPieces UNDO");
			for (int k = 0; k < m_ids.Count; k++)
			{
				m_editableWorld.CreateRailPieceInternal(m_definitionNames[k], m_positions[k], m_rotations[k], m_controlPoints[k]);
				RailPiece railPiece = m_editableWorld.GetLastCreatedRailPiece();
				m_editableWorld.ChangeRailPieceID(railPiece.ID, m_ids[k]);
				if (m_selected[k])
				{
					m_editableWorld.AddToSelection(m_ids[k]);
				}
			}
		}



		//- PRIVATE --------------------------------------------------------------------------


		private readonly List<string>		m_definitionNames;
		private readonly List<int>			m_ids;
		private readonly List<Vector3>		m_positions;
		private readonly List<Quaternion>	m_rotations;
		private readonly List<Vector3[]>	m_controlPoints;
		private readonly List<bool>			m_selected;
	}
}

}
*/