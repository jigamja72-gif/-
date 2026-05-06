using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FootballGame : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject ballPrefab;
    public TextMeshProUGUI scoreText;

    private GameObject ball;
    private Rigidbody ballRb;

    private readonly List<GameObject> players = new();

    private int scoreA;
    private int scoreB;

    private void Start()
    {
        CreateField();
        SpawnBall();
        SpawnTeams();
        CreateUI();
        UpdateScore();
    }

    private void Update()
    {
        foreach (GameObject p in players)
        {
            PlayerAI ai = p.GetComponent<PlayerAI>();
            ai.Run(ball.transform, ballRb);
        }
    }

    private void CreateField()
    {
        GameObject field = GameObject.CreatePrimitive(PrimitiveType.Plane);
        field.transform.localScale = new Vector3(5, 1, 3);

        GameObject goalA = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goalA.transform.position = new Vector3(0, 1, -25);
        goalA.transform.localScale = new Vector3(10, 2, 1);
        goalA.AddComponent<BoxCollider>().isTrigger = true;
        goalA.AddComponent<Goal>().Init(0, this);

        GameObject goalB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goalB.transform.position = new Vector3(0, 1, 25);
        goalB.transform.localScale = new Vector3(10, 2, 1);
        goalB.AddComponent<BoxCollider>().isTrigger = true;
        goalB.AddComponent<Goal>().Init(1, this);
    }

    private void SpawnBall()
    {
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.position = Vector3.zero;
        ball.tag = "Ball";

        ballRb = ball.AddComponent<Rigidbody>();
    }

    private void SpawnTeams()
    {
        for (int i = 0; i < 11; i++)
        {
            CreatePlayer(new Vector3(Random.Range(-20, 20), 1, Random.Range(-10, 0)), 0, i == 0);
            CreatePlayer(new Vector3(Random.Range(-20, 20), 1, Random.Range(0, 10)), 1, i == 0);
        }
    }

    private void CreatePlayer(Vector3 pos, int team, bool isGK)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        p.transform.position = pos;

        Renderer r = p.GetComponent<Renderer>();
        r.material.color = team == 0 ? Color.blue : Color.red;

        PlayerAI ai = p.AddComponent<PlayerAI>();
        ai.team = team;
        ai.isGoalkeeper = isGK;

        players.Add(p);
    }

    private void CreateUI()
    {
        GameObject canvas = new("Canvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        GameObject textObj = new("Score");
        textObj.transform.SetParent(canvas.transform);

        scoreText = textObj.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 40;

        RectTransform rect = scoreText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(150, -50);
    }

    public void GoalScored(int team)
    {
        if (team == 0)
        {
            scoreB++;
        }
        else
        {
            scoreA++;
        }

        ball.transform.position = Vector3.zero;
        ballRb.linearVelocity = Vector3.zero;

        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = "TEAM A " + scoreA + " : " + scoreB + " TEAM B";
    }
}

public class PlayerAI : MonoBehaviour
{
    public int team;
    public bool isGoalkeeper;

    public void Run(Transform ball, Rigidbody ballRb)
    {
        if (isGoalkeeper)
        {
            Vector3 target = new(ball.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 3);
        }
        else
        {
            Vector3 dir = (ball.position - transform.position).normalized;
            transform.position += dir * (5f * Time.deltaTime);
        }

        float dist = Vector3.Distance(transform.position, ball.position);

        if (dist < 2f)
        {
            Vector3 shootDir = (team == 0 ? Vector3.forward : Vector3.back)
                               + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);

            ballRb.linearVelocity = shootDir * 12f;
        }
    }
}

public class Goal : MonoBehaviour
{
    private int team;
    private FootballGame gm;

    public void Init(int t, FootballGame g)
    {
        team = t;
        gm = g;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            gm.GoalScored(team);
        }
    }
}
