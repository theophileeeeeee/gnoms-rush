using UnityEngine;

public class SoldiersTurret : MonoBehaviour
{
    [Header("Prefab du soldat")]
    public KnightManager soldierPrefab;

    [Header("Positions des soldats (offsets locaux)")]
    public Vector3[] offsetsHaut   = new Vector3[3];
    public Vector3[] offsetsBas    = new Vector3[3];
    public Vector3[] offsetsDroite = new Vector3[3];
    public Vector3[] offsetsGauche = new Vector3[3];

    public KnightManager[] soldiers;

    public void Init(RoadPosition roadPosition)
    {
        Vector3[] offsets = roadPosition switch
        {
            RoadPosition.Haut   => offsetsHaut,
            RoadPosition.Bas    => offsetsBas,
            RoadPosition.Droite => offsetsDroite,
            RoadPosition.Gauche => offsetsGauche,
            _                   => offsetsHaut
        };

        soldiers = new KnightManager[offsets.Length];

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 worldPos = transform.position + offsets[i];

            KnightManager soldier = Instantiate(
                soldierPrefab,
                worldPos,
                Quaternion.identity,
                transform
            );

            GameObject home = new GameObject($"HomePoint_{i}");
            home.transform.position = worldPos;
            home.transform.SetParent(transform);

            soldier.SetHomePoint(home.transform);
            soldiers[i] = soldier;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawDirectionGizmos(offsetsHaut,   Color.green,  "H");
        DrawDirectionGizmos(offsetsBas,    Color.red,    "B");
        DrawDirectionGizmos(offsetsDroite, Color.blue,   "D");
        DrawDirectionGizmos(offsetsGauche, Color.yellow, "G");
    }

    private void DrawDirectionGizmos(Vector3[] offsets, Color color, string label)
    {
        if (offsets == null) return;

        Gizmos.color = color;

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 worldPos = transform.position + offsets[i];
            Gizmos.DrawSphere(worldPos, 0.15f);
            Gizmos.DrawLine(transform.position, worldPos);
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(worldPos + Vector3.up * 0.3f, $"{label}{i + 1}");
        }
    }
void Update()
{
    // On ne vérifie pas dans l'Update si on est en Editor via #if 
    // sauf si c'est vraiment ton intention, mais généralement 
    // la logique de jeu doit tourner partout.
    
    for (int i = 0; i < soldiers.Length; i++)
    {
        // Si le slot est vide (soldat détruit ou jamais mis)
        if (soldiers[i] == null)
        {
            Debug.Log($"Le soldat à l'index {i} est mort. Respawn en cours...");
            RespawnSoldier(i);
        }
    }
}

private void RespawnSoldier(int index)
{
    Transform homePoint = transform.Find($"HomePoint_{index}");

    if (homePoint != null)
    {
        KnightManager newSoldier = Instantiate(
            soldierPrefab,
            homePoint.position,
            Quaternion.identity,
            transform
        );
        newSoldier.SetHomePoint(homePoint);
        soldiers[index] = newSoldier;
    }
}
#endif
}