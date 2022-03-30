using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils
{
    // Pesquisa objetos pela tag, incluindo também objetos desativados na cena
    public static GameObject FindObjectByTag(string tag)
    {
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            if (!(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                if (go.scene != null && go.CompareTag(tag))
                    return go;
        return null;
    }

    // Embaralha o array
    public static void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int i = Random.Range(0, n--);
            T tmp = array[n];
            array[n] = array[i];
            array[i] = tmp;
        }
    }

    // Encontra o ponto no raio mais próximo ao parametro point, dadas distancias das extremidades e o número de passos
    public static Vector3 ClosestRayPoint(Vector3 origin, Vector3 direction, float startDist, float endDist, Vector3 point, int steps)
    {
        Vector3 closestPoint = origin + direction * startDist;
        float closestDistance = (closestPoint - point).sqrMagnitude;
        for (int i = 0; i < steps; i++)
        {
            float t = ((i + 1f) / steps) * (endDist - startDist) + startDist;
            Vector3 rayPoint = origin + direction * t;
            float distance = (rayPoint - point).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestPoint = rayPoint;
                closestDistance = distance;
            }
        }
        return closestPoint;
    }

    // Troca o alcance do parametro value que está entre os dois primeiros parametros, para entre o terceiro e quarto parametros
    public static float Remap(float fromStart, float fromEnd, float toStart, float toEnd, float value)
    {
        float t = (value - fromStart) / (fromEnd - fromStart);
        return t * (toEnd - toStart) + toStart;
    }

    // Cria um array misturado a partir de uma porcentagem e tamanho, opcionalmente desliza os valores para conter um true no último
    public static bool[] CreateRandomChanceArray(int size, float percentage, bool slideToEnd = true)
    {
        bool[] chances = new bool[size];
        for (int i = 0; i < size; i++)
            if (i < (int)(size * percentage))
                chances[i] = true;
        Utils.Shuffle(chances);

        if (slideToEnd)
        {   // desliza as chances para que a última sempre contenha um true
            while (!chances[size - 1])
            {
                for (int i = size - 1; i > 0; i--)
                    chances[i] = chances[i - 1];
                chances[0] = false;
            }
        }

        return chances;
    }

    // Extensão que permite executar uma ação em cada elemento duma lista
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T element in source)
        {
            action(element);
        }
    }

    // Extensão que permite executar uma ação em cada elemento duma lista
    public static void For<T>(this IEnumerable<T> source, Action<int, T> action)
    {
        int i = 0;
        foreach (T element in source)
        {
            action(i, element);
            i++;
        }
    }
}
