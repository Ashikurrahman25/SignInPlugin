using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Rest API Hepler/New", order = 1)]
public class Config : ScriptableObject
{
    public string webAPI;
    public string webClientId;
    public string webClientIdIOS;
    public string authDatabaseURL;
    public string gameName;
}
