using UnityEngine;
using System.Collections.Generic;

public class SurvivorAgent : MonoBehaviour
{
    [Header("Identity")]
    public EntityIdentity identity;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyLossRate = 1f;
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 5f;

    [Header("Vision")]
    public float visionRadius = 15f;
    public float fieldOfViewAngle = 120f;

    [Header("Boids (Fish Only)")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float neighborRadius = 5f;
    public float avoidanceRadius = 2f;

    [Header("References")]
    public CustomTerrain terrain;

    private enum State { Wander, Chase, Flee, Eat, Flock }
    [SerializeField] private State currentState;
    private Transform target;
    private Vector3 wanderTarget;
    private float wanderTimer;

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentState = State.Wander;
        
        if (identity == null) identity = GetComponent<EntityIdentity>();
        if (terrain == null) terrain = FindObjectOfType<CustomTerrain>();
        
        if (identity.type == EntityType.Fish)
        {
            currentState = State.Flock;
        }
        else
        {
            PickWanderTarget();
        }
    }

    void Update()
    {
        // Energy Loss
        // Moving costs more energy. Running costs even more.
        float speedFactor = 1.0f;
        if (currentState == State.Chase || currentState == State.Flee) speedFactor = 2.0f; // Running
        else if (currentState == State.Wander) speedFactor = 1.2f; // Walking
        else if (currentState == State.Flock) speedFactor = 1.5f; // Swimming constant
        else speedFactor = 0.5f; // Idle/Eating

        currentEnergy -= energyLossRate * speedFactor * Time.deltaTime;
        
        if (currentEnergy <= 0)
        {
            currentHealth -= 5f * Time.deltaTime; // Starving
        }
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Sense Environment
        SenseEnvironment();

        // State Machine
        switch (currentState)
        {
            case State.Wander: Wander(); break;
            case State.Chase: Chase(); break;
            case State.Flee: Flee(); break;
            case State.Eat: Eat(); break;
            case State.Flock: Flock(); break;
        }
        
        // Keep on terrain
        StickToTerrain();
    }

