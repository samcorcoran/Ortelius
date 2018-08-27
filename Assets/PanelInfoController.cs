using Everett.Ebstorf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelInfoController : MonoBehaviour {

    private Cell displayedCell = null;
    private bool displayedCellChanged = false;

    // UI Elements
    public Text cellIdText;

	// Use this for initialization
	void Start () {
        // Find and register UI elements to write to
        foreach (var textComponent in GetComponentsInChildren<Text>())
        {
            if (textComponent.name == "CellIdValue")
            {
                cellIdText = textComponent;
            }
        }
    }

    // Update is called once per frame
    void Update () {
		if (displayedCellChanged)
        {
            if (displayedCell == null)
            {
                ClearCellInfo();
            }
            else
            {
                DisplayCellInfo();
            }
            // Reset flag
            displayedCellChanged = false;
        }
	}

    public void SetDisplayedCell(Cell cell)
    {
        displayedCell = cell;
        displayedCellChanged = true;
    }

    public void ClearDisplayedCell()
    {
        displayedCell = null;
        displayedCellChanged = true;
    }

    private void ClearCellInfo()
    {
        // Reset all fields to blank
    }

    private void DisplayCellInfo()
    {
        // Write displayedCell info to fields
        cellIdText.text = displayedCell.Id;
    }
}
