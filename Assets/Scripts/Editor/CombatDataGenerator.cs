using UnityEditor;
using UnityEngine;
using AntiBullyingGame.Core;
using System.IO;

public static class CombatDataGenerator
{
    [MenuItem("POO Game/Generar Datos de Combate")]
    public static void Generate()
    {
        string folderPath = "Assets/Resources/CombatData";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string assetPath = folderPath + "/Mateo el Bully.asset";
        
        // Comprobar si ya existe
        EnemyCombatData data = AssetDatabase.LoadAssetAtPath<EnemyCombatData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<EnemyCombatData>();
            data.enemyName = "Mateo el Bully";
            data.resolvedSaveId = "bully_mateo_reformed";
            data.maxHP = 32f;
            data.attackValue = 5f;
            data.defendValue = 0f;
            data.spareMessage = "* Mateo baja los brazos y decide cambiar.";
            data.flavorTexts = new string[] {
                "* Intuitivamente, sientes la tension en el aire.",
                "* Mateo parece dudar por un segundo.",
                "* Tratas de mantener la calma."
            };

            // Opciones de Hablar
            data.hablarOpciones = new DialogueOption[] {
                new DialogueOption {
                    label = "Hablar con calma",
                    playerLine = "* Le dices que sus palabras duelen y que puede parar ahora.",
                    mercyValue = 22,
                    startsBattle = false,
                    enemyResponse = "* Mateo baja la voz por primera vez."
                },
                new DialogueOption {
                    label = "Preguntar que le pasa",
                    playerLine = "* Le preguntas si esta enojado por algo y le ofreces hablar.",
                    mercyValue = 28,
                    startsBattle = false,
                    enemyResponse = "* Mateo mira al suelo. Parece que eso le llego."
                },
                new DialogueOption {
                    label = "Defender a la victima",
                    playerLine = "* Te pones firme: nadie merece ser humillado.",
                    mercyValue = 12,
                    startsBattle = true,
                    enemyResponse = "* Mateo se molesta y vuelve a atacarte."
                },
                new DialogueOption {
                    label = "Pedir la mochila",
                    playerLine = "* Le pides que devuelva la mochila y termine esto sin mas dano.",
                    mercyValue = 35,
                    startsBattle = false,
                    enemyResponse = "* Mateo aprieta la mochila, pero empieza a dudar."
                },
                new DialogueOption {
                    label = "Avisar a un adulto",
                    playerLine = "* Le dices que si sigue, buscaras ayuda de un adulto.",
                    mercyValue = 8,
                    startsBattle = true,
                    enemyResponse = "* Mateo se pone a la defensiva y lanza otra provocacion."
                }
            };

            data.attackWords = new string[] { "Burla", "Rumor", "Insulto", "Empujon", "Amenaza", "Risa" };
            data.minigameDuration = 6.5f;
            data.pelletSpeed = 2.65f;
            data.spawnInterval = 0.4f;

            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("¡Creado Mateo el Bully.asset con éxito en " + assetPath + "!");
            EditorUtility.DisplayDialog("✅ Datos de Combate Generados", "He creado el asset 'Mateo el Bully' con sus datos por defecto en Assets/Resources/CombatData/.", "¡Perfecto!");
        }
        else
        {
            Debug.Log("El asset Mateo el Bully ya existe en " + assetPath);
            EditorUtility.DisplayDialog("ℹ️ El Asset ya existe", "El archivo de configuración de Mateo ya existe en Assets/Resources/CombatData/.", "Entendido");
        }
    }
}
