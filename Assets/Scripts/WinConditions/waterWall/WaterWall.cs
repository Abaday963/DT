using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWall : MonoBehaviour
{
    [Header("Настройки стены")]
    public int wallHeight = 10;
    public float tileSize = 1f;
    public float geyserSpeed = 5f;
    public float animationSpeed = 0.5f;

    [Header("Спрайты")]
    public Sprite[] waterSprites = new Sprite[2];
    public Sprite[] topWaterSprites = new Sprite[2];

    [Header("Взаимодействие")]
    public LayerMask targetLayer;
    public float evaporationTime = 3f;

    [Header("Эффекты")]
    public GameObject evaporationEffectPrefab;
    public AudioClip evaporationSound;
    public AudioClip waterAmbientSound;

    private AudioSource waterAudioSource;
    private AudioSource evaporationAudioSource;

    private List<WaterTile> waterTiles = new List<WaterTile>();
    private bool isBuilding = false;

    void Start()
    {
        // Настраиваем AudioSource для фонового звука воды
        waterAudioSource = gameObject.AddComponent<AudioSource>();
        waterAudioSource.clip = waterAmbientSound;
        waterAudioSource.loop = true;
        waterAudioSource.volume = 0.5f;
        waterAudioSource.playOnAwake = false;

        // Настраиваем AudioSource для звуков испарения
        evaporationAudioSource = gameObject.AddComponent<AudioSource>();
        evaporationAudioSource.playOnAwake = false;
        evaporationAudioSource.volume = 0.7f;

        wallHeight = Random.Range(8, 10);
        StartCoroutine(BuildWaterWall());
    }

    IEnumerator BuildWaterWall()
    {
        isBuilding = true;

        for (int i = 0; i < wallHeight; i++)
        {
            GameObject waterTile = new GameObject($"WaterTile_{i}");
            waterTile.transform.parent = transform;

            // Начальная позиция — каждая плитка на 0.5 пикселя ближе (0.005 юнита)
            // Плюс сдвинута вниз на 2 пикселя (0.02 юнита)
            Vector3 targetPosition = transform.position + Vector3.up * (i * (tileSize - 0.005f));
            Vector3 startPosition = targetPosition + Vector3.down * 0.02f;
            waterTile.transform.position = startPosition;

            SpriteRenderer sr = waterTile.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            BoxCollider2D collider = waterTile.AddComponent<BoxCollider2D>();
            collider.isTrigger = false; // Теперь НЕ триггер
            collider.size = new Vector2(2.25f, 2f);

            // Добавляем Rigidbody2D для физических столкновений
            Rigidbody2D rb = waterTile.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Кинематический, чтобы не падал от гравитации
            rb.gravityScale = 0f;

            WaterTileCollision collision = waterTile.AddComponent<WaterTileCollision>();
            collision.waterWall = this;
            collision.tileIndex = i;

            bool isTopTile = (i == wallHeight - 1);
            WaterTile tile = new WaterTile(waterTile, i, isTopTile);

            // Назначаем начальный спрайт, отличный от предыдущего
            if (!isTopTile)
            {
                int newIndex = 0;
                if (i > 0)
                {
                    int prevIndex = waterTiles[i - 1].spriteIndex;
                    if (waterSprites.Length > 1)
                        newIndex = (prevIndex + 1) % waterSprites.Length;
                }
                tile.spriteIndex = newIndex;
            }
            else
            {
                tile.spriteIndex = i % topWaterSprites.Length;
            }

            tile.spriteRenderer.sprite = tile.isTop ? topWaterSprites[tile.spriteIndex] : waterSprites[tile.spriteIndex];
            waterTiles.Add(tile);

            yield return new WaitForSeconds(geyserSpeed / wallHeight);
        }

        isBuilding = false;

        // Запускаем фоновый звук воды
        if (waterAudioSource != null && waterAmbientSound != null)
        {
            waterAudioSource.Play();
        }

        StartCoroutine(AnimateWater());
    }

    IEnumerator AnimateWater()
    {
        while (true)
        {
            yield return new WaitForSeconds(animationSpeed);

            for (int i = 0; i < waterTiles.Count; i++)
            {
                WaterTile tile = waterTiles[i];
                if (tile.isEvaporated || tile.spriteRenderer == null) continue;

                Sprite[] spritesToUse = tile.isTop ? topWaterSprites : waterSprites;
                if (spritesToUse.Length < 2) continue;

                // Анимация: переключаем спрайт на отличный от предыдущего
                int newIndex = (tile.spriteIndex + 1) % spritesToUse.Length;

                // Если не верхушка, проверим, не совпадает ли с предыдущим
                if (!tile.isTop && i > 0)
                {
                    int prevSprite = waterTiles[i - 1].spriteIndex;
                    if (newIndex == prevSprite)
                    {
                        newIndex = (newIndex + 1) % spritesToUse.Length;
                    }
                }

                tile.spriteIndex = newIndex;
                tile.spriteRenderer.sprite = spritesToUse[tile.spriteIndex];
            }
        }
    }

    public void EvaporateTiles(int hitTileIndex)
    {
        List<int> tilesToEvaporate = new List<int>();
        tilesToEvaporate.Add(hitTileIndex);

        for (int offset = -2; offset <= 2; offset++)
        {
            if (offset == 0) continue;
            int neighborIndex = hitTileIndex + offset;
            if (neighborIndex >= 0 && neighborIndex < waterTiles.Count)
            {
                tilesToEvaporate.Add(neighborIndex);
            }
        }

        // Воспроизводим звук испарения
        if (evaporationAudioSource != null && evaporationSound != null)
        {
            evaporationAudioSource.PlayOneShot(evaporationSound);
        }

        foreach (int index in tilesToEvaporate)
        {
            StartCoroutine(EvaporateTile(waterTiles[index]));
        }
    }

    IEnumerator EvaporateTile(WaterTile tile)
    {
        if (tile.isEvaporated) yield break;

        tile.isEvaporated = true;

        // Создаем эффект испарения в центре плитки
        if (evaporationEffectPrefab != null)
        {
            Vector3 effectPosition = tile.tileObject.transform.position;
            GameObject effect = Instantiate(evaporationEffectPrefab, effectPosition, Quaternion.identity);

            // Автоматически уничтожаем эффект через несколько секунд
            Destroy(effect, 3f);
        }

        tile.spriteRenderer.enabled = false;
        tile.tileCollider.enabled = false;

        yield return new WaitForSeconds(evaporationTime);

        tile.isEvaporated = false;
        tile.spriteRenderer.enabled = true;
        tile.tileCollider.enabled = true;
        tile.spriteRenderer.sprite = tile.isTop ? topWaterSprites[tile.spriteIndex] : waterSprites[tile.spriteIndex];
    }

    void OnDestroy()
    {
        // Останавливаем звуки при уничтожении объекта
        if (waterAudioSource != null && waterAudioSource.isPlaying)
        {
            waterAudioSource.Stop();
        }
    }

    void OnValidate()
    {
        wallHeight = Mathf.Clamp(wallHeight, 10, 10);
    }

    public class WaterTileCollision : MonoBehaviour
    {
        [HideInInspector]
        public WaterWall waterWall;

        [HideInInspector]
        public int tileIndex;

        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision detected!");

            // Проверяем, принадлежит ли объект нужному 2D-слою
            if (((1 << collision.gameObject.layer) & waterWall.targetLayer) != 0)
            {
                waterWall.EvaporateTiles(tileIndex);
                Debug.Log("Water tile evaporated!");
            }
        }
    }

    public class WaterTile
    {
        public GameObject tileObject;
        public SpriteRenderer spriteRenderer;
        public Collider2D tileCollider;
        public bool isTop;
        public bool isEvaporated;
        public int tileIndex;
        public Vector3 originalPosition;
        public int spriteIndex;

        public WaterTile(GameObject obj, int index, bool top)
        {
            tileObject = obj;
            spriteRenderer = obj.GetComponent<SpriteRenderer>();
            tileCollider = obj.GetComponent<Collider2D>();
            isTop = top;
            isEvaporated = false;
            tileIndex = index;
            originalPosition = obj.transform.position;
            spriteIndex = -1;
        }
    }
}