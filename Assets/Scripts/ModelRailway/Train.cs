/*

using System.Diagnostics;

namespace ModelRailway
{
	static class Train
	{
		public static TrainCar GetFront(TrainCar trainCar)
		{
			Debug.Assert(trainCar != null);
			while (trainCar.Previous != null)
			{
				trainCar = trainCar.Previous;
			}
			return trainCar;
		}



		public static void SetRelativeThrottle(TrainCar trainCar, float relativeThrottle)
		{
			trainCar = GetFront(trainCar);
			while (trainCar != null)
			{
				if (trainCar is Engine)
				{
					(trainCar as Engine).RelativeThrottle = relativeThrottle;
				}
				trainCar = trainCar.Next;
			}
		}
	}
}

*/