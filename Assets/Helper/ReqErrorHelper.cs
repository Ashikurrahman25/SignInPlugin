using FirebaseRestClient;
using System;
using System.Linq;

public static class ReqErrorHelper
{
    public static (int, string) GetParsedError(Exception err)
    {
        var reqEx = (Proyecto26.RequestException)err;

        if (reqEx.IsNetworkError)
            return (0, "Network Error");

        var errDict = RequestErrorHelper.ToDictionary(err);
        switch (errDict.Values.First())
        {
            case "INVALID_PASSWORD":
                return (400, "Invalid Password");

            case "INVALID_EMAIL":
                return (400, "Invalid Email");

            case "TOO_MANY_ATTEMPTS_TRY_LATER : " +
            "Access to this account has been temporarily disabled due to many failed login attempts. " +
            "You can immediately restore it by resetting your password or you can try again later.":
                {
                    return (400, "Temporarily disabled due to many failed login attempts.");
                }

            case "EMAIL_EXISTS":
                return (400, "This email already exist, try new email.");

            //case "WEAK_PASSWORD : Password should be at least 6 characters":
            //    return (400, "Password should be at least 6 characters.");

            default:
                return (errDict.Keys.First(), errDict.Values.First());
        }
    }
}