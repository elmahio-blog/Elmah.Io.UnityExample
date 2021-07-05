using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Elmah.Io.Client;
using System.Net;

public class Logging : MonoBehaviour
{
    // This function is always called before any Start functions
    void Awake()
    {
        Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private IElmahioAPI api;

    private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {
        if (api == null)
        {
            api = ElmahioAPI.Create("API_KEY");
        }

        Severity severity = Severity.Debug;
        switch (type)
        {
            case LogType.Assert:
                severity = Severity.Information;
                break;
            case LogType.Error:
                severity = Severity.Error;
                break;
            case LogType.Exception:
                severity = Severity.Fatal;
                break;
            case LogType.Log:
                severity = Severity.Verbose;
                break;
            case LogType.Warning:
                severity = Severity.Warning;
                break;
        }

        var message = new CreateMessage()
        {
            Title = condition,
            Detail = stackTrace,
            Severity = severity.ToString(),
            Url = Application.absoluteURL
        };

        api.Messages.Create("log-id", message);
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        if (api == null)
        {
            api = ElmahioAPI.Create("API_KEY");
        }

        var exception = e.Exception;
        var baseException = exception?.GetBaseException();

        var message = new CreateMessage()
        {
            Data = exception?.ToDataList(),
            DateTime = DateTime.UtcNow,
            Detail = exception?.ToString(),
            Severity = "Error",
            Source = baseException?.Source,
            Title = baseException?.Message ?? "Unhandled Unity exception",
            Type = baseException?.GetType().FullName,
        };

        api.Messages.Create("log-id", message);
    }


    private List<string> list { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("Before Null");
        list.Add("Hello");
    }

    // Update is called once per frame
    void Update()
    {
    }

    // This function is called after all frame updates for the last frame of the object’s existence
    void OnDestroy()
    {
        Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
        TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
    }
}
