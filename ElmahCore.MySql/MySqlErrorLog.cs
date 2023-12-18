﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace ElmahCore.MySql
{
    /// <summary>
    ///     An <see cref="ErrorLog" /> implementation that uses MySQL
    ///     as its backing store.
    /// </summary>
    public class MySqlErrorLog : ErrorLog
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MySqlErrorLog" /> class
        ///     using a dictionary of configured settings.
        /// </summary>
        public MySqlErrorLog(IOptions<MySqlErrorLogOptions> option) : this(option.Value.ConnectionString)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MySqlErrorLog" /> class
        ///     to use a specific connection string for connecting to the database.
        /// </summary>
        public MySqlErrorLog(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            ConnectionString = connectionString;
            CreateTableIfNotExist();
        }

        /// <summary>
        ///     Gets the name of this error log implementation.
        /// </summary>
        public override string Name => "MySQL Error Log";

        /// <summary>
        ///     Gets the connection string used by the log to connect to the database.
        /// </summary>
        public virtual string ConnectionString { get; }

        public override async Task LogAsync(Guid id, Error error, CancellationToken cancellationToken)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            var errorXml = ErrorXml.EncodeString(error);

            using (var connection = new MySqlConnection(ConnectionString))
            using (var command = CommandExtension.LogError(id, ApplicationName, error.HostName, error.Type,
                error.Source, error.Message, error.User, error.StatusCode, error.Time, errorXml))
            {
                await connection.OpenAsync(cancellationToken);
                command.Connection = connection;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public override async Task<ErrorLogEntry?> GetErrorAsync(string id, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (id.Length == 0)
            {
                throw new ArgumentException(null, "id");
            }

            Guid errorGuid;

            try
            {
                errorGuid = new Guid(id);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(e.Message, "id", e);
            }

            string? errorXml;

            using (var connection = new MySqlConnection(ConnectionString))
            using (var command = CommandExtension.GetErrorXml(ApplicationName, errorGuid))
            {
                command.Connection = connection;
                await connection.OpenAsync(cancellationToken);
                errorXml = (string?) await command.ExecuteScalarAsync(cancellationToken);
            }

            if (errorXml == null)
            {
                return null;
            }

            var error = ErrorXml.DecodeString(errorXml);
            return new ErrorLogEntry(this, id, error);
        }

        public override async Task<int> GetErrorsAsync(string? searchText, List<ErrorLogFilter> filters, int errorIndex, int pageSize,
            ICollection<ErrorLogEntry> errorEntryList, CancellationToken cancellationToken)
        {
            if (errorIndex < 0)
            {
                throw new ArgumentOutOfRangeException("errorIndex", errorIndex, null);
            }

            if (pageSize < 0)
            {
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);
            }

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = CommandExtension.GetErrorsXml(ApplicationName, errorIndex, pageSize))
                {
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var id = reader.GetGuid(0);
                            var xml = reader.GetString(1);
                            var error = ErrorXml.DecodeString(xml);
                            errorEntryList.Add(new ErrorLogEntry(this, id.ToString(), error));
                        }
                    }
                }

                return await GetTotalErrorsXml(connection, cancellationToken);
            }
        }

        /// <summary>
        ///     Creates the necessary tables used by this implementation
        /// </summary>
        private void CreateTableIfNotExist()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var databaseName = connection.Database;

                using (var commandCheck = CommandExtension.CheckTable(databaseName))
                {
                    commandCheck.Connection = connection;
                    var exists = Convert.ToBoolean(commandCheck.ExecuteScalar());

                    if (!exists)
                    {
                        using (var commandCreate = CommandExtension.CreateTable())
                        {
                            commandCreate.Connection = connection;
                            commandCreate.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private async Task<int> GetTotalErrorsXml(MySqlConnection connection, CancellationToken cancellationToken)
        {
            using (var command = CommandExtension.GetTotalErrorsXml(ApplicationName))
            {
                command.Connection = connection;
                return (int)(await command.ExecuteScalarAsync(cancellationToken))!;
            }
        }
    }
}