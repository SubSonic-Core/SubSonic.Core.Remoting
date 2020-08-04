﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    /// <summary>
    /// Provides required functions and properties for the receiver channels.
    /// </summary>
    [ComVisible(true)]
    public interface IChannelReceiver
        : IChannel
    {
        /// <summary>
        /// Gets the channel-specific data.
        /// </summary>
        /// <returns>The channel data.</returns>
        object ChannelData { get; }
        /// <summary>
        /// Returns an array of all the URLs for a URI.
        /// </summary>
        /// <param name="objectURI">The URI for which URLs are required.</param>
        /// <returns>An array of the URLs.</returns>
        [SecurityCritical]
        string[] GetUrlsForUri(string objectURI);
        /// <summary>
        /// Instructs the current channel to start listening for requests.
        /// </summary>
        /// <param name="data">Optional initialization information.</param>
        [SecurityCritical]
        void StartListening(object data);
        /// <summary>
        /// Instructs the current channel to stop listening for requests.
        /// </summary>
        /// <param name="data">Optional state information for the channel.</param>
        [SecurityCritical]
        void StopListening(object data);
    }

    /// <summary>
    /// Provides conduits for messages that cross process boundaries.
    /// </summary>
    [ComVisible(true)]
    public interface IChannel
    {
        /// <summary>
        /// Gets the priority of the channel.
        /// </summary>
        /// <returns>An integer that indicates the priority of the channel.</returns>
        int ChannelPriority { get; }
        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        /// <returns>The name of the channel.</returns>
        string ChannelName { get; }

        IChannel Initialize();
        /// <summary>
        /// Returns the object URI as an out parameter, and the URI of the current channel
        /// as the return value.
        /// </summary>
        /// <param name="url">The URL of the object.</param>
        /// <param name="method">When this method returns, contains a System.String that holds the object URI.
        /// This parameter is passed uninitialized.</param>
        /// <returns></returns>
        [SecurityCritical]
        string Parse(Uri uri, out string method);
    }
    /// <summary>
    /// Provides required functions and properties for the sender channels.
    /// </summary>
    [ComVisible(true)]
    public interface IChannelSender
        : IChannel
    {
        /// <summary>
        /// Returns a channel message sink that delivers messages to the specified URL or
        /// channel data object.
        /// </summary>
        /// <param name="url">The URL to which the new sink will deliver messages. Can be null.</param>
        /// <param name="remoteChannelData">The channel data object of the remote host to which the new sink will deliver
        /// messages. Can be null.</param>
        /// <param name="objectURI">When this method returns, contains a URI of the new channel message sink that
        /// delivers messages to the specified URL or channel data object. This parameter
        /// is passed uninitialized.</param>
        /// <returns>A channel message sink that delivers messages to the specified URL or channel
        /// data object, or null if the channel cannot connect to the given endpoint.</returns>
        [SecurityCritical]
        IMessageSink CreateMessageSink(Uri url, object remoteChannelData, out string objectURI);
        /// <summary>
        /// Get all the properties for this channel
        /// </summary>
        IDictionary Properties { get; }
    }
    /// <summary>
    /// provides information that a named client will need
    /// </summary>
    [ComVisible(true)]
    public interface INamedPipeChannelSender
        : IChannelSender
    {
        bool AutoReconnect { get; }
        TimeSpan? ReconnectionInterval { get; }
        Uri ChannelUri { get; }
    }
    /// <summary>
    /// Defines the interface for a message sink.
    /// </summary>
    [ComVisible(true)]
    public interface IMessageSink
    {
        /// <summary>
        /// Gets the next message sink in the sink chain.
        /// </summary>
        /// <returns>The next message sink in the sink chain.</returns>
        IMessageSink NextSink { get; }
        /// <summary>
        /// Asynchronously processes the given message.
        /// </summary>
        /// <param name="msg">The message to process.</param>
        /// <param name="replySink">The reply sink for the reply message.</param>
        /// <returns>An System.Runtime.Remoting.Messaging.IMessageCtrl interface that provides a way
        /// to control asynchronous messages after they have been dispatched.</returns>
        [SecurityCritical]
        IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink);
        /// <summary>
        /// Synchronously processes the given message.
        /// </summary>
        /// <param name="msg">The message to process.</param>
        /// <returns>A reply message in response to the request.</returns>
        [SecurityCritical]
        IMessage SyncProcessMessage(IMessage msg);
    }
    /// <summary>
    /// Provides a way to control asynchronous messages after they have dispatched
    /// </summary>
    [ComVisible(true)]
    public interface IMessageCtrl
    {
        /// <summary>
        /// Cancels an asynchronous call.
        /// </summary>
        /// <param name="msToCancel">The number of milliseconds after which to cancel the message.</param>
        [SecurityCritical]
        void Cancel(int msToCancel);
    }
    /// <summary>
    /// Contains communication data sent between cooperating message sinks.
    /// </summary>
    [ComVisible(true)]
    public interface IMessage
    {
        /// <summary>
        /// Gets an System.Collections.IDictionary that represents a collection of the message's
        /// properties.
        /// </summary>
        /// <returns>A dictionary that represents a collection of the message's properties.</returns>
        IDictionary Properties { get; }
    }
    /// <summary>
    /// contains one property,
    /// System.Runtime.Remoting.Channels.ISecurableChannel.IsSecured, which gets or sets
    /// a Boolean value that indicates whether the current channel is secure.
    /// </summary>
    [ComVisible(true)]
    public interface ISecurableChannel
    {
        /// <summary>
        /// Gets or sets a Boolean value that indicates whether the current channel is secure.
        /// </summary>
        /// <returns>A Boolean value that indicates whether the current channel is secure.</returns>
        bool IsSecured { get; set; }
    }
    [ComVisible(true)]
    public interface IClientChannel
        : IChannelSender, IChannel, ISecurableChannel
    {
    }
    [ComVisible(true)]
    public interface IServerChannel
        : IChannelReceiver, IChannel, ISecurableChannel
    {
        string PortName { get; }
        Uri GetChannelUri();
    }
    [ComVisible(true)]
    public interface IChannelDataStore
    {
        Uri[] ChannelUris { [SecurityCritical] get; }

        object this[object key] { [SecurityCritical] get; [SecurityCritical] set; }
    }
    [ComVisible(true)]
    public interface IClientChannelSinkProvider
    {
        [SecurityCritical]
        IClientChannelSink CreateSink(IChannelSender channel, Uri uri, object remoteChannelData);

        IClientChannelSinkProvider Next { [SecurityCritical] get; [SecurityCritical] set; }
    }
    [ComVisible(true)]
    public interface IClientChannelSink 
        : IChannelSinkBase
        , IMessageSink
    {
        [SecurityCritical]
        Task ProcessRequestAsync(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        Task ProcessResponseAsync(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        Stream GetRequestStream(IMessage msg, ITransportHeaders headers);
        [SecurityCritical]
        void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream);

        IClientChannelSink NextChannelSink { [SecurityCritical] get; }
    }
    [ComVisible(true)]
    public interface IChannelSinkBase
    {
        IDictionary Properties { [SecurityCritical] get; }
    }
    [ComVisible(true)]
    public interface IClientChannelSinkStack : IClientResponseChannelSinkStack
    {
        [SecurityCritical]
        object Pop(IClientChannelSink sink);
        [SecurityCritical]
        void Push(IClientChannelSink sink, object state);
    }
    [ComVisible(true)]
    public interface IClientResponseChannelSinkStack
    {
        [SecurityCritical]
        void AsyncProcessResponse(ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        void DispatchException(Exception e);
        [SecurityCritical]
        void DispatchReplyMessage(IMessage msg);
    }
    [ComVisible(true)]
    public interface ITransportHeaders
    {
        [SecurityCritical]
        IEnumerator GetEnumerator();

        object this[object key] { [SecurityCritical] get; [SecurityCritical] set; }
    }
    [ComVisible(true)]
    public interface IClientFormatterSink 
        : IMessageSink, IClientChannelSink, IChannelSinkBase
    {
    }
}
