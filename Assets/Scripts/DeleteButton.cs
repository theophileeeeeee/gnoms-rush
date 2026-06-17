using UnityEngine;

public class DeleteButton : MonoBehaviour
{
    public void ExecuteDelete()
    {
        Node node = BuildManager.instance.GetSelectedNode();

        if (node == null || node.turret == null)
        {
            Debug.Log("Aucune tour à supprimer");
            return;
        }

        int refund = node.ClearNode();

        BuildManager.instance.uiManager.EarnMoney(refund);
        BuildManager.instance.PlayDeleteSound();
        BuildManager.instance.DeselectNode();
    }
}