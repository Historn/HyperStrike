using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public static class ServerSender
{
    private const string ServerUrl = "https://citmalumnes.upc.es/~joelcm/receiveData.php";

    public static IEnumerator SendData(string jsonData)
    {
        // Format the date correctly in the jsonData before sending.
        string formattedJsonData = FormatDateInJson(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(ServerUrl, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(formattedJsonData);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data sent successfully: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Failed to send data: " + request.error + "\nResponse: " + request.downloadHandler.text);
            }

        }
    }

    private static string FormatDateInJson(string jsonData)
    {
        // Regular expression to match ISO 8601 datetime format (e.g., 2025-01-08 16:41:47.51)
        string datePattern = @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z)";

        // Replace all instances of the ISO 8601 datetime in the jsonData with the desired format
        string formattedJsonData = Regex.Replace(jsonData, datePattern, (match) =>
        {
            // Parse the date string and convert it to the required format
            DateTime parsedDate = DateTime.Parse(match.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
            return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
        });

        return formattedJsonData;
    }
}
