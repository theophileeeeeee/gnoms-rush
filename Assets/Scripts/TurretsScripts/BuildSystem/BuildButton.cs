using UnityEngine;
using UnityEngine.UI;

public class BuildButtonManager : MonoBehaviour
{
    private float lastClickTime;
    private const float clickCooldown = 0.3f;

    public void SelectTurretType(string turretType)
    {
        Debug.Log("[BuildButtonManager] Méthode appelée avec le type : " + turretType);

        if (Time.time - lastClickTime < clickCooldown)
        {
            Debug.LogWarning("[BuildButtonManager] Clic ignoré : Anti-spam actif (cooldown).");
            return;
        }
        lastClickTime = Time.time;

        if (BuildManager.instance == null)
        {
            Debug.LogError("[BuildButtonManager] ERREUR : L'instance de BuildManager est introuvable dans la scène !");
            return;
        }

        Node activeNode = BuildManager.instance.GetSelectedNode();
        if (activeNode == null)
        {
            Debug.LogError("[BuildButtonManager] ERREUR : Impossible de construire, 'selectedNode' est déjà NULL au moment du clic !");
            return;
        }

        Debug.Log("[BuildButtonManager] Node cible validée pour la construction : " + activeNode.name);

        switch (turretType.ToLower())
        {
            case "electric":
                Debug.Log("[BuildButtonManager] Envoi de l'ordre d'achat -> Electric");
                BuildManager.instance.BuildElectric();
                break;

            case "soldiers":
                Debug.Log("[BuildButtonManager] Envoi de l'ordre d'achat -> Soldiers");
                BuildManager.instance.BuildSoldiers();
                break;

            case "archer":
                Debug.Log("[BuildButtonManager] Envoi de l'ordre d'achat -> Archer");
                BuildManager.instance.BuildArcher();
                break;

            case "bomb":
                Debug.Log("[BuildButtonManager] Envoi de l'ordre d'achat -> Bomb");
                BuildManager.instance.BuildBomb();
                break;

            default:
                Debug.LogError("[BuildButtonManager] ERREUR : Le nom de la tour '" + turretType + "' ne correspond à aucun cas du switch !");
                break;
        }
    }
}