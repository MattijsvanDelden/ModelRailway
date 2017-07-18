using System;

namespace ModelRailway
{

class Engine : TrainCar
{
	public float GoalSpeed { get; set; }

	public float PositiveAcceleration { get; set; }

	public float NegativeAcceleration { get; set; }

	public float RelativeThrottle { get; set; }

	

	public Engine(TrainCarDefinition definition) : base(definition)
	{
		//m_mass = 1;
		PositiveAcceleration = 2;
		NegativeAcceleration = 0.75f * PositiveAcceleration;
	}



	public override void TimestepUpdate(float deltaTime)
	{
		const float EngineMotorMaxForce = 0.4f;
		const float EngineMotorForceDrop = 0.1f;
		const float EngineMotorBrakeForce = 0.2f;

		float motorForce;
		if (RelativeThrottle != 0)
		{
			float motorEfficiency = 1 - Math.Abs(m_signedSpeed) * EngineMotorForceDrop;
			if (motorEfficiency < 0)
				motorEfficiency = 0 ;

			motorForce = RelativeThrottle * EngineMotorMaxForce * motorEfficiency ;
		}
		else
		{
			motorForce = -m_signedSpeed * EngineMotorBrakeForce;
		}
		AddLocalForce(motorForce);


		if (m_signedSpeed >= 0)
		{
			if (m_signedSpeed < GoalSpeed)
			{
				m_signedSpeed += PositiveAcceleration * deltaTime;
				if (m_signedSpeed > GoalSpeed)
				{
					m_signedSpeed = GoalSpeed;
				}
			}
			else
			{
				m_signedSpeed -= NegativeAcceleration * deltaTime;
				if (m_signedSpeed < GoalSpeed)
				{
					m_signedSpeed = GoalSpeed;
				}
			}
		}
		else if (m_signedSpeed <= 0)
		{
			if (m_signedSpeed > GoalSpeed)
			{
				m_signedSpeed -= PositiveAcceleration * deltaTime;
				if (m_signedSpeed < GoalSpeed)
				{
					m_signedSpeed = GoalSpeed;
				}
			}
			else
			{
				m_signedSpeed += NegativeAcceleration * deltaTime;
				if (m_signedSpeed > GoalSpeed)
				{
					m_signedSpeed = GoalSpeed;
				}
			}
		}

		base.TimestepUpdate(deltaTime);
	}

}

}