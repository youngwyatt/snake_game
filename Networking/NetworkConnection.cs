﻿// <copyright file="NetworkConnection.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>
/// <authors>
/// Brian Keller & Wyatt Young
/// </authors>
/// <version>
/// November 3rd, 2024
/// </version>

using System.Net.Sockets;
using System.Text;
namespace CS3500.Networking;

/// <summary>
///   Wraps the StreamReader/Writer/TcpClient together so we
///   don't have to keep creating all three for network actions.
/// </summary>
public sealed class NetworkConnection : IDisposable
{
    /// <summary>
    ///   The connection/socket abstraction
    /// </summary>
    private TcpClient _tcpClient = new();

    /// <summary>
    ///   Reading end of the connection
    /// </summary>
    private StreamReader? _reader = null;

    /// <summary>
    ///   Writing end of the connection
    /// </summary>
    private StreamWriter? _writer = null;

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.
    ///   </para>
    /// </summary>
    /// <param name="tcpClient">
    ///   An already existing TcpClient
    /// </param>
    public NetworkConnection( TcpClient tcpClient )
    {
        _tcpClient = tcpClient;
        if ( IsConnected )
        {
            // Only establish the reader/writer if the provided TcpClient is already connected.
            _reader = new StreamReader( _tcpClient.GetStream(), Encoding.UTF8 );
            _writer = new StreamWriter( _tcpClient.GetStream(), Encoding.UTF8 ) { AutoFlush = true }; // AutoFlush ensures data is sent immediately
        }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.  The tcpClient will be unconnected at the start.
    ///   </para>
    /// </summary>
    public NetworkConnection( )
        : this( new TcpClient( ) )
    {
    }

    /// <summary>
    /// Gets a value indicating whether the socket is connected.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return _tcpClient.Connected;
        }
    }


    /// <summary>
    ///   Try to connect to the given host:port. 
    /// </summary>
    /// <param name="host"> The URL or IP address, e.g., www.cs.utah.edu, or  127.0.0.1. </param>
    /// <param name="port"> The port, e.g., 11000. </param>
    public void Connect( string host, int port )
    {
        try
        {
            _tcpClient.Connect(host, port);
            _reader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8);
            _writer = new StreamWriter(_tcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }


    /// <summary>
    ///   Send a message to the remote server.  If the <paramref name="message"/> contains
    ///   new lines, these will be treated on the receiving side as multiple messages.
    ///   This method should attach a newline to the end of the <paramref name="message"/>
    ///   (by using WriteLine).
    ///   If this operation can not be completed (e.g. because this NetworkConnection is not
    ///   connected), throw an InvalidOperationException.
    /// </summary>
    /// <param name="message"> The string of characters to send. </param>
    public void Send( string message )
    {
        // check for connection, if not connected throw new exception
        if (_writer == null || !IsConnected) 
        {
            throw new InvalidOperationException("Cannot send message: writer disconnected");
        }
        // send message using writer and Writeline to ensure message starts with new line
        _writer.WriteLine( message );
    }


    /// <summary>
    ///   Read a message from the remote side of the connection.  The message will contain
    ///   all characters up to the first new line. See <see cref="Send"/>.
    ///   If this operation can not be completed (e.g. because this NetworkConnection is not
    ///   connected), throw an InvalidOperationException.
    /// </summary>
    /// <returns> The contents of the message. </returns>
    public string? ReadLine( )
    {
        // check for connection, if not connected throw new exception
        if (_reader == null || !IsConnected)
        {
            throw new InvalidOperationException("Cannot read message: reader/network disconnected");
        }
        string? message = _reader.ReadLine();
        return message;
    }

    /// <summary>
    ///   If connected, disconnect the connection and clean 
    ///   up (dispose) any streams.
    /// </summary>
    public void Disconnect( )
    {
        if (IsConnected) 
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _tcpClient.Dispose();
            _tcpClient = new();
        }
    }

    /// <summary>
    ///   Automatically called with a using statement (see IDisposable)
    /// </summary>
    public void Dispose( )
    {
        Disconnect();
    }
}

