using UnityEngine;



namespace Game
{

public class Game : MonoBehaviour
{
	public GameSetup	Settings;



	public virtual void Awake()		// Called by Unity
	{
		m_controls = new Controls(Settings.Controls);
	}



	public void Update()	// Called by Unity
	{
		m_controls.Update();

		TimestepUpdate(Time.deltaTime);
		FramestepUpdate(Time.deltaTime);
	}



	//- PROTECTED ----------------------------------------------------------------------



	protected virtual void TimestepUpdate(float deltaTime)
	{
	}



	protected virtual void FramestepUpdate(float deltaTime)
	{
	}



	protected bool SetControlFireCallback(string controlName, Controls.FireCallback callback)
	{
		return m_controls.SetFireCallback(controlName, callback);
	}



	protected bool SetControlHeldCallback(string controlName, Controls.HeldCallback callback)
	{
		return m_controls.SetHeldCallback(controlName, callback);
	}



	//- PRIVATE ------------------------------------------------------------------------



	private Controls m_controls;

}

}
