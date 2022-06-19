using DG.Tweening;
using Google;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestAPIHelper;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public int transitionValue;
    public Config config;
    public CanvasGroup loadingPanel;
    public TMP_InputField regEmail;
    public TMP_InputField regName;
    public TMP_InputField regPass;
    public TMP_InputField loginEmail;
    public TMP_InputField loginPass;
    public TMP_InputField passResetEmail;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI ghostCoinText;
    public TextMeshProUGUI totalGameText;
    public TextMeshProUGUI gamesText;
    public GameObject confirmPanel;
    public GameObject ghostProfile;
    public GameObject warningPanel;

    APIHelper api;
    Authentication auth;
    public static UIController instance;

    private void Awake()
    {
        api = new APIHelper(config.webAPI, config.authDatabaseURL);
        auth = new Authentication(config, api);
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        TryAutoLogin();
    }

   
    public void WarnFacebookLogin()
    {
        HideLoadingPanel();
        MoveIn(warningPanel);
    }
    public void TryAutoLogin()
    {
        ShowLoadingPanel();
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("refreshToken", "")))
        {
            string refreshToken = PlayerPrefs.GetString("refreshToken");
            api.AutoLogin(refreshToken, (response) =>
             {
                 //HideLoadingPanel();
                 var txt = JObject.Parse(response);
                 string uID = txt["user_id"].ToString();
                 string idToken = txt["id_token"].ToString();

                 api.CheckEmailVerified(idToken, (bool isVerified) =>
                 {
                     if (!isVerified)
                     {
                         //api.SendVerificationEmail(idToken);
                         MoveIn(confirmPanel);
                         HideLoadingPanel();
                     }
                     else
                     {
                         api.GetData("user", uID, idToken, (str) =>
                         {
                             if (!str.Equals("null"))
                             {
                                 GhostUser ghostUser = JsonConvert.DeserializeObject<GhostUser>(str);
                                 nameText.text = ghostUser.Name;
                                 emailText.text = ghostUser.Email;
                                 ghostCoinText.text = ghostUser.GhostCoin.ToString("00");
                                 totalGameText.text = ghostUser.Games.Count.ToString("00");

                                 CheckGameList(uID, idToken, ghostUser);
                                 ghostProfile.SetActive(true);
                             }
                         });
                     }
                 });
             });
        }
        else
        {
            HideLoadingPanel();
        }
    }
    public void SignUp()
    {
        if (string.IsNullOrEmpty(regEmail.text) || string.IsNullOrWhiteSpace(regEmail.text) ||
           string.IsNullOrEmpty(regPass.text) || string.IsNullOrWhiteSpace(regPass.text) ||
           string.IsNullOrEmpty(regName.text) || string.IsNullOrWhiteSpace(regName.text))
        {
            Alert.instance.Init("Inputs Required", "Please fill all inputs", false);
            return;
        }

        ShowLoadingPanel();
        api.HandleEmailPassword(regEmail.text, regPass.text, true, (string str) =>
        {
            OnSignUp(str);
        });
    }
    public void Login()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrWhiteSpace(loginEmail.text) ||
         string.IsNullOrEmpty(loginPass.text) || string.IsNullOrWhiteSpace(loginPass.text))
        {
            Alert.instance.Init("Inputs Required", "Please fill all inputs", false);
            return;
        }

        ShowLoadingPanel();
        api.HandleEmailPassword(loginEmail.text, loginPass.text, false, (string str) =>
        {
            OnLogin(str);
        });
    }
    void OnSignUp(string str)
    {
        var response = JObject.Parse(str);
        string uID = response["localId"].ToString();
        string idToken = response["idToken"].ToString();

        api.SendVerificationEmail(idToken);

        GhostUser ghostUser = new GhostUser();
        ghostUser.Email = response["email"].ToString();
        ghostUser.Name = regName.text;
        ghostUser.UID = uID;
        ghostUser.Name = regName.text;
        ghostUser.GhostCoin = 0;
        ghostUser.Games.Add(config.gameName);

        api.PutData("user", uID, ghostUser, idToken);
        HideLoadingPanel();
        MoveIn(confirmPanel);
    }
    void OnLogin(string str)
    {
        var response = JObject.Parse(str);
        string uID = response["localId"].ToString();
        string idTOken = response["idToken"].ToString();

        api.CheckEmailVerified(idTOken, (bool verified) =>
        {
            HideLoadingPanel();

            if (!verified)
            {
                api.LogOut();
                Alert.instance.Init("Verify Email", "Please verify your email to continue", false);
            }
            else
            {
                Debug.Log("Logging in");

                api.GetData("user", uID, idTOken, (string str) =>
                 {
                     Debug.Log(str);

                     if (!str.Equals("null"))
                     {
                         Global.ghostUser = JsonConvert.DeserializeObject<GhostUser>(str);
                         nameText.text = Global.ghostUser.Name;
                         emailText.text = Global.ghostUser.Email;
                         ghostCoinText.text = Global.ghostUser.GhostCoin.ToString("00");
                         totalGameText.text = Global.ghostUser.Games.Count.ToString("00");

                         CheckGameList(uID, idTOken, Global.ghostUser);
                         ghostProfile.SetActive(true);
                     }
                 });
            }
        });
    }

    public void AppleLogin()
    {
        ShowLoadingPanel();
        auth.AppleLogin((data) =>
        {
            api.GetData("user", uID, idToken, (userData) =>
            {
                if (userData.Equals("null"))
                {
                    Global.ghostUser.Name = txt["fullName"].ToString();
                    Global.ghostUser.GhostCoin = 0;
                    Global.ghostUser.Email = txt["email"].ToString();
                    Global.ghostUser.UID = uID;

                    api.PutData("user", uID, Global.ghostUser, idToken);
                }
                else
                    Global.ghostUser = JsonConvert.DeserializeObject<GhostUser>(userData);


                nameText.text = Global.ghostUser.Name;
                emailText.text = Global.ghostUser.Email;
                ghostCoinText.text = Global.ghostUser.GhostCoin.ToString("00");

                CheckGameList(uID, idToken, Global.ghostUser);
                totalGameText.text = Global.ghostUser.Games.Count.ToString("00");
                ghostProfile.SetActive(true);
                HideLoadingPanel();
            });
        });
    }

    public void GoogleLogin()
    {
        ShowLoadingPanel();
        auth.GoogleLogin((data) =>
        {
            CommonLogin(data);
        });
    }
    public void FacebookLogin()
    {
        ShowLoadingPanel();
        auth.FacebookLogin();
    }

    public void HandleFacebookLogin(string data)
    {
        CommonLogin(data);
    }

    public void CommonLogin(string data)
    {
        Debug.Log(data);

        var txt = JObject.Parse(data);
        string uID = txt["localId"].ToString();
        string idToken = txt["idToken"].ToString();

        api.CheckEmailVerified(idToken, (bool isVerified) =>
        {
            if (!isVerified)
            {
                if (txt.ContainsKey("isNewUser"))
                    api.SendVerificationEmail(idToken);


                MoveIn(confirmPanel);
                HideLoadingPanel();
            }
            else
            {
                api.GetData("user", uID, idToken, (userData) =>
                {
                    if (userData.Equals("null"))
                    {
                        Global.ghostUser.Name = txt["fullName"].ToString();
                        Global.ghostUser.GhostCoin = 0;
                        Global.ghostUser.Email = txt["email"].ToString();
                        Global.ghostUser.UID = uID;

                        api.PutData("user", uID, Global.ghostUser, idToken);
                    }
                    else
                        Global.ghostUser = JsonConvert.DeserializeObject<GhostUser>(userData);


                    nameText.text = Global.ghostUser.Name;
                    emailText.text = Global.ghostUser.Email;
                    ghostCoinText.text = Global.ghostUser.GhostCoin.ToString("00");

                    CheckGameList(uID, idToken, Global.ghostUser);
                    totalGameText.text = Global.ghostUser.Games.Count.ToString("00");
                    ghostProfile.SetActive(true);
                    HideLoadingPanel();
                });
            }
        });
    }

    public void CheckGameList(string uID, string idToke, GhostUser ghostUser)
    {
        int gameIndex = ghostUser.Games.FindIndex(game => game.Equals(config.gameName));

        if (gameIndex != -1)
        {
            Debug.Log("Game Already Exists in the list");

            gamesText.text = "";
            foreach (var game in ghostUser.Games)
            {
                if (!gamesText.text.Equals(""))
                    gamesText.text += $"\n• {game}";
                else
                    gamesText.text = $"• {game}";
            }
        }
        else
        {
            Debug.Log("Game list must update");
            ghostUser.Games.Add(config.gameName);
            api.UpdateData("user", uID, idToke, ghostUser, (data) =>
            {
                gamesText.text = "";
                foreach (var game in ghostUser.Games)
                {
                    if (!gamesText.text.Equals(""))
                        gamesText.text += $"\n• {game}";
                    else
                        gamesText.text = $"• {game}";
                }
                Debug.Log("Game Data Updated");
            });
        }
    }

    public void ResetPassword()
    {
        ShowLoadingPanel();
        api.SendPasswordReset(passResetEmail.text, () =>
        {
            HideLoadingPanel();
        });
    }

    public void LogOut()
    {
        api.LogOut();
    }

    public void MoveIn(GameObject panelToMove)
    {
        panelToMove.DoHorizontalTransition(0f, null);

    }

    public void MoveOut(GameObject panelToMove)
    {
        panelToMove.DoHorizontalTransition(transitionValue, null);
    }

    public void ShowLoadingPanel()
    {
        loadingPanel.gameObject.SetActive(true);
        loadingPanel.DOFade(1f, .3f);
    }

    public void HideLoadingPanel()
    {
        loadingPanel.DOFade(0f, .3f).OnComplete(() =>
        {
            loadingPanel.gameObject.SetActive(false);
        });
    }
}
