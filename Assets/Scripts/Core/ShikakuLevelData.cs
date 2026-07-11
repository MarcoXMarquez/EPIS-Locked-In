using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevoNivelShikaku", menuName = "EPIS/Minijuegos/Shikaku Level")]
public class ShikakuLevelData : ScriptableObject
{
    [Header("Ajustes del Tablero")]
    public int width = 5;  // Columnas
    public int height = 5; // Filas

    [Header("Números Iniciales")]
    public List<NumberEntry> puzzleNumbers;

    [System.Serializable]
    public struct NumberEntry
    {
        public int x;      // Posición horizontal (0 a width-1)
        public int y;      // Posición vertical (0 a height-1)
        public int value;  // El número que verá el jugador (ej: 4, 6, 8)
    }
}