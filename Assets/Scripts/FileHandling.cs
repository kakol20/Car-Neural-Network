using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class JSONManager
{
    public static void SaveData<T>(T[] data, string path)
    {
        // convert data to json string
        Wrapper<T> wrapper = new Wrapper<T>
        {
            Items = data
        };
        string json = JsonUtility.ToJson(wrapper, true);

        File.WriteAllText(path, json);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public static class XMLManager
{
    public static void LoadData<T>(out T data, string path) where T : class
    {
        FileStream file = new FileStream(path, FileMode.Open);
        XmlSerializer xmls = new XmlSerializer(typeof(T));

        data = xmls.Deserialize(file) as T;

        file.Close();
    }

    public static void SaveData<T>(T data, string path) where T : class
    {
        XmlSerializer xmls = new XmlSerializer(typeof(T));

        FileStream file = new FileStream(path, FileMode.Create);

        xmls.Serialize(file, data);

        file.Close();
    }
}

public class CarSettings
{
    public float brakeForce;
    public float drag;
    public float maxVelocity;
    public float steeringAngle;
    public float throttleForce;
}

public class NetworkData
{
    public float[][] biases;
    public float[][][] weights;
}

public class Settings
{
    public bool loadNetworkAtStart;
    public bool saveBestNetwork;

    public Settings()
    {
        loadNetworkAtStart = false;
        saveBestNetwork = false;
    }
}

[Serializable]
public class TrainingLog
{
    public int generation;
    public float medianFitness;
    public float topFitness;
}