using UnityEngine;

public class Interaction : MonoBehaviour
{
    public BlockSelectionUI blockSelectionUI; 

    void Update()
    {
        bool leftClickPressed = Input.GetButtonDown("Fire1"); 
        bool rightClickPressed = Input.GetButtonDown("Fire2"); 
        bool gPressed = Input.GetKeyDown(KeyCode.G);          

        if (leftClickPressed || rightClickPressed || gPressed)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (leftClickPressed)
                {
                    Destroy(hit.transform.gameObject); 
                }
                else if (rightClickPressed)
                {
                    Vector3 blockSpawnPoint = hit.transform.position + hit.normal;
                    GameObject selectedBlock = blockSelectionUI.GetSelectedBlock();
                    Instantiate(selectedBlock, blockSpawnPoint, Quaternion.identity); 
                }
                else if (gPressed)
                {
                    blockSelectionUI.SetSelectedBlock(hit.transform.gameObject); 
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            blockSelectionUI.CycleBlock(); 
        }
    }
}