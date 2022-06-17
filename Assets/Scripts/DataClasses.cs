using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UserAPIReq
{
    public string idToken;

    public UserAPIReq()
    {

    }
}

public class EmailVerification
{
    public string requestType;
    public string idToken;

    public EmailVerification()
    {

    }
}

public class PasswordReset
{
    public string requestType;
    public string email;

    public PasswordReset()
    {

    }
}

public class Token
{
    public string token;

    public Token()
    {

    }
}


public class Relogin
{
    public string grant_type;
    public string refresh_token;
}

[Serializable]
public class GhostUser
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string UID { get; set; }
    public int GhostCoin { get; set; }

    public List<string> Games { get; set; } = new List<string>();


}

[Serializable]
public class ProviderRequest
{
    public string postBody;
    public string requestUri;
    public bool returnIdpCredential;
    public bool returnSecureToken;

    public ProviderRequest()
    {

    }
}

[Serializable]
public class EmailPassRequest
{
    public string email;
    public string password;
    public bool returnSecureToken;

    public EmailPassRequest()
    {

    }
}

