using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class BlockSelectionUI : MonoBehaviour
{
    public GameObject[] blockPrefabs;
    public Button[] blockButtons;
    private int selectedBlockIndex = 0;

    void Start()
    {
        UpdateButtonColors();
    }

    public void CycleBlock()
    {
        selectedBlockIndex = (selectedBlockIndex + 1) % blockPrefabs.Length;
        UpdateButtonColors();
    }

    public GameObject GetSelectedBlock()
    {
        return blockPrefabs[selectedBlockIndex];
    }

    public void SetSelectedBlock(GameObject block)
    {
        int index = System.Array.IndexOf(blockPrefabs, block);
        if (index != -1)
        {
            selectedBlockIndex = index;
            UpdateButtonColors();
        }
    }

    void UpdateButtonColors()
{
    for (int i = 0; i < blockButtons.Length; i++)
    {
        Outline outline = blockButtons[i].GetComponent<Outline>();
        if (outline == null)
        {
            outline = blockButtons[i].gameObject.AddComponent<Outline>();
        }

        outline.enabled = (i == selectedBlockIndex); 
        outline.effectColor = Color.black;  
        outline.effectDistance = new Vector2(30, 30); 
    }
}


}