using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //이거 다른데에서도 코드 파일에서도 쓸게용
    public static GameManager Instance;
    //
    public Text timeText;
    public Text recordText;
    public Text myrecordText;
    public GameObject gameOverText;

    public int enemyCount;

    private float surviveTime;
    private bool isgameOver;
    public bool isgameClear;

    public List<GameObject> levelPrefabs = new List<GameObject>();
    //레벨 프리팹을 할당해서 생성할곳
    private GameObject currentLevelObj;
    public int currentLevelIndex = 0;
    private bool isLoadingLevel = true;

    // Start is called before the first frame update
    void Start()
    {
        surviveTime = 0;
        isgameOver = false;
        isgameClear = false;
        
        levelPrefabs = new List<GameObject>();
        GameObject[] loadedLevels = Resources.LoadAll<GameObject>("Levels");

        // 이름 기준으로 정렬 후 리스트에 추가
        Array.Sort(loadedLevels, (a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));
        foreach (var level in loadedLevels)
        {
            levelPrefabs.Add(level);
        }

        LevelLoad(currentLevelIndex);
    }

    //왜 계속 시작하자 마자 끝나는거니...
    // Update is called once per frame
    void Update()
    {
        //0을 제외한 모든 숫자는 참이므로 !100 은 0이외의 수이니
        //참이다, 그러므로 앞에 not연산자가 붙었으니 false가 되기 위하여
        //0으로 변환된다
        if (!isgameOver)
        {
            //매 프레임마다 점수 추가(살아남은 초)
            surviveTime += Time.deltaTime;
            //서바이브 타임에 5배를 해서 점수로 변환(초의 5배)
            timeText.text = "Score : " + (int)surviveTime * 5;
            //파이선에서는 print("Time : ",surviveTime) 형식으로 작성

            if (!isLoadingLevel && enemyCount < 1 && !isgameClear)
            {
                isgameClear = true;
                currentLevelIndex++;
                if (currentLevelIndex < levelPrefabs.Count)
                    LevelLoad(currentLevelIndex);
                else
                    EndGame();
            }
        }

        else
        {
            //게임이 종료됐을때 h을 누르면 홈씬으로 돌아감
            if (Input.GetKeyDown(KeyCode.H))
            {
                SceneManager.LoadScene("HomeScene");
            }

        }

    }

    public void LevelLoad(int currentLevelIndex)
    {
        if (currentLevelIndex >= levelPrefabs.Count)
        {
            Debug.Log("끌리얼, 현재 인덱스 : " + currentLevelIndex);
            return;
        }

        isLoadingLevel = true;

        if (currentLevelObj != null)
        {
            Destroy(currentLevelObj);
        }

        currentLevelObj = Instantiate(levelPrefabs[currentLevelIndex]);
        isgameClear = false; // 새로운 레벨 시작 시 클리어 플래그 리셋
        isLoadingLevel = false;
        CountEnemy();
        Instance = this;
    }

    public void CountEnemy()
    {
        //GameObject를 담을 리스트를 만들거야 
        //그 리스트의 이름은 enemies이고 
        //Enemy 태그를 가진 애들을 리스트에 저장할거야
        //이 코드는 호출시마다 적의 수 업데이트를 함
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
    }

    //게임 끝남 상태 변경, 게임 끝남 텍스트 띄우고 베스트 타임 경신 역할 함
    public void EndGame()
    {
        isgameOver = true;
        gameOverText.SetActive(true);
        float bestTime = PlayerPrefs.GetFloat("BestTime");
        if (surviveTime > bestTime)
        {
            bestTime = surviveTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
        }


        recordText.text = "Best Score : " + (int)bestTime * 5;
        myrecordText.text = "My Score : " + (int)surviveTime * 5;
    }

    //적의 수를 세는 함수
    

    
}
