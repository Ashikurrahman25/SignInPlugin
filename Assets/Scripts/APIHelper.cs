using Google;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proyecto26;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RestAPIHelper
{
    public enum Provider { Google, Facebook, Apple }
    public class APIHelper
    {
        string WebAPI { get;  set; }
        string DatabaseURL { get;  set; }

        public APIHelper(string webAPI, string databaseURL)
        {
            this.WebAPI = webAPI;
            this.DatabaseURL = databaseURL;
            Debug.Log("Rest API Initialized");
        }


        public void AutoLogin(string refreshToken, Action <string> responseTxt)
        {
            Relogin req = new Relogin();
            req.grant_type = "refresh_token";
            req.refresh_token = refreshToken;
            var body_string = JsonConvert.SerializeObject(req);
            RestClient.Request(new RequestHelper
            {
                Uri = $"https://securetoken.googleapis.com/v1/token?key={WebAPI}",
                Method = "POST",
                Timeout = 10,
                BodyString = body_string

            }).Then(response =>
            {
                ResponseHelper res = response;
                Debug.Log(res.StatusCode);
                responseTxt(res.Text);
                var txt = JObject.Parse(res.Text);
                SaveRefreshToken(txt["id_token"].ToString(), txt["refresh_token"].ToString());
            }).Catch(err =>
            {
                var (errorCode, errorMsg) = ReqErrorHelper.GetParsedError(err);
                Debug.LogError(errorMsg);

                UIController.instance.HideLoadingPanel();
                Alert.instance.Init("Login Failed", "Please login again to continue", false);
            });
        }


        public void LoginWithProvider(Provider provider, string token, Action <string> data)
        {
            ProviderRequest req = new ProviderRequest();

            switch (provider)
            {
                case Provider.Google:
                    req.postBody = $"id_token={token}&providerId=google.com";
                    break;
                case Provider.Facebook:
                    req.postBody = $"access_token={token}&providerId=facebook.com";
                    break;
                case Provider.Apple:
                    break;
                default:
                    break;
            }

            
            req.requestUri = "http://localhost:9000";
            req.returnIdpCredential = true;
            req.returnSecureToken = true;

            var body_string = JsonConvert.SerializeObject(req);
            RestClient.Request(new RequestHelper
            {
                Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={WebAPI}",
                Method = "POST",
                Timeout = 10,
                BodyString = body_string

            }).Then(response =>
            {
                ResponseHelper res = response;
                var txt = JObject.Parse(res.Text);

                if(provider == Provider.Facebook)
                {
                    if (txt.ContainsKey("needConfirmation"))
                    {
                        UIController.instance.WarnFacebookLogin();
                    }
                }

                data(res.Text);
                SaveRefreshToken(txt["idToken"].ToString(), txt["refreshToken"].ToString());
            }).Catch(err =>
            {
                var (errorCode, errorMsg) = ReqErrorHelper.GetParsedError(err);
                Debug.LogError(errorMsg);
                Alert.instance.Init("Error", errorMsg, false);
                UIController.instance.HideLoadingPanel();
            });
        }

        public void HandleEmailPassword(string email, string password, bool isSignUp, Action<string> onSuccess)
        {
            EmailPassRequest req = new EmailPassRequest();
            req.email = email.Trim().ToLower();
            req.password = password;
            req.returnSecureToken = true;
            var body_string = JsonConvert.SerializeObject(req);

            string uri = "";

            if (isSignUp)
                uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={WebAPI}";
            else
                uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={WebAPI}";

            RestClient.Request(new RequestHelper
            {
                Uri = uri,
                Method = "POST",
                Timeout = 10,
                BodyString = body_string

            }).Then(response =>
            {
                ResponseHelper res = response;
                Debug.Log(res.StatusCode);
                Debug.Log(res.Text);
                var txt = JObject.Parse(res.Text);
                SaveRefreshToken(txt["idToken"].ToString(), txt["refreshToken"].ToString());
                onSuccess(res.Text);

            }).Catch(err =>
            {
                var (errorCode, errorMsg) = ReqErrorHelper.GetParsedError(err);
                Debug.LogError(errorMsg);
                UIController.instance.HideLoadingPanel();
                Alert.instance.Init("Error", errorMsg, false);
            });
        }

        public void CheckEmailVerified(string tokenID,Action <bool> emailVerified)
        {
            UserAPIReq req = new UserAPIReq();
            req.idToken = tokenID;
            var body_string = JsonConvert.SerializeObject(req);
            RestClient.Request(new RequestHelper
            {
                Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={WebAPI}",
                Method = "POST",
                Timeout = 10,
                BodyString = body_string
            }).Then(response =>
            {
                ResponseHelper res = response;
                var txt = JObject.Parse(res.Text);
                bool isVerified = txt["emailVerified"].ToString().ToLower().Equals("true") ? true : false;
                emailVerified (isVerified);
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError(err);
                UIController.instance.HideLoadingPanel();
                Alert.instance.Init("Error Occuered!", "An unexpected error occured, try again later", false);
            });
        }

        public void SendVerificationEmail(string token)
        {
            EmailVerification req = new EmailVerification();
            req.requestType = "VERIFY_EMAIL";
            req.idToken = token;

            var body_string = JsonConvert.SerializeObject(req);
            RestClient.Request(new RequestHelper
            {
                Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={WebAPI}",
                Method = "POST",
                Timeout = 10,
                BodyString = body_string
            }).Then(response =>
            {
                ResponseHelper res = response;
                Alert.instance.Init("Verify Email", "A verification email has been sent to your email", true);

            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError(err);
                Alert.instance.Init("Error Occuered!", "An unexpected error occured, try again later", false);
            });
        }

        public void SendPasswordReset(string email, Action onSuccess)
        {
            PasswordReset req = new PasswordReset();
            req.requestType = "PASSWORD_RESET";
            req.email = email;
            var body_string = JsonConvert.SerializeObject(req);
            RestClient.Request(new RequestHelper
            {
                Uri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={WebAPI}",
                Method = "POST",
                Timeout = 10,
                BodyString = body_string
            }).Then(response =>
            {
                ResponseHelper res = response;
                var txt = JObject.Parse(res.Text);
                Debug.Log(txt["email"]);
                onSuccess?.Invoke();
                Alert.instance.Init("Reset Password", "Password reset link was sent to your email", true);

            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError(err);
                UIController.instance.HideLoadingPanel();
                Alert.instance.Init("Error Occuered!", "An unexpected error occured, try again later", false);

            });
        }

        public void PutData <T>(string child,string identifier, T data, string idToken) where T : class
        {
            var body_string = JsonConvert.SerializeObject(data);
            //Debug.Log(data);
            RestClient.Request(new RequestHelper
            {
                Uri = $"{DatabaseURL}/{child}/{identifier}.json?auth={idToken}",
                Method = "PUT",
                Timeout = 10,
                BodyString = body_string

            }).Then(response =>
            {
                ResponseHelper res = response;
                Debug.Log(res.StatusCode);
                Debug.Log(res.Text);
                
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError(err);
            });
        }

        public void GetData (string child, string identifier, string idToken, Action<string> userData)
        {
            RestClient.Request(new RequestHelper
            {
                Uri = $"{DatabaseURL}/{child}/{identifier}.json?auth={idToken}",
                Method = "GET",
                Timeout = 10

            }).Then(response =>
            {
                ResponseHelper res = response;
                userData(res.Text);
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError(err);
            });
        }

        public void UpdateData(string child, string identifier,string idToken, GhostUser data, Action <string> updatedData)
        {
            var body_string = JsonConvert.SerializeObject(data);
            RestClient.Request(new RequestHelper
            {
                Uri = $"{DatabaseURL}/{child}/{identifier}.json?auth={idToken}",
                Method = "PATCH",
                Timeout = 1,
                BodyString = body_string
            }).Then(response =>
            {
                ResponseHelper res = response;
                updatedData(res.Text);
            }).Catch(err =>
            {
                var error = err as RequestException;
                Debug.LogError(err);
            });
        }
      

        public void LogOut()
        {
            PlayerPrefs.SetString("idToken", "");
            PlayerPrefs.SetString("refreshToken", "");

            Debug.Log("Successfully logged out");
        }

        public void SaveRefreshToken(string idToken, string refreshToken)
        {
            PlayerPrefs.SetString("idToken", idToken);
            PlayerPrefs.SetString("refreshToken", refreshToken);

            Debug.Log("Stored refresh token");
        }

    }
}


