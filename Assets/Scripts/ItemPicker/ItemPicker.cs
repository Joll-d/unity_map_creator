using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class ItemPicker : MonoBehaviour
{

    public string startingPath = @"D:\Project\Unity\MapCreator\My project (1)\Assets\Resources\Prefabs";
    private List<string> _prefabPaths = new List<string>();
    void Start()
    {
        TraverseDirectoryBreadthFirst(startingPath);
    }

    private void TraverseDirectoryBreadthFirst(string path)
    {
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(path);

        while (queue.Count > 0)
        {
            string currentPath = queue.Dequeue();

            GetAllFilePathsInFolder(currentPath);

            string[] subdirectories = Directory.GetDirectories(currentPath);
            foreach (string subdirectory in subdirectories)
            {
                queue.Enqueue(subdirectory);
            }

        }

    }

    private void GetAllFilePathsInFolder(string folderPath)
    {
        string[] files = Directory.GetFiles(folderPath, "*.prefab");
        _prefabPaths.AddRange(files);
    }

    
}
