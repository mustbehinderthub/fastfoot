using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Enemies { Dragon = 0, Bean = 1 }

    #region Singleton

    public static GameManager instance;

    public static GameManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    #endregion

    public GameObject playerPrefab;
    public GameObject player;
    public CharacterController characterController;
    public Transform playerSpawn;

    public List<Transform> enemySpawn;

    public List<GameObject> enemyList = new List<GameObject>();
    public List<GameObject> activeEnemies = new List<GameObject>();

    // Level zusammen mit Environment ??
    public GameObject levelPrefab;
    public GameObject level;

    public GameObject camPrefab;
    Camera cam;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;


    // Start is called before the first frame update
    void Start()
    {
        // Initialisiere Damage Text
        DamageTextController.Initialize();

        player = Instantiate(playerPrefab, playerSpawn.position, Quaternion.Euler(0f, 90f, 0f));
        characterController = player.GetComponent<CharacterController>();
        level = Instantiate(levelPrefab, Vector3.zero, Quaternion.Euler(0f, 90f, 0f));

        GameObject camera = Instantiate(camPrefab);
        cam = camera.GetComponentInChildren<Camera>();
        virtualCamera = camera.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        virtualCamera.Follow = player.transform;

        // Sende Daten an UI Manager, da anders herum nicht geht
        UIManager.instance.charCon = player.GetComponent<CharacterController>();

    }

    void SpawnEnemies(Enemies enemy, Transform pos)
    {
        Instantiate(enemyList[(int)enemy], pos, false);
    }


    public RaycastHit MouseToWorldPos()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, 1000f, ~LayerMask.GetMask("Player")))
            return hitInfo;
        else return default(RaycastHit);
        //  Debug.Log("MouseToWorldPos hit " + hitInfo.transform.name + " at point " + hitInfo.point);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            SpawnEnemies(Enemies.Dragon, enemySpawn[0]);
    }

}
