using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectListControl : MonoBehaviour
{
    private List<PlayerItem> playerItems;
    private Texture2D[] imageTextures;

    [SerializeField]
    private GameObject buttonTemplate;

    [SerializeField]
    private GridLayoutGroup gridLayoutGroup;

    private void Awake()
    {
        if (PropertiesModel.TypeVisualization == "DrawAgain")
        {
            imageTextures = Resources.LoadAll<Texture2D>("imageDrawing/original");
        }
    }

    private void Start()
    {
        playerItems = new List<PlayerItem>();

        for (int i = 0; i < imageTextures.Length; i++)
        {
            PlayerItem playerItem = new PlayerItem();
            playerItem.imageTexture = imageTextures[i];

            playerItems.Add(playerItem);
        }

        GenerationListButton();
    }

    void GenerationListButton()
    {
        foreach (PlayerItem item in playerItems)
        {
            GameObject newButtom = Instantiate(buttonTemplate);
            newButtom.SetActive(true);

            newButtom.GetComponent<ObjectListButton>().SetImage(item.imageTexture);
            newButtom.transform.SetParent(buttonTemplate.transform.parent, false);
        }
    }

    public class PlayerItem
    {
        public Texture2D imageTexture;
    }
}
