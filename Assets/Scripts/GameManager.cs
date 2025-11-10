using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    public Text timeText;
    public Text recordText;
    public Text myrecordText;
    public GameObject gameOverText;

    private float surviveTime;
    private bool isgameOver;

    // Start is called before the first frame update
    void Start()
    {
        surviveTime = 0;
        isgameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        //0을 제외한 모든 숫자는 참이므로 !100 은 0이외의 수이니
        //참이다, 그러므로 앞에 not연산자가 붙었으니 false가 되기 위하여
        //0으로 변환된다
        if(!isgameOver)
        {
           surviveTime += Time.deltaTime;
           timeText.text = "Score : " + (int)surviveTime*5;
           //파이선에서는 print("Time : ",surviveTime) 형식으로 작성
        }

        else
        {
            if(Input.GetKeyDown(KeyCode.H))
            {
                SceneManager.LoadScene("HomeScene");
            }

        }
    }

    public void EndGame()
    {
        isgameOver = true;
        gameOverText.SetActive(true);
        float bestTime = PlayerPrefs.GetFloat("BestTime");
        if(surviveTime > bestTime)
        {
            bestTime = surviveTime;
            PlayerPrefs.SetFloat("BestTime",bestTime);
        }


        recordText.text = "Best Score : " + (int)bestTime*5;
        myrecordText.text = "My Score : " + (int)surviveTime * 5;
    }
}
