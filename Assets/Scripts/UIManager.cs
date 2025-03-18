using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Color color;
    public Sprite sprite;
    public GameObject prefab;

    public float doubleTapThreshold = 0.5f;

    private float lastTapTime = 0;
    private bool isWaitingForDoubleTap = false;
    public void SetColor(Image image)
    {
        this.color = image.color;
    }
    public void SetSprite(Image image)
    {
        this.sprite = image.sprite;
    }
    private void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (Time.time - lastTapTime <= doubleTapThreshold)
                {

                    lastTapTime = 0;
                    isWaitingForDoubleTap = false;
                    StopAllCoroutines();

                    Vector2 worldPos = GetRealPosition(touch.position);
                    RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

                    if (hit.collider != null)
                    {
                        Destroy(hit.collider.gameObject);
                    }
                }
                else
                {
                    lastTapTime = Time.time;
                    StartCoroutine(SingleTapCoroutine(touch.position));
                }
            }
        }
    }

    IEnumerator SingleTapCoroutine(Vector2 touchPos)
    {
        isWaitingForDoubleTap = true;
        yield return new WaitForSeconds(doubleTapThreshold);

        if (isWaitingForDoubleTap == true)
        {
            if (color != null && sprite != null && prefab != null)
            {
                GameObject rider = Instantiate(prefab, GetRealPosition(touchPos), prefab.transform.rotation);
                rider.SetActive(false);
                SpriteRenderer spriteRenderer = rider.GetComponent<SpriteRenderer>();
                spriteRenderer.color = color;
                spriteRenderer.sprite = sprite;
                rider.SetActive(true);
            }
            isWaitingForDoubleTap = false;
        }
    }

    public Vector2 GetRealPosition(Vector2 position)
    {
        return Camera.main.ScreenToWorldPoint(position);
    }
}
