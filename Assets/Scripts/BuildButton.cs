using UnityEngine;
using UnityEngine.UI;
using System;

public class BuildButton : MonoBehaviour
{
    [Header("Type de construction")]
    public string turretType; // Nom/type de la tour à construire, à remplir dans l'inspecteur

    void OnMouseDown()
    {
        // Appelle la méthode correspondante dans BuildManager selon le type
        switch (turretType.ToLower())
        {
            case "electric":
                BuildManager.instance.BuildElectric();
                break;

            case "soldiers":
                BuildManager.instance.BuildSoldiers();
                break;

            default:
                Debug.LogWarning("Type de tour inconnu : " + turretType);
                break;
        }
    }
}
