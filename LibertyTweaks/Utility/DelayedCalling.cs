using IVSDKDotNet;
using System;
using System.Collections.Generic;

// Credits: ItsClonkAndre

public class DelayedCalling
{
    private readonly List<DelayedCall> delayedCalls;

    private class DelayedCall
    {
        public DateTime CallIn;
        public Action TheAction;
        public string CallerName;

        public DelayedCall(DateTime time, Action a, string callerName)
        {
            CallIn = time;
            TheAction = a;
            CallerName = callerName;
        }
    }

    /// <summary>
    /// Initializes a new instance of the DelayedCalling class.
    /// </summary>
    public DelayedCalling()
    {
        delayedCalls = new List<DelayedCall>();
    }

    /// <summary>
    /// Processes the delayed calls that are due.
    /// </summary>
    public void Process()
    {
        DateTime now = DateTime.UtcNow;

        for (int i = 0; i < delayedCalls.Count; i++)
        {
            DelayedCall delayedCall = delayedCalls[i];

            if (delayedCall.CallIn < now)
            {
                try
                {
                    // Execute the delayed call
                    delayedCall.TheAction?.Invoke();
                }
                catch (Exception ex)
                {
                    // Log the error
                    LogError($"An error occurred while processing delayed call for {delayedCall.CallerName}! Details: {ex}");
                }

                delayedCalls.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Adds a delayed call to be executed after the specified time.
    /// </summary>
    /// <param name="executeIn">Time span after which to execute the action.</param>
    /// <param name="className">Name of the class initiating the delayed call.</param>
    /// <param name="actionToCall">Action to be executed.</param>
    public void Add(TimeSpan executeIn, string className, Action actionToCall)
    {
        if (actionToCall == null)
            return;

        DateTime executeAt = DateTime.UtcNow.Add(executeIn);
        lock (delayedCalls)
        {
            delayedCalls.Add(new DelayedCall(executeAt, actionToCall, className));
        }
    }

    /// <summary>
    /// Clears all pending delayed calls.
    /// </summary>
    public void ClearAll()
    {
        lock (delayedCalls)
        {
            delayedCalls.Clear();
        }
    }

    // Replace with your own logging mechanism if available
    private void LogError(string message)
    {
        IVGame.Console.PrintError(message);
    }
}
