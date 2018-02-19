using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakfastAcademy : Academy
{
    public float randomTarget;
    public override void InitializeAcademy()
    {
        Monitor.verticalOffset = 0.5f;
    }

    public override void AcademyReset()
    {
        randomTarget = (float)resetParameters["random_target"];

    }

    public override void AcademyStep()
    {


    }
}
