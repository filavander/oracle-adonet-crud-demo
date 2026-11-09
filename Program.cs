using System;
using System.Data;
using System.Data.SqlTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace ado1
{
    class Start
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Ado Configuration");

            DatabaseService dbService = new DatabaseService();

            AlterData alterData = new AlterData(dbService);
            await alterData.SelectTable();
            await alterData.DisplayTable();


            await alterData.InserTable(DateTime.Now, "ADO.NET", 4, "Test msg", "666");


            await alterData.SelectTable();
            await alterData.DisplayTable();
            

            await alterData.UpdateTable(3);


            await alterData.SelectTable();
            await alterData.DisplayTable();


            await alterData.DeleteTable(3);


            await alterData.SelectTable();
            await alterData.DisplayTable();


            await alterData.RunInsertProcedure(500, "5 rows insert...");


            await alterData.SelectTable();
            await alterData.DisplayTable();


            await alterData.SelectTableUsingRefCursor(3);
            await alterData.DisplayTable();


            await alterData.DeleteTable(3);


            await alterData.SelectTable();
            await alterData.DisplayTable();
        }
    }
    class DatabaseService
    {
        private readonly string? _connectionString;
        private readonly string? _oracleWalletPath;
        private readonly bool _SslServerDNMatch;
        public DatabaseService()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Start>()
            .Build();

            _oracleWalletPath = configuration["Settings:OracleWalletPath"];
            Console.WriteLine($"Wallet path: {_oracleWalletPath}");
            _SslServerDNMatch = bool.Parse(configuration["Settings:SslServerDNMatch"] ?? "false");
            Console.WriteLine($"SslServerDNMatch: {_SslServerDNMatch}");
            Console.WriteLine();
            

            // string walletPath = @"C:\mirek\OracleCloudVM\OracleDeveloper\Wallet_exercise"; 
            OracleConfiguration.WalletLocation = _oracleWalletPath;
            OracleConfiguration.SSLServerDNMatch = _SslServerDNMatch;

            _connectionString = configuration.GetConnectionString("OracleCloudConnection");
            if(string.IsNullOrEmpty(_connectionString))
            {
                Console.WriteLine("Connection string is not found. Exiting application.");
                Environment.Exit(1);
            }
        }
        public OracleConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }
    }
    class TableRecord
    {
        public int Id {get;set;}
        public DateTime Time {get;set;}
        public string? ProcName {get;set;}
        public int OracleCode {get;set;}
        public string? ErrorMsg {get;set;}
        public string? ErrorLine {get;set;}
        public TableRecord(int id,DateTime time,string procName,int oracleCode,string errorMsg,string errorLine)
        {
            this.Id = id;
            this.Time = time;
            this.ProcName = procName;
            this.OracleCode = oracleCode;
            this.ErrorMsg = errorMsg;
            this.ErrorLine = errorLine;
        }
    }
    class AlterData
    { 
        public List<TableRecord> tableRecord = new List<TableRecord>();
        private DatabaseService _databaseService;
        public AlterData(DatabaseService databaseService)
        {
            this._databaseService = databaseService;
        }
        public async Task SelectTable()
        {
            await using (OracleConnection connection = _databaseService.CreateConnection())
            {
                try{
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection status: [SUCCESS].");
                    // Console.WriteLine($"Select record(s) from the mirek.error_log table.");
                    string sql = "select log_id,log_date,proc_name,oracle_code,error_msg,error_line from mirek.error_log order by log_id";
                    // string sql = "select * from mirek.error_log order by log_id";

                    await using(OracleCommand command = new OracleCommand(sql, connection))
                    {
                        Console.WriteLine($"Selects all columns and all rows from error_log table.");
                        
                        await using(OracleDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                int id_ordinal = reader.GetOrdinal("log_id");
                                int id = reader.IsDBNull(id_ordinal) ? 0 : reader.GetInt32(id_ordinal);
                                int date_ordinal = reader.GetOrdinal("log_date");
                                DateTime date = reader.IsDBNull(date_ordinal) ? DateTime.MinValue : reader.GetDateTime(date_ordinal);
                                int procName_ordinal = reader.GetOrdinal("proc_name");
                                string procName = reader.IsDBNull(procName_ordinal) ? "NO DATA" : reader.GetString(procName_ordinal);
                                int oracleCode_ordinal = reader.GetOrdinal("oracle_code");
                                int oracleCode = reader.IsDBNull(oracleCode_ordinal) ? 0 : reader.GetInt32(oracleCode_ordinal);
                                int errorMsg_ordinal = reader.GetOrdinal("error_msg");
                                string errorMsg = reader.IsDBNull(errorMsg_ordinal) ? "NO DATA" : reader.GetString(errorMsg_ordinal);
                                int errorLine_ordinal = reader.GetOrdinal("error_line");
                                string errorLine = reader.IsDBNull(errorLine_ordinal) ? "NO DATA" : reader.GetString(errorLine_ordinal);

                                TableRecord tRecord = new TableRecord(id,date,procName,oracleCode,errorMsg,errorLine);
                                tableRecord.Add(tRecord);
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    ErrorDisplay(ex);
                }
            }
        }
        public async Task DisplayTable()
        {
            foreach(TableRecord record in tableRecord)
            {
                Console.WriteLine($"{record.Id} {record.Time} {record.ProcName} {record.OracleCode} {record.ErrorMsg} {record.ErrorLine}");
            }
            tableRecord.Clear();
            await Task.Delay(50);
            Console.WriteLine();
        }
        public async Task InserTable(DateTime logDate,string procName,int oracleCode, string errorMsg, string errorLine)
        {
            await using(OracleConnection connection = _databaseService.CreateConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection status: [SUCCESS].");
                    Console.WriteLine($"Insert a record to the mirek.error_log table.");
                    string sql = "insert into mirek.error_log (log_date,proc_name,oracle_code,error_msg,error_line)" +
                    "values (:logDate,:procName,:oracleCode,:errorMsg,:errorLine)";
                    await using (OracleCommand command = new OracleCommand(sql,connection))
                    {
                        OracleParameter paramDate = new OracleParameter();
                        paramDate.ParameterName = "logDate";
                        paramDate.OracleDbType = OracleDbType.Date;
                        paramDate.Value = logDate;
                        command.Parameters.Add(paramDate);

                        OracleParameter param_procName = new OracleParameter();
                        param_procName.ParameterName = "procName";
                        param_procName.OracleDbType = OracleDbType.Varchar2;
                        param_procName.Value = procName;
                        command.Parameters.Add(param_procName);

                        OracleParameter param_oracleCode = new OracleParameter();
                        param_oracleCode.ParameterName = "oracleCode";
                        param_oracleCode.OracleDbType = OracleDbType.Int32;
                        param_oracleCode.Value = oracleCode;
                        command.Parameters.Add(param_oracleCode);

                        OracleParameter param_errorMsg = new OracleParameter();
                        param_errorMsg.ParameterName = "errorMsg";
                        param_errorMsg.OracleDbType = OracleDbType.Varchar2;
                        param_errorMsg.Value = errorMsg;
                        command.Parameters.Add(param_errorMsg);

                        OracleParameter param_errorLine = new OracleParameter();
                        param_errorLine.ParameterName = "errorLine";
                        param_errorLine.OracleDbType = OracleDbType.Varchar2;
                        param_errorLine.Value = errorLine;
                        command.Parameters.Add(param_errorLine);


                        int rowAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Inserted {rowAffected} row(s).");
                        
                    }
                }catch(Exception ex)
                {
                    ErrorDisplay(ex);
                }
            }
            Console.WriteLine();
        }
        public async Task UpdateTable(int hours)
        {
            await using(OracleConnection connection = _databaseService.CreateConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection status: [SUCCESS].");
                    Console.WriteLine($"Update error_line field in the mirek.error_log.");

                    string sql = $"update mirek.error_log set error_line = '777777' where log_date > :logDate";

                    await using(OracleCommand command = new OracleCommand(sql,connection))
                    {
                        OracleParameter param_logDate = new OracleParameter();
                        param_logDate.ParameterName = "logDate";
                        param_logDate.OracleDbType = OracleDbType.Date;
                        // Sposób C: Użycie konstrukcji nowej struktury TimeSpan(godziny, minuty, sekundy)
                        param_logDate.Value = DateTime.Now - new TimeSpan(hours,0,0);
                        command.Parameters.Add(param_logDate);

                        int rowAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Updated {rowAffected} row(s).");
                    }
                }catch(Exception ex)
                {
                    ErrorDisplay(ex);
                }
                Console.WriteLine();
            }
        }
        public async Task DeleteTable(int hours)
        {
            await using(OracleConnection connection = _databaseService.CreateConnection())
            {
                try{
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection status: [SUCCESS].");
                    string sql = $"delete from mirek.error_log where log_date > :log_time";
                    await using (OracleCommand command = new OracleCommand(sql,connection))
                    {
                        OracleParameter param_logTime = new OracleParameter();
                        param_logTime.ParameterName = "log_time";
                        param_logTime.OracleDbType = OracleDbType.Date;
                        param_logTime.Value = DateTime.Now - new TimeSpan(hours,0,0);
                        command.Parameters.Add(param_logTime);

                        Console.WriteLine($"Deleting records from the error_log table.");
                        int rowAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Deleted {rowAffected} row(s).");
                    }
                }catch(Exception ex)
                {
                    ErrorDisplay(ex);
                }
                Console.WriteLine();
            }
        }
        public async Task RunInsertProcedure(int oraCode, string errorMsg)
        {
            await using(OracleConnection connection = _databaseService.CreateConnection())
            {
                try{
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection status: [SUCCESS].");
                    Console.WriteLine($"Inserts into mirek.error_log table 5 rows with PL/SQL mirek.ado_insert_error(...) procedure.");

                    await using(var transaction = await connection.BeginTransactionAsync()){
                        try{
                            await using(OracleCommand command = new OracleCommand("mirek.ado_insert_error",connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;

                                OracleParameter param_oraCode = new OracleParameter();
                                param_oraCode.ParameterName = "oracleCode";
                                param_oraCode.OracleDbType = OracleDbType.Int32;
                                param_oraCode.Value = oraCode;
                                command.Parameters.Add(param_oraCode);

                                OracleParameter param_errorMsg = new OracleParameter();
                                param_errorMsg.ParameterName = "errorMsg";
                                param_errorMsg.OracleDbType = OracleDbType.Varchar2;
                                param_errorMsg.Value = errorMsg;
                                command.Parameters.Add(param_errorMsg);

                                OracleParameter param_rowsCount = new OracleParameter();
                                param_rowsCount.ParameterName = "rowCount";
                                param_rowsCount.OracleDbType = OracleDbType.Int32;
                                param_rowsCount.Direction = System.Data.ParameterDirection.Output;
                                command.Parameters.Add(param_rowsCount);


                                await command.ExecuteNonQueryAsync();
                                if(param_rowsCount.Value is Oracle.ManagedDataAccess.Types.OracleDecimal oracleDec){
                                    int rowsAffected = oracleDec.ToInt32(); 
                                    Console.WriteLine($"Inserted {rowsAffected} row(s).");
                                }
                                Console.WriteLine();
                            }
                            await transaction.CommitAsync();
                        }catch(Exception ex)
                        {
                            ErrorDisplay(ex);
                            await transaction.RollbackAsync();
                        }
                    }
                }catch(Exception ex)
                {
                    ErrorDisplay(ex);
                }
            }
        }
        public async Task SelectTableUsingRefCursor(int log_id)
        {
            await using(OracleConnection connection = _databaseService.CreateConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection status: [SUCCESS].");
                    Console.WriteLine($"Select data from mirek.error_log table with SYS_REFCURSOR.");


                    await using(OracleCommand command = new OracleCommand("mirek.ado_select_with_refcursor_error",connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        OracleParameter param_id = new OracleParameter();
                        param_id.ParameterName = "log_id";
                        param_id.OracleDbType = OracleDbType.Int32;
                        param_id.Value = log_id;
                        command.Parameters.Add(param_id);

                        OracleParameter param_ref_cursor = new OracleParameter();
                        param_ref_cursor.ParameterName = "ref_cursor";
                        param_ref_cursor.OracleDbType = OracleDbType.RefCursor;
                        param_ref_cursor.Direction = ParameterDirection.Output;
                        command.Parameters.Add(param_ref_cursor);

                        await command.ExecuteNonQueryAsync();

                        await using(OracleDataReader reader = ((OracleRefCursor)param_ref_cursor.Value).GetDataReader())
                        {
                            while(await reader.ReadAsync())
                            {
                                int id_ordinal = reader.GetOrdinal("log_id");
                                int id = reader.IsDBNull(id_ordinal) ? 0 : reader.GetInt32(id_ordinal);
                                int log_date_ordinal = reader.GetOrdinal("log_date");
                                DateTime log_date = reader.IsDBNull(log_date_ordinal) ? DateTime.MinValue : reader.GetDateTime(log_date_ordinal);
                                int proc_name_ordinal = reader.GetOrdinal("proc_name");
                                string proc_name = reader.IsDBNull(proc_name_ordinal) ? "No data in sys_refcurssor." : reader.GetString(proc_name_ordinal);
                                int oracle_code_ordinal = reader.GetOrdinal("oracle_code");
                                int oracle_code = reader.IsDBNull(oracle_code_ordinal) ? 0 : reader.GetInt32(oracle_code_ordinal);
                                int error_msg_ordinal = reader.GetOrdinal("error_msg");
                                string error_msg = reader.IsDBNull(error_msg_ordinal) ? "No data in sys_refcurssor." : reader.GetString(error_msg_ordinal);
                                int error_line_ordinal = reader.GetOrdinal("error_line");
                                string error_line = reader.IsDBNull(error_line_ordinal) ? "No data in sys_refcurssor." : reader.GetString(error_line_ordinal);
                                TableRecord tRecord = new TableRecord(id,log_date,proc_name,oracle_code,error_msg,error_line);
                                tableRecord.Add(tRecord);
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    ErrorDisplay(ex);
                }
            }
        }
        private void ErrorDisplay(Exception ex)
        {
            Console.WriteLine($"Database operation error: {ex.Message}.");
            Console.WriteLine($"Database operation status: [FAILED].");
            Console.WriteLine($"Closing the application.");
            Environment.Exit(1);
        }
    }
}
