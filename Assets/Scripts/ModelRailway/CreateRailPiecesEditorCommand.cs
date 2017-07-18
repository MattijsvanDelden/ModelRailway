using System.Collections.Generic;
using UnityEngine;

/*

namespace ModelRailway
{

partial class EditableWorld
{
	private class CreateRailPiecesEditorCommand : EditorCommand
	{
		public CreateRailPiecesEditorCommand(
			EditableWorld world, 
			List<string> definitionNames, 
			List<Vector3> positions, 
			List<Quaternion> rotations, 
			List<Vector3[]> controlPoints) : base(world)
		{
			m_definitionNames	= definitionNames;
			m_positions = positions;
			m_rotations = rotations;
			m_controlPoints = controlPoints;
			m_ids = null;
		}



		public override void Do()
		{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("CreateRailPieces DO");
			bool useExistingIDs = true;
			if (m_ids == null)
			{
				m_ids = new List<int>();
				useExistingIDs = false;
			}
			for (int k = 0; k < m_definitionNames.Count; k++)
			{
				m_editableWorld.CreateRailPieceInternal(m_definitionNames[k], m_positions[k], m_rotations[k], m_controlPoints[k]);
				RailPiece railPiece = m_editableWorld.GetLastCreatedRailPiece();
				if (useExistingIDs)
				{
					m_editableWorld.ChangeRailPieceID(railPiece.ID, m_ids[k]);
				}
				else
				{
					m_ids.Add(railPiece.ID);
				}
			}
		}



		public override void Undo()
		{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("CreateRailPieces UNDO");
			foreach (int id in m_ids)
			{
				m_editableWorld.DestroyRailPieceInternal(id);
			}
		}



		private readonly List<string>		m_definitionNames;
		private readonly List<Vector3>		m_positions;
		private readonly List<Quaternion>	m_rotations;
		private readonly List<Vector3[]>	m_controlPoints;
		private List<int>					m_ids;
	}
}

}
*/