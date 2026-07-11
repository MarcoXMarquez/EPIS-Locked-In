using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Cerebro del minijuego Shikaku. 
/// Maneja la generación, validación de reglas, dibujo de bloques y borrado.
/// </summary>
public class ShikakuManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public ShikakuLevelData currentLevel; 
    public GameObject cellPrefab;
    public GameObject blockPrefab;
    public RectTransform boardRoot;    
    public Transform gridContainer;   
    public Transform blockContainer;  
    public RectTransform selectionGhost; 
    public CanvasGroup modalCanvasGroup;
    
    private ShikakuGenerator generator;

    [Header("Configuración Visual")]
    public Color[] blockColors; 
    public bool isPuzzleActive = false;

    [Header("Estado de Victoria")]
    public UnityEngine.Events.UnityEvent onLevelComplete;
    private int totalCellsToFill;
    private int currentFilledArea = 0;

    [Header("Lógica Interna")]
    private List<ShikakuCell> activeCells = new List<ShikakuCell>();
    private ShikakuCell[,] cellMatrix;
    private Vector2[,] cellPositions; 
    private bool[,] cellOccupied; // Matriz de colisión para evitar superposiciones
    private ShikakuCell startCell, endCell;
    private bool isDragging = false;

    void Start() { 
        ClearGrid(); 
        generator = GetComponent<ShikakuGenerator>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) TogglePuzzle(!isPuzzleActive);
    }

    // --- CONTROL DE PANTALLA ---

    public void TogglePuzzle(bool state)
    {
        isPuzzleActive = state;
        modalCanvasGroup.alpha = state ? 1 : 0;
        modalCanvasGroup.interactable = state;
        modalCanvasGroup.blocksRaycasts = state;

        if (state) 
        { 
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true; 
            
            // PASO CLAVE: Si tenemos el generador, creamos un nivel aleatorio
            if (generator != null) {
                generator.CreateRandomLevel();
            } else {
                GenerateGrid(); // Fallback por si no hay generador
            }
        }
        else 
        { 
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false; 
            ClearGrid(); 
        }
    }

    // --- GENERACIÓN DEL TABLERO ---

    [ContextMenu("Generate Level")]
    public void GenerateGrid()
    {
        ClearGrid();
        if (currentLevel == null) return;

        int w = currentLevel.width;
        int h = currentLevel.height;

        cellMatrix = new ShikakuCell[w, h];
        cellPositions = new Vector2[w, h];
        cellOccupied = new bool[w, h]; // Reiniciar mapa de colisiones
        
        gridContainer.GetComponent<GridLayoutGroup>().constraintCount = w;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                GameObject newCellObj = Instantiate(cellPrefab, gridContainer);
                ShikakuCell cellScript = newCellObj.GetComponent<ShikakuCell>();
                cellScript.x = x; cellScript.y = y;
                cellMatrix[x, y] = cellScript;

                var entry = currentLevel.puzzleNumbers.Find(n => n.x == x && n.y == y);
                cellScript.SetValue(entry.value);
                activeCells.Add(cellScript);
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainer.GetComponent<RectTransform>());

        // Guardar posiciones centrales para cálculos rápidos del Ghost
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                cellPositions[x, y] = cellMatrix[x, y].GetComponent<RectTransform>().anchoredPosition;

        totalCellsToFill = w * h;
        currentFilledArea = 0;
    }

    // --- MANEJO DE INTERACCIÓN (INPUT) ---

    public void OnCellPointerDown(ShikakuCell cell) 
    {
        // SI LA CELDA ESTÁ OCUPADA: Borramos el bloque existente
        if (cellOccupied[cell.x, cell.y])
        {
            DeleteBlock(cell.blockOccupyingMe);
            return;
        }

        // SI ESTÁ LIBRE: Iniciamos el arrastre
        isDragging = true;
        startCell = cell;
        endCell = cell;
        UpdateVisualSelection();
    }

    public void OnCellPointerEnter(ShikakuCell cell) 
    { 
        if (!isDragging) return; 
        endCell = cell; 
        UpdateVisualSelection(); 
    }

    public void OnCellPointerUp(ShikakuCell cell) 
    { 
        if (!isDragging) return; 
        isDragging = false; 
        ValidateSelection(); 
        selectionGhost.gameObject.SetActive(false); 
    }

    // --- LÓGICA DE DIBUJO Y VALIDACIÓN ---

    void UpdateVisualSelection()
    {
        if (startCell == null || endCell == null) return;
        selectionGhost.gameObject.SetActive(true);

        // Usamos los centros guardados en la tabla para posicionar el Ghost
        Vector2 p1 = cellPositions[startCell.x, startCell.y];
        Vector2 p2 = cellPositions[endCell.x, endCell.y];

        float cellW = cellPrefab.GetComponent<RectTransform>().rect.width;
        float cellH = cellPrefab.GetComponent<RectTransform>().rect.height;

        selectionGhost.sizeDelta = new Vector2(Mathf.Abs(p1.x - p2.x) + cellW, Mathf.Abs(p1.y - p2.y) + cellH);
        selectionGhost.anchoredPosition = (p1 + p2) / 2;
    }

    void ValidateSelection()
    {
        int minX = Mathf.Min(startCell.x, endCell.x); int maxX = Mathf.Max(startCell.x, endCell.x);
        int minY = Mathf.Min(startCell.y, endCell.y); int maxY = Mathf.Max(startCell.y, endCell.y);

        int area = (maxX - minX + 1) * (maxY - minY + 1);

        // 1. REGLA DE COLISIÓN: Verificar si el área toca bloques ya existentes
        for (int i = minX; i <= maxX; i++) {
            for (int j = minY; j <= maxY; j++) {
                if (cellOccupied[i, j]) {
                    Debug.Log("<color=orange>IA:</color> Error: Superposición de datos.");
                    return; 
                }
            }
        }

        // 2. REGLA DE SHIKAKU: Debe haber 1 solo número y su valor debe ser igual al área
        List<int> foundNums = new List<int>();
        for (int i = minX; i <= maxX; i++)
            for (int j = minY; j <= maxY; j++)
                if (cellMatrix[i, j].value > 0) foundNums.Add(cellMatrix[i, j].value);

        if (foundNums.Count == 1 && foundNums[0] == area) 
        {
            SpawnPermanentBlock(minX, maxX, minY, maxY, area);
        }
    }

    void SpawnPermanentBlock(int minX, int maxX, int minY, int maxY, int area)
    {
        GameObject newBlock = Instantiate(blockPrefab, blockContainer);
        RectTransform rt = newBlock.GetComponent<RectTransform>();

        // Copiamos la geometría exacta que el jugador vio en el Ghost
        rt.anchoredPosition = selectionGhost.anchoredPosition;
        rt.sizeDelta = selectionGhost.sizeDelta;

        if(blockColors.Length > 0)
            newBlock.GetComponent<Image>().color = blockColors[Random.Range(0, blockColors.Length)];

        // Marcamos las celdas como ocupadas y vinculamos el bloque para poder borrarlo luego
        for (int i = minX; i <= maxX; i++) {
            for (int j = minY; j <= maxY; j++) {
                cellOccupied[i, j] = true;
                cellMatrix[i, j].blockOccupyingMe = newBlock;
            }
        }

        currentFilledArea += area;
        CheckVictory();
    }

    // --- BORRADO Y LIMPIEZA ---

    void DeleteBlock(GameObject blockToDelete)
    {
        if (blockToDelete == null) return;
        int areaLiberada = 0;

        for (int x = 0; x < currentLevel.width; x++) {
            for (int y = 0; y < currentLevel.height; y++) {
                if (cellMatrix[x, y].blockOccupyingMe == blockToDelete) {
                    cellOccupied[x, y] = false;
                    cellMatrix[x, y].blockOccupyingMe = null;
                    areaLiberada++;
                }
            }
        }
        currentFilledArea -= areaLiberada;
        Destroy(blockToDelete);
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        isDragging = false;
        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = gridContainer.GetChild(i).gameObject;
                if (child.name == "Block_Container" || child.name == "Selection_Ghost") continue;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
        }

        if (blockContainer != null)
        {
            for (int i = blockContainer.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(blockContainer.GetChild(i).gameObject); 
                else DestroyImmediate(blockContainer.GetChild(i).gameObject);
            }
        }

        activeCells.Clear();
        currentFilledArea = 0;
        if (selectionGhost != null) selectionGhost.gameObject.SetActive(false);
    }

    // --- CONDICIÓN DE VICTORIA ---

    void CheckVictory() 
    {
        if (currentFilledArea >= totalCellsToFill) {
            Debug.Log("<color=gold>IA:</color> Grid depurado al 100%. Puzzle resuelto.");
            onLevelComplete.Invoke();
            Invoke("AutoClose", 1.5f);
        }
    }

    void AutoClose() => TogglePuzzle(false);
}