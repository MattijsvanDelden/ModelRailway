namespace ModelRailway
{

partial class EditableWorld
{
	private abstract class EditorCommand
	{
		protected EditorCommand(EditableWorld world)
		{
			m_editableWorld = world;
		}

		public abstract void Do();
		public abstract void Undo();

		protected readonly EditableWorld m_editableWorld;
	}
}

}
