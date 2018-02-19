using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterAcademy : Academy {

	public int numObstacles = 0;
    public override void InitializeAcademy()
    {
        Monitor.verticalOffset = 0.5f;
    }

    public override void AcademyReset()
    {
        numObstacles = (int)resetParameters["num_obstacles"];

    }

    public override void AcademyStep()
    {


    }
}
