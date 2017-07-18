using System.Collections.Generic;
/*
namespace ModelRailway
{
	partial class EditableWorld
	{
		private class ChangeSelectionEditorCommand : EditorCommand
		{
			public ChangeSelectionEditorCommand(EditableWorld world, List<int> idsToAdd, List<int> idsToRemove) : base(world)
			{
				m_idsToAdd = idsToAdd;
				m_idsToRemove = idsToRemove;
			}



			public override void Do()
			{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("ChangeSelection DO");
				if (m_idsToAdd != null)
				{
					foreach (int id in m_idsToAdd)
					{
						m_editableWorld.AddToSelection(id);
					}
				}
				if (m_idsToRemove != null)
				{
					foreach (int id in m_idsToRemove)
					{
						m_editableWorld.RemoveFromSelection(id);
					}
				}
			}

			
			
			public override void Undo()
			{
//				Graphics.XNAGame.TheXNAGame.MessageManager.AddMessage("ChangeSelection UNDO");
				if (m_idsToAdd != null)
				{
					foreach (int id in m_idsToAdd)
					{
						m_editableWorld.RemoveFromSelection(id);
					}
				}
				if (m_idsToRemove != null)
				{
					foreach (int id in m_idsToRemove)
					{
						m_editableWorld.AddToSelection(id);
					}
				}
			}



			private readonly List<int> m_idsToAdd;
			private readonly List<int> m_idsToRemove;
		}
	}
}
*/