using System.Collections.Generic;
using UnityEngine;

public class SpriteObjectsManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject objectPrefab;

    [Header("BrushSettings")]
    [SerializeField] private BrushSettings brushSettings;

    private GameObject selectedObject;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    public void HandleDragStart(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null && spawnedObjects.Contains(hit.collider.gameObject))
        {
            selectedObject = hit.collider.gameObject;
        }
    }

    public void HandleDrag(Vector2 newPosition)
    {
        if (selectedObject != null)
        {
            selectedObject.transform.position = newPosition;
        }
    }

    public void HandleDragEnd()
    {
        selectedObject = null;
    }

    public void HandleSwipe(Vector2 position)
    {
        if (brushSettings.SwipeTrail != null)
        {
            brushSettings.SwipeTrail.gameObject.SetActive(true);
            brushSettings.SwipeTrail.Clear();
            brushSettings.UpdateTrailColor();
        }
        ClearAllObjects();
    }


    public void CreateObject(Vector2 position)
    {
        if (objectPrefab != null && brushSettings.CurrentSprite != null && brushSettings.CurrentColor.a > 0)
        {
            GameObject newObject = Instantiate(objectPrefab, position, Quaternion.identity);
            SpriteRenderer spriteRenderer = newObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = brushSettings.CurrentSprite;
            spriteRenderer.color = brushSettings.CurrentColor;
            spawnedObjects.Add(newObject);
        }
    }

    public void DestroyObjectAtPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);

        for(int i = 0; i < spawnedObjects.Count; ++i)
        {
            if(hit.collider != null && spawnedObjects[i] == hit.collider.gameObject)
            {
                spawnedObjects.RemoveAt(i);
                Destroy(hit.collider.gameObject);
                break;
            }
        }
    }

    public void ClearAllObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }
        spawnedObjects.Clear();
    }
}