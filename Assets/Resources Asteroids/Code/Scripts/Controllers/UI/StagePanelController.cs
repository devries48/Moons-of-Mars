using Game.Astroids;
using UnityEngine;
using UnityEngine.UI;

public class StagePanelController : MonoBehaviour
{

    [SerializeField] Button continueButton;

    AsteroidsGameManager GameManager
    {
        get
        {
            if (__gameManager == null)
                __gameManager = AsteroidsGameManager.Instance;

            if (__gameManager == null)
                Debug.LogWarning("GameManager is null");

            return __gameManager;
        }
    }
    AsteroidsGameManager __gameManager;


    void OnEnable()
    {
        //DisableButton(continueButton);
    }

    void DisableButton(Button button)
    {
        continueButton.interactable = false;

        //var colorBlock = button.colors;
        //var newColor = button.colors.normalColor;
        //newColor.a = 50;
        //colorBlock.disabledColor = newColor;
        //button.colors = colorBlock;
    }
    public void EnableButton()
    {
        print("eNABLEbUTTON");
        continueButton.interactable = true;
    }

    void OnDisable()
    {
        continueButton.onClick.RemoveListener(ContinueButtonClick);
    }

    void ContinueButtonClick()
    {
        GameManager.m_LevelManager.HideStageResuls();
    }

}
