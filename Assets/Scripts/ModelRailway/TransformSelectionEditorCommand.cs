using System.Collections.Generic;
using UnityEngine;



namespace ModelRailway
{

partial class EditableWorld
{
	private class TransformSelectionEditorCommand : EditorCommand
	{
		public TransformSelectionEditorCommand(EditableWorld world, List<int> ids, List<Transform> transformsBefore, List<Transform> transformsAfter) : base(world)
		{
			m_ids = ids;
			m_transformsBefore = transformsBefore;
			m_transformsAfter = transformsAfter;
		}



		public override void Do()
		{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("TransformSelection DO");
//			m_editableWorld.SetSelectionTransforms(m_ids, m_transformsAfter);
		}



		public override void Undo()
		{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("TransformSelection UNDO");
//			m_editableWorld.SetSelectionTransforms(m_ids, m_transformsBefore);
		}



		// Note: for this command, we don't use a 'delta', because 
		// using a delta of the transform and inverting that and multiplying
		// it with the old transform is not exact
		private readonly List<int> m_ids;
		private readonly List<Transform> m_transformsBefore;
		private readonly List<Transform> m_transformsAfter;
	}
}

}
