using UnityEngine;

public class ArcherController : MonoBehaviour
{
    [Header("Détection")]
    public float rayonDetection = 5f;
    public string enemyTag = "Ennemy";
    public Animator animator;

    [Header("Tir")]
    public GameObject prefabFleche;
    public Transform pointDeTir;
    public float vitesseFleche = 5f;
    public float fireRate = 1f;

    private Transform target;
    private float fireCountdown;

    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        GameObject[] ennemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in ennemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= rayonDetection)
            target = nearestEnemy.transform;
        else
            target = null;
    }

    void Update()
    {
        if (target != null)
            FlipVersEnnemi(target);

        if (fireCountdown <= 0f)
        {
            if (target != null)
            {
                Shoot(target);
                fireCountdown = 1f / fireRate;
            }
        }

        fireCountdown -= Time.deltaTime;
    }

    void FlipVersEnnemi(Transform cible)
    {
        // Si l'ennemi est à gauche, on flip, sinon on remet à l'endroit
        if (cible.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void Shoot(Transform cible)
    {
        if (prefabFleche == null) { Debug.Log("prefabFleche est null !"); return; }
        if (pointDeTir == null) { Debug.Log("pointDeTir est null !"); return; }

        animator.SetTrigger("Shoot");

        GameObject fleche = Instantiate(prefabFleche, pointDeTir.position, Quaternion.identity);
        FlecheMovement fm = fleche.GetComponent<FlecheMovement>();
        if (fm == null) fm = fleche.AddComponent<FlecheMovement>();

        fm.Init(cible, vitesseFleche);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, rayonDetection);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rayonDetection);
    }
}