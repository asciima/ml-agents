using UnityEngine;
using System.Collections;


public class RotationLimitLimb : RotationLimit {


	#region Main Interface
	

	[Range(0f, 180f)] public float limit = 45;
	[Range(0f, 180f)] public float twistLimit = 180;
	
	#endregion Main Interface
	
	/*
		* Limits the rotation in the local space of this instance's Transform.
		* */
	protected override Quaternion LimitRotation(Quaternion rotation) {		
		// Subtracting off-limits swing
		Quaternion swing = LimitSwing(rotation);
		
		// Apply twist limits
		return LimitTwist(swing, axis, secondaryAxis, twistLimit);
	}
	
	/*
		* Apply swing limits
		* */
	private Quaternion LimitSwing(Quaternion rotation) {
		if (axis == Vector3.zero) return rotation; // Ignore with zero axes
		if (rotation == Quaternion.identity) return rotation; // Assuming initial rotation is in the reachable area
		if (limit >= 180) return rotation;
		
		Vector3 swingAxis = rotation * axis;
		
		// Get the limited swing axis
		Quaternion swingRotation = Quaternion.FromToRotation(axis, swingAxis);
		Quaternion limitedSwingRotation = Quaternion.RotateTowards(Quaternion.identity, swingRotation, limit);
		
		// Rotation from current(illegal) swing rotation to the limited(legal) swing rotation
		Quaternion toLimits = Quaternion.FromToRotation(swingAxis, limitedSwingRotation * axis);
		
		// Subtract the illegal rotation
		return toLimits * rotation;
	}
}

