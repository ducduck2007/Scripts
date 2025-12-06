using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RequestDataWhenLoggedIn
{
    private static Queue<Action> _queue = new Queue<Action>();

    private static int _count;
    private static int _maxRequestInit;
    private static bool _isInitQueue;
    
    public static Queue<Action> RequestQueue()
    {
        int dataMin = 5;
        // _queue.Enqueue(SendData.OnConfigTimeInfo);
        
        // OnProcessInitGame();
        return _queue;
    }
}