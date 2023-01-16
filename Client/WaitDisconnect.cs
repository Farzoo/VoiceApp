﻿using NetLib.Server;

namespace Client;

public class WaitDisconnect
{
    private readonly ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);

    public WaitDisconnect(BaseClient client)
    {
        client.RegisterOnDisconnect(this.OnDisconnect);
    }

    public void Wait()
    {
        this._waitHandle.Wait();
    }

    private void OnDisconnect(BaseClient client)
    {
        this._waitHandle.Set();
    }
}