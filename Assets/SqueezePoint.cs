using UnityEngine;

public enum SqueezeBehaviour { Standing,Crouched};
public class SqueezePoint : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private SqueezeBehaviour behaviour = SqueezeBehaviour.Standing;
}