    void SenseEnvironment()
    {
        // Fish just flock mostly, unless fleeing
        if (identity.type == EntityType.Fish)
        {
             // Check for predators to flee from
             Collider[] hitsFish = Physics.OverlapSphere(transform.position, visionRadius);
             foreach(var hit in hitsFish)
             {
                 if (hit.transform == transform) continue;
                 EntityIdentity other = hit.GetComponent<EntityIdentity>();
                 if (other != null && (other.type == EntityType.Predator || other.type == EntityType.Human))
                 {
                     target = hit.transform;
                     currentState = State.Flee;
                     return;
                 }
             }
             // Otherwise flock
             if (currentState != State.Flee) currentState = State.Flock;
             return;
        }

        // Priority: Flee Predator > Chase Food (if hungry) > Wander
        // Predators: Chase Prey > Wander
        
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRadius);
        Transform bestPrey = null;
        Transform bestPredator = null;
        float closestPreyDist = float.MaxValue;
        float closestPredatorDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            
            EntityIdentity other = hit.GetComponent<EntityIdentity>();
            if (other == null) continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) > fieldOfViewAngle / 2) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);

            // Logic based on who I am
            if (identity.type == EntityType.Human)
            {
                if (other.type == EntityType.Predator)
                {
                    if (dist < closestPredatorDist) { closestPredatorDist = dist; bestPredator = hit.transform; }
                }
                else if (other.type == EntityType.Vegetable || other.type == EntityType.Prey || other.type == EntityType.Fish)
                {
                    if (dist < closestPreyDist) { closestPreyDist = dist; bestPrey = hit.transform; }
                }
            }
            else if (identity.type == EntityType.Predator)
            {
                if (other.type == EntityType.Human || other.type == EntityType.Prey || other.type == EntityType.Fish)
                {
                    if (dist < closestPreyDist) { closestPreyDist = dist; bestPrey = hit.transform; }
                }
            }
            else if (identity.type == EntityType.Prey)
            {
                if (other.type == EntityType.Predator || other.type == EntityType.Human)
                {
                    if (dist < closestPredatorDist) { closestPredatorDist = dist; bestPredator = hit.transform; }
                }
                else if (other.type == EntityType.Vegetable)
                {
                     if (dist < closestPreyDist) { closestPreyDist = dist; bestPrey = hit.transform; }
                }
            }
        }

        // Decision Making
        if (bestPredator != null)
        {
            target = bestPredator;
            currentState = State.Flee;
        }
        else if (bestPrey != null && currentEnergy < maxEnergy * 0.9f) // Only eat if not full
        {
            target = bestPrey;
            currentState = State.Chase;
        }
        else if (currentState != State.Wander)
        {
            // Energy Minimization: If low energy and safe, maybe just idle?
            // For now, we wander but maybe at slower speed (handled in Update)
            currentState = State.Wander;
            PickWanderTarget();
        }
    }

    void Wander()
    {
        if (Vector3.Distance(transform.position, wanderTarget) < 1f || wanderTimer <= 0)
        {
            PickWanderTarget();
        }

        MoveTowards(wanderTarget, moveSpeed);
        wanderTimer -= Time.deltaTime;
    }

    void Chase()
    {
        if (target == null) { currentState = State.Wander; return; }
        
        MoveTowards(target.position, runSpeed);

        if (Vector3.Distance(transform.position, target.position) < 1.5f)
        {
            currentState = State.Eat;
        }
    }

    void Flee()
    {
        if (target == null) { currentState = State.Wander; return; }

        Vector3 dirAway = (transform.position - target.position).normalized;
        Vector3 fleePos = transform.position + dirAway * 5f;
        MoveTowards(fleePos, runSpeed);
    }

    void Eat()
    {
        if (target == null) { currentState = State.Wander; return; }

        // Simple eating logic: destroy target, gain energy
        // In a real game, we might deal damage or reduce food amount
        
        EntityIdentity other = target.GetComponent<EntityIdentity>();
        if (other != null)
        {
            currentEnergy += 30f;
            currentHealth += 10f;
            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            // If it's a vegetable, destroy it. If it's prey, maybe kill it?
            // For simplicity, we just destroy the object if it's vegetable or prey
            // If it's a human eating prey, the prey dies.
            
            if (other.type == EntityType.Vegetable || other.type == EntityType.Prey || other.type == EntityType.Fish)
            {
                Destroy(target.gameObject);
            }
            else if (identity.type == EntityType.Predator && (other.type == EntityType.Human || other.type == EntityType.Prey || other.type == EntityType.Fish))
            {
                // Predator eating human/prey
                SurvivorAgent preyAgent = target.GetComponent<SurvivorAgent>();
                if (preyAgent != null)
                {
                    preyAgent.TakeDamage(20f); // Damage instead of instant kill
                }
                else
                {
                    Destroy(target.gameObject);
                }
            }
        }
        
        target = null;
        currentState = State.Wander;
    }

    void MoveTowards(Vector3 dest, float speed)
    {
        Vector3 direction = (dest - transform.position).normalized;
        direction.y = 0; // Keep movement horizontal
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void PickWanderTarget()
    {
        // Pick a random point on the terrain
        if (terrain != null)
        {
            Vector3 size = terrain.terrainSize();
            float x = Random.Range(0, size.x);
            float z = Random.Range(0, size.z);
            wanderTarget = new Vector3(x, 0, z);
        }
        else
        {
            // Fallback if no terrain
            Vector2 randomCircle = Random.insideUnitCircle * 20f;
            wanderTarget = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }
        wanderTimer = 5f;
    }

    void StickToTerrain()
    {
        if (terrain != null)
        {
            Vector3 terrainSize = terrain.terrainSize();
            Vector3 currentPos = transform.position;

            // 1. Constrain X and Z to be within valid terrain bounds
            // We use 0 as min and terrainSize as max.
            float clampedX = Mathf.Clamp(currentPos.x, 0.5f, terrainSize.x - 0.5f);
            float clampedZ = Mathf.Clamp(currentPos.z, 0.5f, terrainSize.z - 0.5f);

            // 2. Determine the correct Height (Y)
            float finalY = currentPos.y;

            // If it's a Land animal (Human, Predator, Prey), snap strictly to the ground
            if (identity.type != EntityType.Fish)
            {
                finalY = terrain.getInterp(clampedX, clampedZ);
            }
            else 
            {
                // If it's a Fish, ensure it doesn't swim *under* the ground
                float terrainHeight = terrain.getInterp(clampedX, clampedZ);
                if (finalY < terrainHeight + 0.5f) 
                {
                    finalY = terrainHeight + 0.5f; // Push fish up if they hit the sand
                }
            }

            // 3. Apply the constrained position
            transform.position = new Vector3(clampedX, finalY, clampedZ);
        }
    }

    void Flock()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int neighborCount = 0;

        Collider[] hits = Physics.OverlapSphere(transform.position, neighborRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            
            SurvivorAgent otherAgent = hit.GetComponent<SurvivorAgent>();
            if (otherAgent != null && otherAgent.identity.type == EntityType.Fish)
            {
                // Separation
                Vector3 diff = transform.position - hit.transform.position;
                if (diff.magnitude < avoidanceRadius)
                {
                    separation += diff.normalized / diff.magnitude;
                }

                // Alignment
                alignment += hit.transform.forward;

                // Cohesion
                cohesion += hit.transform.position;

                neighborCount++;
            }
        }

        Vector3 moveDir = transform.forward;

        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            cohesion /= neighborCount;
            cohesion = (cohesion - transform.position).normalized;

            Vector3 flockDir = (separation * separationWeight) + (alignment * alignmentWeight) + (cohesion * cohesionWeight);
            moveDir = Vector3.Lerp(transform.forward, flockDir.normalized, Time.deltaTime * 2f);
        }
        else
        {
            // If alone, wander a bit
            if (Random.value < 0.01f)
            {
                moveDir = (Quaternion.Euler(0, Random.Range(-45, 45), 0) * transform.forward).normalized;
            }
        }

        // Avoid obstacles (simple)
        if (Physics.Raycast(transform.position, transform.forward, 2f))
        {
            moveDir = Vector3.Reflect(transform.forward, Vector3.right); // Simple bounce
        }

        // Apply movement
        Quaternion lookRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // Simple death
        Destroy(gameObject);
    }
}
