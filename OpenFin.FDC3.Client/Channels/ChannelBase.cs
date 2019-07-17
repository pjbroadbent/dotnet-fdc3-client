﻿using OpenFin.FDC3.Context;
using OpenFin.FDC3.Events;
using OpenFin.FDC3.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenFin.FDC3.Channels
{
    /// <summary>
    /// The base class for context channels.
    /// </summary>
    public abstract class ChannelBase
    {
        public string ChannelId { get; private set; }
        public readonly ChannelType ChannelType;

        protected ChannelBase(string channelId, ChannelType channelType)
        {
            ChannelId = channelId;
            ChannelType = channelType;
        }

        /// <summary>
        /// Returns a collection of all windows connected to this channel.
        /// </summary>
        /// <returns></returns>
        public Task<List<Identity>> GetMembersAsync()
        {
            return Connection.GetChannelMembersAsync(this.ChannelId);
        }

        /// <summary>
        /// Returns the last context that was broadcast on this channel. All channels initially have no context, until a window is added to the channel and then broadcasts.
        /// The context of a channel will be captured regardless of how it's set on the channel.
        /// </summary>
        /// <returns>
        /// If no contexts have been passed to the channel this method returns null. Context is set to its initial context-less state when a channel is cleared of all windows.
        /// </returns>
        public Task<ContextBase> GetCurrentContextAsync()
        {
            return Connection.GetCurrentContextAsync(this.ChannelId);
        }

        /// <summary>
        /// Adds the provided window to this channel.
        /// If this channel has a current context, the context will be passed to the window through its context listener upon joining this channel.
        /// </summary>
        /// <param name="identity">The window to be added to this channel</param>
        /// <returns></returns>
        public Task JoinAsync(Identity identity = null)
        {
            return Connection.JoinChannelAsync(this.ChannelId, identity);
        }

        public Task BroadcastAsync(ContextBase context)
        {
            return Connection.ChannelBroadcastAsync(this.ChannelId, context);
        }

        public Task AddContextListenerAsync(Action<ContextBase> listener)
        {
            var channelContextListener = new ChannelContextListener
            {
                Channel = this,
                Handler = listener
            };

            var hasAny = FDC3Handlers.HasContextListener(this.ChannelId);

            FDC3Handlers.ChannelContextHandlers.Add(channelContextListener);

            if (!hasAny)
                return Connection.AddChannelContextListenerAsync(this.ChannelId);
            else
                return new TaskCompletionSource<object>(null).Task;
        }

        public void RemoveContextListener(ChannelContextListener listener)
        {
            FDC3Handlers.ChannelContextHandlers.RemoveAll(x => x.Channel.ChannelId == listener.Channel.ChannelId);
            Connection.RemoveChannelContextListenerAsync(listener);
        }

        public Task AddEventListenerAsync(FDC3EventType eventType, Action<FDC3Event> eventHandler)
        {
            var hasAny = FDC3Handlers.HasEventListener(this.ChannelId);            
            FDC3Handlers.FDC3ChannelEventHandlers[this.ChannelId].Add(eventType, eventHandler);

            if (!hasAny)
                return Connection.AddChannelEventListenerAsync(this.ChannelId, eventType);
            else
                return new TaskCompletionSource<object>(null).Task;
        }

        public Task UnsubscribeEventListenerAsync(FDC3EventType eventType, Action<FDC3Event> eventHandler)
        {
            FDC3Handlers.FDC3ChannelEventHandlers[this.ChannelId][eventType] -= eventHandler;
            FDC3Handlers.FDC3ChannelEventHandlers[this.ChannelId].Remove(eventType);

            //if (FDC3Handlers.FDC3EventHandlers[this.ChannelId][eventType])
            //{
            //    return Connection.RemoveFDC3EventListenerAsync(this.ChannelId, eventType);
            //}
            //else
            //{
            //    return new TaskCompletionSource<object>().Task;
            //}

            return null;
        }
    }
}