﻿namespace TNRD.Zeepkist.GTR.Stream;

internal class RabbitOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
