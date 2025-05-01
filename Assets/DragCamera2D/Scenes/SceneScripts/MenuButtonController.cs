using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    public Button rtsBTN;
    public Button followBTN;
    public Button dollyBTN;
    public Button snapBTN;
    public Button menuBTN;

    // Start is called before the first frame update
    void Start()
    {
        if (menuBTN != null) menuBTN.onClick.AddListener(() => changeScene(0));
        if (rtsBTN != null) rtsBTN.onClick.AddListener(() => changeScene(1));
        if (followBTN != null) followBTN.onClick.AddListener(() => changeScene(2));
        if (dollyBTN != null) dollyBTN.onClick.AddListener(() => changeScene(3));
        if (snapBTN != null) snapBTN.onClick.AddListener(() => changeScene(4));
    }

    void changeScene(int sceneid) {
        SceneManager.LoadScene(sceneBuildIndex: sceneid);
    }
}
