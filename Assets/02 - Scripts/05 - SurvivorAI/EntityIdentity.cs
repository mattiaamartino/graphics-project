using UnityEngine;

public enum EntityType
{
    Human,
    Predator,
    Prey,
    Vegetable,
    Fish
}

public class EntityIdentity : MonoBehaviour
{
    public EntityType type;
}
