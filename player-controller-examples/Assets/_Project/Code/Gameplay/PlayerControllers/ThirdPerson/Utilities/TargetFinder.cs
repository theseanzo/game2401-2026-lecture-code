using UnityEngine;
using _Project.Code.Gameplay.Combat;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson.Utilities
{
    public static class TargetFinder
    {
        public static Transform FindNearestTarget(Transform origin, float radius, float maxAngle)
        {
            var colliders = Physics.OverlapSphere(origin.position, radius);

            Transform nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider.transform == origin) continue;

                var targetable = collider.GetComponent<ITargetable>();
                if (targetable == null || !targetable.IsTargetable) continue;

                // Check if target is within the forward-facing cone
                Vector3 directionToTarget = (collider.transform.position - origin.position).normalized;
                float angleToTarget = Vector3.Angle(origin.forward, directionToTarget);

                if (angleToTarget > maxAngle)
                    continue;

                float distance = Vector3.Distance(origin.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = collider.transform;
                }
            }

            return nearest;
        }
    }
}