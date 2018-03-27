using System;
using System.Collections.Generic;

namespace StreamingServices.Models
{
    /// <summary>
    /// Base class for Web Services
    /// </summary>
    /// <typeparam name="TCommandType"></typeparam>
    public abstract class ClientWebService<TCommandType>
        where TCommandType : struct, IConvertible
    {
        /// <summary>
        /// Uri of Web Service 
        /// </summary>
        protected readonly Uri _uri;

        /// <summary>
        /// API of Web Service 
        /// </summary>
        private Dictionary<TCommandType, dynamic> _commands;

        public ClientWebService(string uriString)
        {
            _uri = new Uri(uriString);
            _commands = new Dictionary<TCommandType, dynamic>();
        }

        /// <summary>
        /// Get command for query to API of web service 
        /// </summary>
        /// <typeparam name="TRequest">Request model</typeparam>
        /// <param name="commandType">Command type</param>
        /// <returns></returns>
        protected TRequest GetCommand<TRequest>(TCommandType commandType)
            where TRequest : class, new()
        {
            if (!_commands.TryGetValue(commandType, out dynamic request))
            {
                request = new TRequest();
                _commands.Add(commandType, request);
            }

            return request;
        }

        /// <summary>
        /// Get custom command for query to API of web service 
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        protected dynamic GetCommand(TCommandType commandType)
        {
            if (!_commands.TryGetValue(commandType, out dynamic request))
            {
                request = GetRequestModel(commandType);
                _commands.Add(commandType, request);
            }

            return request;
        }

        /// <summary>
        /// Method that generate custom request model
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        protected virtual dynamic GetRequestModel(TCommandType commandType)
        {
            throw new NotImplementedException();
        }
    }
}
