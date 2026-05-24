using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

            GameObject home = new GameObject($"HomePoint_{i}");
            home.transform.position = worldPos;
            home.transform.SetParent(transform);

            SpawnSoldierAt(i, home.transform);
        }
    }

private void SpawnSoldierAt(int index, Transform homePoint)
{
    KnightManager soldier = Instantiate(
        soldierPrefab,
        transform.position,
        Quaternion.identity,
        transform
    );

    soldiers[index] = soldier;
    StartCoroutine(AssignHomeNextFrame(soldier, homePoint));
}

private IEnumerator AssignHomeNextFrame(KnightManager soldier, Transform homePoint)
{
    yield return null;
    if (soldier != null)
        soldier.SetHomePoint(homePoint);
}

    void Update()
    {
        for (int i = 0; i < soldiers.Length; i++)
        {
            if (soldiers[i] == null)
            {
                Transform homePoint = transform.Find($"HomePoint_{i}");
                if (homePoint != null)
                    SpawnSoldierAt(i, homePoint);
            }
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
#endif
}