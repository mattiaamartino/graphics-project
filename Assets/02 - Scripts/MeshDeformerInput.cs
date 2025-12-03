using UnityEngine;

public class MeshDeformerInput : MonoBehaviour
{
    [SerializeField] private float force = 10f;
    [SerializeField] private float forceOffset = 0.1f;
    private Camera cam;

    private string[] deformers;
    [SerializeField] private string currentDeformer; // liste

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("SecondCamera").GetComponent<Camera>();

        // your deformers here
        int numberDeformers = 2;
        deformers = new string[numberDeformers];
        deformers[0] = "Push";
        deformers[1] = "Test";
        currentDeformer = deformers[0];

    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // only deform while holding left mouse button
        {
            HandleInput();
        }

    }

    void HandleInput()
    {
        Ray inputRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();

            if (deformer)
            {
                deformer.SetDeformer(currentDeformer);
                Vector3 point = hit.point;
                point += hit.normal * forceOffset;
                deformer.AddDeformingForce(point, force);
            }
        }

    }
}
