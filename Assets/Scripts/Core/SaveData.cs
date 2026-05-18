using System;
using System.Collections.Generic;
namespace AntiBullyingGame.Core
{
    [Serializable]
    public class SaveData
    {
        // La posición del personaje guardada como un array de 3 floats (x, y, z)
        // porque Vector3 a veces presenta problemas con JsonUtility en versiones antiguas.
        public float[] position;
        public float health;
        public int morale;
        public List<InventorySaveData> inventory = new List<InventorySaveData>();
        public List<string> interactedNPCs = new List<string>();
    }
}
