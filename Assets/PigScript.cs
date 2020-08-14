using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PigScript : MonoBehaviour
{
    [Header("Точки для расчёта наклона")]
    //Точки для расчёта смещений
    [Tooltip("Верхняя точка для просчёта X и Y одной клетки, а также смещения по Z-координате при ходьбе вверх")]
    [SerializeField] private Transform upPoint;
    [Tooltip("Нижняя точка для просчёта X и Y одной клетки, а также смещения по Z-координате при ходьбе вверх")]
    [SerializeField] private Transform downPoint;
    
    private Vector2 cell_size;
    //Максимальная скорость ходьбы 
    [Header("Параметры свинки")]
    [SerializeField] private float Speed;

    //Смещение по Х при ходьбе вверх (Угол наклона)
    float X_offset;
    //Смещение по Z при ходьбе вверх
    float Z_offset;

    [Header("Компоненты управления")]
    [Tooltip("компонент джойстика")]
    [SerializeField] private FixedJoystick Joystick;

    //Просто для сокращения, чтобы не писать gameobject.transform.position
    private Vector3 Pos
    {
        get
        {
            return gameObject.transform.position;
        }
        set
        {
            gameObject.transform.position = value;
        }
    }

    [Header("Спрайты")]
    [Tooltip("Атлас с графическими материалами")]
    [SerializeField] private SpriteAtlas Sprites;
    [Tooltip("Имена спрайтов свинки: 1й - вправо, 2й - вверх, 3й - влево, 4й - вниз")]
    [SerializeField] private string[] SpriteNames;

    private SpriteRenderer Renderer;

    [Header("Бомбы")]
    [SerializeField] private GameObject Bomb_Prefab;

    private List<GameObject> Stones = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        //размер одной клетки
        cell_size = new Vector2(upPoint.position.x - downPoint.position.x,  (upPoint.position.y - downPoint.position.y) / 9);
        //смещение по Z при движении вверх или вниз. Нужно для того чтобы "заходить за камни"
        Z_offset = (upPoint.position.z - downPoint.position.z) / (upPoint.position.y - downPoint.position.y);
        //смещение по X при движении вверх или вниз.
        X_offset = (upPoint.position.x - downPoint.position.x) / (upPoint.position.y - downPoint.position.y);
        Renderer = GetComponent<SpriteRenderer>();
        Renderer.sprite = Sprites.GetSprite(SpriteNames[0]);
    }

    // Update is called once per frame
    void Update()
    {
        int touchcount = 0;
        if (Joystick.Horizontal != 0 & Joystick.Vertical != 0)
        {
            if (Mathf.Abs(Joystick.Horizontal) > Mathf.Abs(Joystick.Vertical))
            {
                Pos = new Vector3(Pos.x + Speed * Joystick.Horizontal * Time.deltaTime, Pos.y, Pos.z);
                Renderer.sprite = Sprites.GetSprite(SpriteNames[1 - (int)Mathf.Sign(Joystick.Horizontal)]);
            }
            else
            {
                Pos = new Vector3(Pos.x + (X_offset * Speed * Time.deltaTime * Mathf.Sign(Joystick.Vertical)), Pos.y + Speed * Joystick.Vertical * Time.deltaTime, Z_offset * (Pos.y - downPoint.position.y));
                Renderer.sprite = Sprites.GetSprite(SpriteNames[2 - (int)Mathf.Sign(Joystick.Vertical)]);
            }
            //Если уже зажат джойстик, то для установки бомбы будет проверяться второе нажатие
            touchcount = 1;
        }
        if (Input.touchCount > touchcount)
        {
            Touch touch = Input.GetTouch(touchcount);
            if (touch.phase == TouchPhase.Began && Camera.main.WorldToScreenPoint(touch.position).x > Camera.main.pixelWidth / 2)
            {
                Vector2 BombPos = GetBombSpawnPos(Pos);
                if (Physics2D.Raycast(BombPos, Vector2.up, 0, LayerMask.GetMask("Bomb")).collider == null)
                {
                    GameObject Bomb = Instantiate(Bomb_Prefab, BombPos, Quaternion.identity);
                    Bomb.GetComponent<SpriteRenderer>().sprite = Sprites.GetSprite("bomb");
                }
            }
        }

    }

    Vector3 GetBombSpawnPos(Vector2 PigPos)
    {
        //Получаем вектор расстояния между свинкой и близлежащим камнем
        Vector2 Direction = PigPos - (Vector2)Stones[0].transform.position;

        Debug.Log(Direction.x);
        Debug.Log(Direction.x - X_offset * Direction.y);

        //Округляем, получая -1, 0 или 1 для X и Y, тем самым зная в какой клетке относительно камня расположена свинка
        Direction = new Vector2(Mathf.RoundToInt(Direction.x - X_offset * Direction.y),Mathf.RoundToInt(Direction.y));

        Debug.Log(Direction);

        //Получаем центр клетки, используя размеры клетки (cell_size) и наши относительные координаты (Direction)
        Vector3 Cell_Center = Stones[0].transform.position + new Vector3(cell_size.x * Direction.x + cell_size.x * X_offset * Direction.y, cell_size.y * Direction.y, 0);

        Debug.DrawLine(Stones[0].transform.position, Cell_Center);

        return Cell_Center;
    }

    //Можно было сделать иначе:
    //просчитать относительные координаты восьми позиций относительно координат свинки
    //при нажатии на правую часть экрана создавать Raycast'ы в эти 8 координат и соответственно получать рядом стоящие камни
    //дальше по такому же принципу
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Stones.Add(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Stones.Remove(collision.gameObject);
    }
}
