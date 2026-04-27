using System;

namespace AntiBullyingGame.Core
{
    [Serializable]
    public class SaveData
    {
        // La posición del personaje guardada como un array de 3 floats (x, y, z)
        // porque Vector3 a veces presenta problemas con JsonUtility en versiones antiguas.
        public float[] position = new float[3];
        
        public float health;
        
        public int morale;
    }
}
