using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class PlayerShipBuild : MonoBehaviour, IUnityAdsListener, IUnityAdsInitializationListener
{
    [SerializeField] GameObject[] shopButtons;
    GameObject target;
    GameObject tmpSelection;
    GameObject textBoxPanel;

    [SerializeField] GameObject[] visualWeapons;
    [SerializeField] SOActorModel defaultPlayerShip;
    GameObject playerShip;
    GameObject buyButton;
    GameObject bankObj;
    GameObject shipItem;
    int bank = 600;
    bool purchaseMade = false;

    [SerializeField] string androidGameId;
    [SerializeField] string IOSGameId;
    [SerializeField] bool testMode = true;
    string adId = null;

    void Awake()
    {
        CheckPlatform();
    }


    void Start()
    {
        TurnOffSelectionHighlights();
        textBoxPanel = GameObject.Find("textBoxPanel");

        purchaseMade = false;
        bankObj = GameObject.Find("bank");
        bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
        buyButton = textBoxPanel.transform.Find("BUY ?").gameObject;
        TurnOffPlayerShipVisuals();
        PreparePlayerShipForUpgrade();
        StartCoroutine(WaitForAd());
    }

    void CheckPlatform()
    {
        string gameId = null;
        #if UNITY_IOS
        {
            gameId = IOSGameId;
            adId = "Rewarded_IOS";
        }
        #elif UNITY_ANDROID
        {
            gameId = androidGameId;
            adId = "Rewarded_Android";
        }
        #endif
        Advertisement.Initialize(gameId, testMode, false, (UnityEngine.Advertisements.IUnityAdsInitializationListener)this);
    }

    void TurnOffSelectionHighlights()
    {
        for (int i = 0; i < shopButtons.Length; i++)
        {
            shopButtons[i].SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        AttemptSelection();
    }

    GameObject ReturnClickedObject(out RaycastHit hit)
    {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 100, out hit))
        {
            target = hit.collider.gameObject;
        }

        return target;
    }

    void AttemptSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            target = ReturnClickedObject(out hitInfo);
            if (target != null)
            {
                if (target.transform.Find("itemText"))
                {
                    TurnOffSelectionHighlights();
                    Select();
                    UpdateDescriptionBox();

                    //Not already sold
                    if (target.transform.Find("itemText").GetComponent<TextMesh>().text != "SOLD")
                    {
                        //can afford
                        Affordable();

                        //can not afford
                        LackOfCredits();
                    }
                    else if (target.transform.Find("item Text").GetComponent<TextMesh>().text == "SOLD")
                    {
                        SoldOut();
                    }
                }
                else if (target.name == "BUY ?")
                {
                    BuyItem();

                }
                else if (target.name == "START")
                {
                    StartGame();
                }
                else if(target.name == "WATCH AD")
                {
                    WatchAdvert();
                }
            }
        }
    }

    void Select()
    {
        tmpSelection = target.transform.Find("SelectionQuad").gameObject;
        tmpSelection.SetActive(true);
    }

    void UpdateDescriptionBox()
    {
        textBoxPanel.transform.Find("name").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponentInParent<ShopPiece>().ShopSelection.iconName;
        textBoxPanel.transform.Find("desc").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponentInParent<ShopPiece>().ShopSelection.description;
    }

    void Affordable()
    {
        if (bank >= System.Int32.Parse(target.transform.GetComponent<ShopPiece>().ShopSelection.cost))
        {
            Debug.Log("CAN BUY");
            buyButton.SetActive(true);
        }
    }

    void LackOfCredits()
    {
        if (bank < System.Int32.Parse(target.transform.Find("itemText").GetComponent<TextMesh>().text))
        {
            Debug.Log("CAN BUY");
        }
    }

    void SoldOut()
    {
        Debug.Log("SOLD OUT");
    }

    void TurnOffPlayerShipVisuals()
    {
        for (int i = 0; i < visualWeapons.Length; i++)
        {
            visualWeapons[i].gameObject.SetActive(false);
        }
    }

    void PreparePlayerShipForUpgrade()
    {
        playerShip = GameObject.Instantiate(defaultPlayerShip.actor);

        playerShip.GetComponent<Player>().enabled = false;
        playerShip.transform.position = new Vector3(0, 10000, 0);
        playerShip.GetComponent<IActorTemplate>().ActorStats(defaultPlayerShip);
    }

    void BuyItem()
    {
        Debug.Log("PURCHASED");
        purchaseMade = true;
        buyButton.SetActive(false);
        tmpSelection.SetActive(false);

        for (int i = 0; i < visualWeapons.Length; i++)
        {
            if (visualWeapons[i].name == tmpSelection.transform.parent.gameObject.GetComponent<ShopPiece>().ShopSelection.iconName)
            {
                visualWeapons[i].SetActive(true);
            }
        }

        Debug.Log(tmpSelection.transform.parent.gameObject.GetComponent<ShopPiece>().ShopSelection.iconName);
        UpgradeToShip(tmpSelection.transform.parent.gameObject.GetComponent<ShopPiece>().ShopSelection.iconName);
        bank = bank - System.Int32.Parse(tmpSelection.transform.parent.GetComponent<ShopPiece>().ShopSelection.cost);
        bankObj.transform.Find("bankText").GetComponent<TextMesh>().text = bank.ToString();
        tmpSelection.transform.parent.transform.Find("itemText").GetComponent<TextMesh>().text = "SOLD";
    }

    void UpgradeToShip(string upgrade)
    {
        shipItem = GameObject.Instantiate(Resources.Load(upgrade)) as GameObject;
        shipItem.transform.SetParent(playerShip.transform);
        shipItem.transform.localPosition = Vector3.zero;
    }

    void StartGame()
    {
        if (purchaseMade)
        {
            playerShip.name = "UpgradeShip";
            if (playerShip.transform.Find("energy +1(Clone)"))
            {
                playerShip.GetComponent<Player>().Health = 2;
            }
            DontDestroyOnLoad(playerShip);
        }
        //UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        GameManager.Instance.GetComponent<ScenesManager>().BeginGame(GameManager.gameLevelScene);
    }

    IEnumerator WaitForAd()
    {
        while(!Advertisement.isInitialized)
        {
            yield return null;
        }
        LoadAd();
    }

    void LoadAd()
    {
        Advertisement.AddListener((UnityEngine.Advertisements.IUnityAdsListener)this);
        Advertisement.Load(adId);
    }

    void WatchAdvert()
    {
        Advertisement.Show(adId);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete");
    }
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
       Debug.Log($"Unity Ads Initialization Failed : {error.ToString()}- {message}");
    }
    public void OnUnityAdsReady(string placementId)
    {

    }
    public void OnUnityAdsDidError(string message)
    {

    }
    public void OnUnityAdsDidStart(string placementId)
    {

    }
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if(showResult == ShowResult.Finished)
        {
            //REWARD PLAYER
            Debug.Log("Unity Ads Rewarded Ad Completed");
            bank += 300;
            bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
        }
        else if(showResult == ShowResult.Skipped)
        {
            //DO NOT REWARD PLAYER
        }
        else if(showResult == ShowResult.Failed)
        {
            Debug.LogWarning("The ad did not finish due to an error.");
        }
        Advertisement.Load(placementId);
        TurnOffSelectionHighlights();
    }
}
