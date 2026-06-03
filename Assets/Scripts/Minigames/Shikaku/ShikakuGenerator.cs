using UnityEngine;
using System.Collections.Generic;

public class ShikakuGenerator : MonoBehaviour
{
    [Header("Configuración de Generación")]
    [Range(5, 8)] public int minSize = 5;
    [Range(5, 8)] public int maxSize = 8;

    [ContextMenu("Generar Nivel Cuadrado")]
    public void CreateRandomLevel()
    {
        // 1. Determinar un único valor N para que sea NxN
        int n = Random.Range(minSize, maxSize + 1);
        int width = n;
        int height = n;

        // 2. El resto de la lógica se mantiene igual, pero garantizando el cuadrado
        bool[,] occupied = new bool[width, height];
        List<ShikakuLevelData.NumberEntry> entries = new List<ShikakuLevelData.NumberEntry>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (occupied[x, y]) continue;

                // Intentar expandir un rectángulo (ahora dentro de un contenedor cuadrado)
                Vector2Int rectSize = TryGrowRectangle(x, y, width, height, occupied);
                
                for (int i = x; i < x + rectSize.x; i++)
                    for (int j = y; j < y + rectSize.y; j++)
                        occupied[i, j] = true;

                int numX = Random.Range(x, x + rectSize.x);
                int numY = Random.Range(y, y + rectSize.y);

                entries.Add(new ShikakuLevelData.NumberEntry {
                    x = numX,
                    y = numY,
                    value = rectSize.x * rectSize.y
                });
            }
        }

        ApplyToManager(width, height, entries);
    }
    Vector2Int TryGrowRectangle(int startX, int startY, int w, int h, bool[,] occupied)
    {
        // Máximo tamaño posible para el bloque (puedes ajustar esto)
        int maxW = Random.Range(1, 4); 
        int maxH = Random.Range(1, 4);

        int finalW = 0;
        int finalH = 0;

        // Intentar expandir en ancho
        for (int i = 0; i < maxW; i++)
        {
            if (startX + i < w && !occupied[startX + i, startY]) finalW++;
            else break;
        }

        // Intentar expandir en alto para ese ancho
        for (int j = 0; j < maxH; j++)
        {
            bool rowFree = true;
            for (int i = 0; i < finalW; i++)
            {
                if (startY + j >= h || occupied[startX + i, startY + j])
                {
                    rowFree = false;
                    break;
                }
            }
            if (rowFree) finalH++;
            else break;
        }

        return new Vector2Int(finalW, finalH);
    }

    void ApplyToManager(int w, int h, List<ShikakuLevelData.NumberEntry> entries)
    {
        ShikakuManager manager = GetComponent<ShikakuManager>();
        if (manager != null)
        {
            // 1. Creamos la data nueva en memoria
            ShikakuLevelData newData = ScriptableObject.CreateInstance<ShikakuLevelData>();
            newData.width = w;
            newData.height = h;
            newData.puzzleNumbers = entries;

            // 2. Se la pasamos al manager
            manager.currentLevel = newData;

            // 3. El manager se encarga de limpiar lo viejo y dibujar lo nuevo
            manager.GenerateGrid();
            Debug.Log($"<color=green>IA:</color> Nuevo puzzle generado: {w}x{h}");
        }
    }
}