using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMessageController : MonoBehaviour
{
    public static ShowMessageController Instance { get; private set; }

    public bool nowShow;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        nowShow = false;
    }

    void Update()
    {
        
    }

    public IEnumerator ShowMessage(GameObject messageObj, float sec)
    {
        if(nowShow)
        {
            yield break;
        }
        else
        {
            nowShow = true;
            messageObj.SetActive(true);
            yield return new WaitForSeconds(sec);
            messageObj.SetActive(false);
            nowShow = false;
        }
    }
}
