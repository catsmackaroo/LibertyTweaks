using IVSDKDotNet;
using System;
using System.Collections.Generic;

public class DelayedCalling
{
    #region Variables
    private List<DelayedCall> delayedCalls;
    #endregion

    #region Classes
    private class DelayedCall
    {
        #region Variables
        public DateTime CallIn;
        public Action TheAction;
        public string CallerName;
        #endregion

        #region Constructor
        public DelayedCall(DateTime time, Action a, string callerName)
        {
            CallIn = time;
            TheAction = a;
            CallerName = callerName;
        }
        #endregion
    }
    #endregion

    #region Constructor
    public DelayedCalling()
    {
        delayedCalls = new List<DelayedCall>();
    }
    #endregion

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
                    // TODO: Maybe replace with your own logging method
                    IVGame.Console.PrintError(string.Format("An error occured while processing delayed calling queue for {0}! Details: {1}", delayedCall.CallerName, ex));
                }

                delayedCalls.RemoveAt(i);
                i--;
            }
        }
    }
    public void Add(TimeSpan executeIn, string className, Action actionToCall)
    {
        if (actionToCall == null)
            return;

        delayedCalls.Add(new DelayedCall(DateTime.UtcNow.Add(executeIn), actionToCall, className));
    }
	public void ClearAll()
    {
        delayedCalls.Clear();
    }
}