using UnityEngine;
using VALVE;
using NPKEVIN.Utils;

public class MapLoader : MonoBehaviour
{
    public MAP map;

    public void Start()
    {
        if (gameObject.name != "Map")
            UnloadMap();
        LoadMap();
    }

    public void LoadMap()
    {
        string mapPath = Application.dataPath + "/vampire/maps/";

        BSP bsp = new BSP(mapPath, map.ToString());
        BSPTools tool = new BSPTools();
        tool.GenerateMap(bsp, gameObject);
    }

    public void UnloadMap()
    {
        gameObject.name = "Map";
        // one foreach loop doesnt clear all children for some strange reason
        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);
        }
    }
}

public enum MAP
{
    ch_cloud_1,
    ch_dragon_1,
    ch_fishmarket_1,

    sm_apartment_1,
    sm_asylum_1,
    sm_hub_1,
    sm_hub_2,
}
